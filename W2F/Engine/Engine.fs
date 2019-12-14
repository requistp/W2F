namespace rec Engine
open CalendarTimings
open CommonFunctions
open EngineTypes
open MBrace.FsPickler
open System
open System.IO

//----------------------------------------------------------------------------------------------------------
module rec Entities =

    let private addComponents (start:Map<ComponentID,AbstractComponent>) (cts:AbstractComponent[]) =
        cts
        |> Array.fold (fun (m:Map<ComponentID,AbstractComponent>) (c:AbstractComponent) -> 
            match (m.ContainsKey c.ID) with 
            | true -> m.Remove(c.ID).Add(c.ID,c)
            | false -> m.Add(c.ID,c)
            ) start

    let private addComponentTypes (start:Map<byte,ComponentID[]>) (cts:AbstractComponent[]) =
        cts
        |> Array.fold (fun (m:Map<byte,ComponentID[]>) (c:AbstractComponent) ->
            map_AppendToArray_Unique m c.ComponentType c.ID
            ) start

    let private addLocation (start:Map<Location,ComponentID[]>) (cts:AbstractComponent[]) = 
        cts 
        |> filterForLocationType
        |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:AbstractComponent_WithLocation) ->
            map_AppendToArray_Unique m f.Location f.ID
            ) start

    let private filterForLocationType (cts:AbstractComponent[]) = 
        cts 
        |> Array.filter (fun c -> c.ComponentType = 0uy)
        |> Array.map (fun c -> c :?> AbstractComponent_WithLocation)

    let private moveLocations (ent:Entities) (cts:AbstractComponent[]) = 
        cts 
        |> filterForLocationType
        |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:AbstractComponent_WithLocation) ->
            let oldF = ent.Components.Item f.ID :?> AbstractComponent_WithLocation
            match (oldF = f) with
            | true -> m
            | false -> map_AppendToArray_Unique (map_RemoveFromArray m oldF.Location oldF.ID) f.Location f.ID
            ) ent.Locations

    let private removeComponents (start:Map<ComponentID,AbstractComponent>) (cids:ComponentID[]) =
        cids
        |> Array.fold (fun (m:Map<ComponentID,AbstractComponent>) cid -> m.Remove cid) start

    let private removeComponentTypes (start:Map<byte,ComponentID[]>) (cts:AbstractComponent[]) =
        cts
        |> Array.fold (fun (m:Map<byte,ComponentID[]>) c -> 
            map_RemoveFromArray m c.ComponentType c.ID
            ) start

    let private removeLocation (start:Map<Location,ComponentID[]>) (cts:AbstractComponent[]) = 
        cts 
        |> filterForLocationType
        |> Array.fold (fun (m:Map<Location,ComponentID[]>) f ->
            map_RemoveFromArray m f.Location f.ID
            ) start

    let private updateComponents_Internal (ent:Entities) (cts:AbstractComponent[]) = 
        {
            ent with
                Components = addComponents ent.Components cts
                Locations = moveLocations ent cts
        }

    //----------------------------------------------------------------------------------------------------------------------------

    let copy (game:Game) (eid:EntityID) : AbstractComponent[] =
        let newEID = game.Entities.NewEntityID
        let firstCID = game.Entities.NewComponentID
        get game.Entities eid
        |> Array.mapi (fun i (c:AbstractComponent) -> c.Copy (firstCID+i) newEID )

    let create (game:Game) (cts:AbstractComponent[]) = 
        let raiseComponentEvents (cts2:AbstractComponent[]) (g2:Game) =
            cts2 |> Array.fold (fun g c -> Events.execute (EngineEvent_ComponentAdded c) g) g2
        {
            game with
                Entities = 
                    {
                        Components = addComponents game.Entities.Components cts
                        ComponentTypes = addComponentTypes game.Entities.ComponentTypes cts
                        Entities = game.Entities.Entities.Add(game.Entities.NewEntityID, cts |> Array.map (fun c -> c.ID))
                        Locations = addLocation game.Entities.Locations cts
                        MaxComponentID = game.Entities.MaxComponentID + ComponentID(uint32 cts.Length)
                        MaxEntityID = game.Entities.NewEntityID
                    }
                //Log = Logging.log1 game.Log "Ok" "Game" "createEntity" (game.Entities.MaxEntityID + 1u) None (Some (cts.Length.ToString() + " components"))
        }
        |> Events.execute (EngineEvent_EntityCreated cts.[0].EntityID)
        |> raiseComponentEvents cts

    let exists (ent:Entities) eid = ent.Entities.ContainsKey eid

    let get (ent:Entities) eid = ent.Entities.Item eid |> getComponents_ByIDs ent

    let getAtLocation (ent:Entities) (location:Location) = 
        ent.Locations.Item(location)
        |> getComponents_ByIDs ent
        |> Array.map (fun c -> c :?> AbstractComponent_WithLocation)

    let getAtLocationWithComponent (ent:Entities) (ct:ComponentTypeID) (excludeEID:EntityID option) (location:Location) =
        location
        |> getEntityIDsAtLocation ent
        |> Array.filter (fun eid -> excludeEID.IsNone || eid <> excludeEID.Value) // Not excluded or not me
        |> Array.choose (tryGetComponent ent ct)

    let getComponent (ent:Entities) (ct:ComponentTypeID) (eid:EntityID) =
        get ent eid 
        |> Array.find (fun (c:AbstractComponent) -> c.ComponentType = ct)

    let getComponent_ByID (ent:Entities) cid = ent.Components.Item cid

    let getComponents_ByIDs (ent:Entities) (cids:ComponentID[]) : AbstractComponent[] = 
        cids 
        |> Array.map (getComponent_ByID ent)

    let getComponents_OfType (ent:Entities) ct =  
        ent.ComponentTypes.Item ct 
        |> getComponents_ByIDs ent

    let getComponentTypes (ent:Entities) (eid:EntityID) =
        eid
        |> get ent
        |> Array.map (fun c -> c.ComponentType) 

    let getEntityIDsAtLocation (ent:Entities) location =
        ent.Locations.Item(location) 
        |> getComponents_ByIDs ent
        |> Array.map (fun c -> c.EntityID) 

    let getLocation (ent:Entities) (eid:EntityID) = 
        ((getComponent ent 0uy eid) :?> AbstractComponent_WithLocation).Location 

    let getLocationMap (ent:Entities) = 
        ent.Locations
        |> Map.map (fun _ cids -> cids |> getComponents_ByIDs ent)

    let isLocationImpassible (ent:Entities) (excludeEID:EntityID option) (location:Location) =
        location
        |> getAtLocation ent
        |> Array.filter (fun c -> excludeEID.IsNone || c.EntityID <> excludeEID.Value)
        |> Array.exists (fun c -> not c.IsPassable)

    let remove (eid:EntityID) (game:Game) = 
        {
            game with 
                Entities = 
                    {
                        game.Entities with
                            Components = removeComponents game.Entities.Components (game.Entities.Entities.Item eid)
                            ComponentTypes = removeComponentTypes game.Entities.ComponentTypes (Entities.get game.Entities eid)
                            Entities = game.Entities.Entities.Remove eid
                            Locations = removeLocation game.Entities.Locations (Entities.get game.Entities eid)
                    }
                Log = if game.Settings.LoggingOn then Logging.log1 game.Log "Ok" "Game" "removeEntity" eid None None else game.Log
        }
        |> Events.execute (EngineEvent_EntityCreated eid)

    let tryGetComponent (ent:Entities) (ct:ComponentTypeID) (eid:EntityID) : Option<AbstractComponent> = 
        match (get ent eid) |> Array.filter (fun c -> c.ComponentType = ct) with
        | [||] -> None
        | cts -> Some cts.[0]

    let updateComponent (game:Game) (c:AbstractComponent) (logstring:string option) = 
        {
            game with 
                Entities = updateComponents_Internal game.Entities [|c|]
                Log = if game.Settings.LoggingOn then (if logstring.IsSome then Logging.log game.Log logstring.Value else game.Log) else game.Log
        }

    let updateComponents (game:Game) (cts:AbstractComponent[]) (logstring:string option) = 
        {
            game with 
                Entities = updateComponents_Internal game.Entities cts
                Log = if game.Settings.LoggingOn then (if logstring.IsSome then Logging.log game.Log logstring.Value else game.Log) else game.Log
        }


//----------------------------------------------------------------------------------------------------------
module Events = 
    let private add (el:EventListener) (game:Game) : Game =
        { 
            game with 
                EventListeners = map_AppendToArray_NonUnique game.EventListeners el.Type el
                Log = if game.Settings.LoggingOn then (Logging.log game.Log (sprintf "%-3s | %-20s -> %s" "Ok" "Event Listener" el.Description)) else game.Log
        }        
    
    let execute (e:AbstractEventData) (game:Game) : Game =
        match (game.EventListeners.ContainsKey e.Type) with
        | false -> if game.Settings.LoggingOn then ({ game with Log = Logging.log game.Log (sprintf "%-3s | %-20s -> %s" "" "<no listeners>" e.Description) } ) else game
        | true -> 
            game.EventListeners.Item(e.Type)
            |> Array.fold (fun (g:Game) el -> el.Action g e) game

    let registerListeners (els:EventListener[]) (game:Game) : Game =
        els 
        |> Array.fold (fun g el -> add el g) game
    

//----------------------------------------------------------------------------------------------------------
module Log = 
    let write (game:Game) = 
        //match game.Settings.LoggingOn with 
        //| false -> game
        //| true ->           
        // I left this remmed out so I can see if something is writing to the log
            Async.Ignore
            (
                game.Log |> Array.iter (fun s -> Logging.writeLog (sprintf "%7i | %s" game.Round.ToUint32 s))
            ) |> ignore
            { game with Log = Array.empty }
    

//------------------------------------------------------------------------------------------------------------
module Persistance =    
    let private savePath = "./saves"
    let private binarySerializer = FsPickler.CreateBinarySerializer()
    let private xmlSerializer = FsPickler.CreateXmlSerializer(indent = true)
        
    let private inputStream format filename = 
        let str = 
            match format with
            | Binary -> new StreamReader(File.OpenRead(savePath + "/" + filename + format.Ext))
            | XML -> new StreamReader(File.OpenRead(savePath + "/" + filename + format.Ext))
        str.BaseStream
        
    let private outputStream (format:SaveGameFormats) (round:RoundNumber) = 
        if Directory.Exists(savePath) |> not then Directory.CreateDirectory(savePath) |> ignore
        let str = 
            let filename = savePath + "/" + "Save_" +  (DateTime.Now.ToString("yyyyMMddHHmm")) + "_r" + round.ToUint32.ToString() + format.Ext
            match format with
            | Binary -> new StreamWriter(File.OpenWrite(filename))
            | XML -> new StreamWriter(File.OpenWrite(filename))
        str.AutoFlush <- true
        str.BaseStream
    
    let load (format:SaveGameFormats) (filename:string) =
        match format with
        | Binary -> binarySerializer.Deserialize<Game> (inputStream format filename)
        | XML -> xmlSerializer.Deserialize<Game> (inputStream format filename)
    
    let save (game:Game) = 
        Async.Ignore
        (
            match game.Settings.SaveFormat with
            | Binary -> binarySerializer.Serialize(outputStream game.Settings.SaveFormat game.Round, game)
            | XML -> xmlSerializer.Serialize(outputStream game.Settings.SaveFormat game.Round, game)
        ) |> ignore
        game
    

//------------------------------------------------------------------------------------------------------------
module Round = 
    let increment (game:Game) : Game =
        {
            game with 
                Round = game.Round + 1u 
        }


//---------------------------------------------------------------------------------------------------------------------------
module Scheduler = 
    let private add (game:Game) (isNew:bool) (se:ScheduledEvent) = 
        let scheduledRound = 
            game.Round +
            match (isNew && se.ScheduleType = RepeatIndefinitely) with
            | true -> TimingOffset se.Frequency
            | false -> se.Frequency
        { 
            game with 
                ScheduledEvents = map_AppendToArray_NonUnique game.ScheduledEvents scheduledRound se 
                Log = if game.Settings.LoggingOn then (Logging.log1 game.Log "Ok" "Scheduler.add" se.Event.Description se.Event.EntityID None (Some ("Round:" + scheduledRound.ToUint32.ToString()))) else game.Log
        }

    let addToSchedule (se:ScheduledEvent) (game:Game) = add game true se
    
    let executeSchedule (game:Game) =
        match (game.ScheduledEvents.ContainsKey game.Round) with
        | false -> game
        | true ->
            game.ScheduledEvents.Item(game.Round)
            |> Array.fold (fun (g:Game) se ->
                match (Engine.Entities.exists g.Entities se.Event.EntityID) with
                | false -> g
                | true -> 
                    g
                    |> Events.execute se.Event 
                    |> function // Reschedule
                    | g when se.ScheduleType = RunOnce -> g
                    | g -> 
                        match se.ScheduleType with
                        | RepeatFinite x when x = 1u -> g
                        | RepeatFinite x -> add g false { se with ScheduleType = RepeatFinite (x - 1u) }
                        | RepeatIndefinitely -> add g false se
                ) game
            |> function
            | g -> { g with ScheduledEvents = g.ScheduledEvents |> Map.filter (fun k _ -> k > g.Round) }
    

//---------------------------------------------------------------------------------------------------------------------------
module Settings = 
    let saveAfterRound (game:Game) : Game = 
        match game.Settings.SaveEveryRound with
        | false -> game
        | true -> Persistance.save game
        
    let setMapSize (l:Location) (game:Game) : Game = { game with MapSize = l }
    
    let setRenderMode (mode:RenderTypes) (game:Game) : Game = { game with Settings = { game.Settings with RenderType = mode } }
    
    let setSaveEveryRound (toggle:bool) (game:Game) : Game = { game with Settings = { game.Settings with SaveEveryRound = toggle } }
    
    let setSaveFormat (format:SaveGameFormats) (game:Game) : Game = { game with Settings = { game.Settings with SaveFormat = format } }



