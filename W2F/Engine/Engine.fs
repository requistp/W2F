namespace rec Engine
open CalendarTimings
open CommonFunctions
open EngineTypes
open MBrace.FsPickler
open System
open System.IO

//----------------------------------------------------------------------------------------------------------
module Entities =

    let private addComponents (start:Map<ComponentID,AbstractComponent>) (cts:AbstractComponent[]) =
        cts
        |> Array.fold (fun (m:Map<ComponentID,AbstractComponent>) (c:AbstractComponent) -> m.Add(c.ID,c) ) start

    let addComponentTypes (start:Map<byte,ComponentID[]>) (cts:AbstractComponent[]) =
        cts 
        |> Array.groupBy (fun c -> c.ComponentType)
        |> Array.fold (fun (m:Map<byte,ComponentID[]>) (ctid,cts2) -> 
            let a = Array.map (fun (c:AbstractComponent) -> c.ID) cts2
            match m.ContainsKey(ctid) with
            | false -> m.Add(ctid,a)
            | true -> 
                let a2 = Array.append (m.Item(ctid)) a
                m.Remove(ctid).Add(ctid,a2)
            ) start

    let addEntities (start:Map<EntityID,ComponentID[]>) (cts:AbstractComponent[]) =
        cts 
        |> Array.groupBy (fun c -> c.EntityID)
        |> Array.fold (fun (m:Map<EntityID,ComponentID[]>) (eid,cts2) ->
            m.Add(eid,cts2|>Array.map(fun c -> c.ID))
            ) start 
            
    let addLocation (start:Map<Location,ComponentID[]>) (cts:AbstractComponent[]) = 
        cts 
        |> filterForLocationType
        |> Array.groupBy (fun (c:AbstractComponent_WithLocation) -> c.Location)
        |> Array.fold (fun (m:Map<Location,ComponentID[]>) (l,a0) ->
            let a = a0 |> Array.map (fun c -> c.ID)
            match m.ContainsKey(l) with
            | false -> m.Add(l,a)
            | true -> 
                let a2 = Array.append (m.Item l) a
                m.Remove(l).Add(l,a2)
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
        get game.Entities eid
        |> Array.mapi (fun i (c:AbstractComponent) -> c.Copy (game.Entities.NewComponentID+i) (game.Entities.NewEntityID) )

    let create (game:Game) (cts:AbstractComponent[]) = 
        let raiseComponentEvents (g2:Game) =
            cts 
            |> Array.fold (fun g c -> Events.execute (EngineEvent_ComponentAdded c) g) g2
        let raiseCreateEntityEvents (g2:Game) =
            cts 
            |> Array.groupBy (fun c -> c.EntityID)
            |> Array.fold (fun g (eid,_) -> Events.execute (EngineEvent_EntityCreated eid) g) g2
        let maxeid = EntityID (cts |> Array.map (fun c -> c.EntityID.ToUint32) |> Array.max)
        {
            game with
                Entities = 
                    { 
                        Components = addComponents game.Entities.Components cts
                        ComponentTypes = addComponentTypes game.Entities.ComponentTypes cts
                        Entities = addEntities game.Entities.Entities cts
                        Locations = addLocation game.Entities.Locations cts
                        MaxComponentID = game.Entities.MaxComponentID + cts.Length
                        MaxEntityID = maxeid
                    }
        }
        |> Engine.Log.append (Logging.format1 "Ok" "Engine.Entities" "create" maxeid None (Some (cts.Length.ToString() + " components")))
        |> raiseCreateEntityEvents
        |> raiseComponentEvents

    let create2 (game:Game) (ctss:AbstractComponent[][]) = 
        ctss 
        |> Array.collect (fun c -> c)
        |> create game

    let exists (ent:Entities) eid = ent.Entities.ContainsKey eid

    let get (ent:Entities) eid = ent.Entities.Item eid |> getComponents_ByIDs ent

    let get_AtLocation (ent:Entities) (location:Location) = 
        ent.Locations.Item(location)
        |> getComponents_ByIDs ent
        |> Array.map (fun c -> c :?> AbstractComponent_WithLocation)

    let get_AtLocationWithComponent (ent:Entities) (ct:ComponentTypeID) (excludeEID:EntityID option) (location:Location) =
        location
        |> get_EntityIDsAtLocation ent
        |> Array.filter (fun eid -> excludeEID.IsNone || eid <> excludeEID.Value) // Not excluded or not me
        |> Array.choose (tryGet_Component ent ct)

    let get_Component (ent:Entities) (ct:ComponentTypeID) (eid:EntityID) =
        get ent eid 
        |> Array.find (fun (c:AbstractComponent) -> c.ComponentType = ct)

    let get_Component_ByID (ent:Entities) cid = ent.Components.Item cid

    let getComponents_ByIDs (ent:Entities) (cids:ComponentID[]) : AbstractComponent[] = 
        cids 
        |> Array.map (get_Component_ByID ent)

    let get_Components_OfType (ent:Entities) ct = 
        match ent.ComponentTypes.ContainsKey ct with
        | false -> [||]
        | true ->
            ent.ComponentTypes.Item ct 
            |> getComponents_ByIDs ent

    let get_ComponentTypes (ent:Entities) (eid:EntityID) =
        eid
        |> get ent
        |> Array.map (fun c -> c.ComponentType) 

    let get_EntityIDsAtLocation (ent:Entities) location =
        ent.Locations.Item(location) 
        |> getComponents_ByIDs ent
        |> Array.map (fun c -> c.EntityID) 

    let get_Location (ent:Entities) (eid:EntityID) = 
        ((get_Component ent 0uy eid) :?> AbstractComponent_WithLocation).Location 

    let get_Locations (ent:Entities) = 
        ent.Locations
        |> Map.map (fun _ cids -> cids |> getComponents_ByIDs ent)
        
    let isLocationPassible (ent:Entities) (excludeEID:EntityID option) (location:Location) =
        location
        |> get_AtLocation ent
        |> Array.filter (fun c -> excludeEID.IsNone || c.EntityID <> excludeEID.Value)
        |> Array.forall (fun c -> c.IsPassable)

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
        }
        |> Engine.Log.append (Logging.format1 "Ok" "Game" "removeEntity" eid None None)
        |> Events.execute (EngineEvent_EntityRemoved eid)

    let tryGet_Component (ent:Entities) (ct:ComponentTypeID) (eid:EntityID) : Option<AbstractComponent> = 
        match (get ent eid) |> Array.filter (fun c -> c.ComponentType = ct) with
        | [||] -> None
        | cts -> Some cts.[0]

    let updateComponent (game:Game) (c:AbstractComponent) (logstring:string option) = 
        {
            game with 
                Entities = updateComponents_Internal game.Entities [|c|]
        }
        |> Engine.Log.appendo logstring

    let updateComponents (game:Game) (cts:AbstractComponent[]) (logstring:string option) = 
        {
            game with 
                Entities = updateComponents_Internal game.Entities cts
        }
        |> Engine.Log.appendo logstring


//----------------------------------------------------------------------------------------------------------
module Events = 

    let private add (el:EventListener) (game:Game) : Game =
        { 
            game with 
                EventListeners = map_AppendToArray_NonUnique game.EventListeners el.Type el
        }
        |> Engine.Log.append (sprintf "%-3s | %-20s -> %s" "Ok" "Event Listener" el.Description)
    
    let execute (e:AbstractEventData) (game:Game) : Game =
        match (game.EventListeners.ContainsKey e.Type) with
        | false -> Engine.Log.append (sprintf "%-3s | %-20s -> %s" "" "<no listeners>" e.Description) game
        | true -> 
            game.EventListeners.Item(e.Type)
            |> Array.fold (fun (g:Game) el -> el.Action g e) game

    let registerListeners (els:EventListener[]) (game:Game) : Game =
        els 
        |> Array.fold (fun g el -> add el g) game
    

//----------------------------------------------------------------------------------------------------------
module GameLoop = 
    
    let exit (game:Game) : Game =
        { 
            game with ExitGame = true 
        }

    let setSteps (steps:GameLoopStep[]) (game:Game) =
        {
            game with GameLoopSteps = steps
        }

    let rec start (game:Game) : Game = 
        let executeGameLoopSteps (g:Game) : Game =
            g.GameLoopSteps
            |> Array.fold (fun g1 step -> step g1) g
        // Start
        game
        |> function
        | g when g.ExitGame -> 
            g
            |> Engine.Log.write
            |> Engine.Persistance.save
        | g ->
            g
            |> executeGameLoopSteps
            |> Engine.Scheduler.executeSchedule
            |> Engine.Log.write
            |> Engine.Settings.saveAfterRound
            |> Engine.Round.increment
            |> start


//----------------------------------------------------------------------------------------------------------
module Log = 

    let append (s:string) (game:Game) = 
        match game.Settings.LoggingOn,s with
        | false,_  -> game
        | true ,"" -> game
        | true ,_  -> 
            {
                game with Log = Array.append game.Log [|s|]
            }
            |> checkDumpLog

    let appendo (s:string option) (game:Game) = 
        match game.Settings.LoggingOn,s.IsNone with
        | false,_    -> game
        | true ,true -> game
        | true ,false when s.Value = "" -> game
        | true ,false -> 
            {
                game with Log = Array.append game.Log [|s.Value|]
            }
            |> checkDumpLog

    let checkDumpLog (game:Game) = 
        match game.Log.Length >= game.Settings.LogLengthMax with
        | false -> game
        | true -> write game

    let write (game:Game) = 
        //match game.Settings.LoggingOn with 
        //| false -> game
        //| true ->           
        // I left this on so I can see if something is writing to the log
        match game.Log with 
        | [||] -> game
        | _ ->
            Async.Ignore
            (
                game.Log 
                |> Array.iter (fun s -> Logging.writeLog (sprintf "%7i | %s" game.Round.ToUint32 s))
            ) 
            { 
                game with Log = Array.empty 
            }
    

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
        | Binary -> binarySerializer.Deserialize<GameSave> (inputStream format filename)
        | XML -> xmlSerializer.Deserialize<GameSave> (inputStream format filename)
        |> fun gs -> gs.toGame
        |> function
            | g when not g.Settings.SaveComponentsOnly -> g
            | g -> 
                let cts = g.Entities.Components |> map_ValuesToArray
                {
                    g with 
                        Entities = 
                            {
                                g.Entities with
                                    ComponentTypes = Entities.addComponentTypes Map.empty cts
                                    Entities = Entities.addEntities Map.empty cts
                                    Locations = Entities.addLocation Map.empty cts
                            }
                }

    let save (game:Game) = 
        Async.Ignore
        (
            let save = 
                match game.Settings.SaveComponentsOnly with
                | false -> game.toSave
                | true ->
                    {
                        game.toSave with 
                            Entities = 
                                {
                                    Entities.empty with
                                        Components = game.Entities.Components
                                        MaxComponentID = game.Entities.MaxComponentID
                                        MaxEntityID = game.Entities.MaxEntityID
                                }
                    }
            match game.Settings.SaveFormat with
            | Binary -> binarySerializer.Serialize(outputStream game.Settings.SaveFormat game.Round, save)
            | XML -> xmlSerializer.Serialize(outputStream game.Settings.SaveFormat game.Round, save)
        )
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
        }
        |> Engine.Log.append (Logging.format1 "Ok" "Scheduler.add" se.Event.Description se.Event.EntityID None (Some ("Round:" + scheduledRound.ToUint32.ToString())))

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

    let saveAfterRound (game:Game) = 
        match game.Settings.SaveEveryRound with
        | false -> game
        | true -> Persistance.save game
    
    let setLogging (toggle:bool) (game:Game) = { game with Settings = { game.Settings with LoggingOn = toggle } }

    let setMapSize (l:Location) (game:Game) = { game with MapSize = l }
    
    let setRenderMode (mode:RenderTypes) (game:Game) = { game with Settings = { game.Settings with RenderType = mode } }
    
    let setSaveEveryRound (toggle:bool) (game:Game) = { game with Settings = { game.Settings with SaveEveryRound = toggle } }
    
    let setSaveFormat (format:SaveGameFormats) (game:Game) = { game with Settings = { game.Settings with SaveFormat = format } }

    let setSaveComponentsOnly (toggle:bool) (game:Game) = { game with Settings = { game.Settings with SaveComponentsOnly = toggle } }
        
    let setSaveOnExitGameLoop (toggle:bool) (game:Game) = { game with Settings = { game.Settings with SaveOnExitGameLoop = toggle } }



