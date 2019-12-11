﻿namespace Game 
open CommonFunctions
open CommonTypes
open Component
open GameTypes
open FormComponent

module rec Entities =

    let private addComponents (start:Map<ComponentID,Component>) (cts:Component[]) =
        cts
        |> Array.fold (fun (m:Map<ComponentID,Component>) (c:Component) -> 
            match (m.ContainsKey (getComponentID c)) with 
            | true -> m.Remove(getComponentID c).Add(getComponentID c,c)
            | false -> m.Add(getComponentID c,c)
            ) start

    let private addComponentTypes (start:Map<ComponentTypes,ComponentID[]>) (cts:Component[]) =
        cts
        |> Array.fold (fun (m:Map<ComponentTypes,ComponentID[]>) (c:Component) ->
            map_AppendToArray_Unique m (getComponentType c) (getComponentID c)
            ) start

    let private addLocation (start:Map<Location,ComponentID[]>) (cts:Component[]) = 
        cts 
        |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)
        |> Array.map ToForm
        |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:FormComponent) ->
            map_AppendToArray_Unique m f.Location f.ID
            ) start

    let private moveLocations (ent:Entities) (cts:Component[]) = 
        cts 
        |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)
        |> Array.map ToForm
        |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:FormComponent) ->
            let (Form oldF) = ent.Components.Item f.ID
            match (oldF = f) with
            | true -> m
            | false -> map_AppendToArray_Unique (map_RemoveFromArray m oldF.Location oldF.ID) f.Location f.ID
            ) ent.Locations

    let private removeComponents (start:Map<ComponentID,Component>) (cids:ComponentID[]) =
        cids
        |> Array.fold (fun (m:Map<ComponentID,Component>) cid -> m.Remove cid) start

    let private removeComponentTypes (start:Map<ComponentTypes,ComponentID[]>) (cts:Component[]) =
        cts
        |> Array.fold (fun (m:Map<ComponentTypes,ComponentID[]>) c -> 
            map_RemoveFromArray m (getComponentType c) (getComponentID c)
            ) start

    let private removeLocation (start:Map<Location,ComponentID[]>) (cts:Component[]) = 
        cts 
        |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)
        |> Array.map ToForm
        |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:FormComponent) ->
            map_RemoveFromArray m f.Location f.ID
            ) start

    let private updateComponent_Internal (ent:Entities) (c:Component) = updateComponents_Internal ent [|c|]

    let private updateComponents_Internal (ent:Entities) (cts:Component[]) = 
        {
            ent with
                Components = addComponents ent.Components cts
                Locations = moveLocations ent cts
        }

//----------------------------------------------------------------------------------------------------------------------------

    let create (game:Game) (cts:Component[]) = 
        cts 
        |> Array.fold (fun g c -> Events.execute (ComponentAdded c) g)
            {
                game with
                    Entities = 
                        {
                            Components = addComponents game.Entities.Components cts
                            ComponentTypes = addComponentTypes game.Entities.ComponentTypes cts
                            Entities = game.Entities.Entities.Add(game.Entities.NewEntityID, Array.map getComponentID cts)
                            Locations = addLocation game.Entities.Locations cts
                            MaxComponentID = game.Entities.MaxComponentID + ComponentID(uint32 cts.Length)
                            MaxEntityID = game.Entities.NewEntityID
                        }
                    Log = Logging.log1 game.Log "Ok" "Game" "createEntity" (game.Entities.MaxEntityID + 1u) None (Some (cts.Length.ToString() + " components"))
            }

    let exists (ent:Entities) eid = ent.Entities.ContainsKey eid

    let get (ent:Entities) eid = ent.Entities.Item eid |> getComponents_ByIDs ent

    let getAtLocationWithComponent (ent:Entities) (ct:ComponentTypes) (typeConverter:Component->'a) (excludeEID:EntityID option) (location:Location) = 
        location
        |> getEntityIDsAtLocation ent
        |> Array.filter (fun eid -> excludeEID.IsNone || eid <> excludeEID.Value) // Not excluded or not me
        |> Array.choose (tryGetComponent ent ct)
        |> Array.map typeConverter

    let getComponent (ent:Entities) (ct:ComponentTypes) (typeConverter:Component->'a) (eid:EntityID) = 
        get ent eid 
        |> Array.find (fun (c:Component) -> getComponentType c = ct)
        |> typeConverter

    let getComponent_ByID (ent:Entities) cid = ent.Components.Item cid

    let getComponents_ByIDs (ent:Entities) (cids:ComponentID[]) = cids |> Array.map (getComponent_ByID ent)

    let getComponents_OfType (ent:Entities) (typeConverter:Component->'a) ct = 
        ent.ComponentTypes.Item ct 
        |> Array.map (getComponent_ByID ent)
        |> Array.map typeConverter

    let getComponentTypes (ent:Entities) (eid:EntityID) =
        eid
        |> get ent
        |> Array.map getComponentType

    let getEntityIDsAtLocation (ent:Entities) location =
        ent.Locations.Item(location) 
        |> getComponents_ByIDs ent
        |> Array.map getComponentEntityID

    let getLocation (ent:Entities) (eid:EntityID) = (getComponent ent FormComponent ToForm eid).Location

    let getLocationMap (ent:Entities) = 
        ent.Locations
        |> Map.map (fun _ cids -> 
            cids
            |> Array.map (getComponent_ByID ent)
            |> Array.map ToForm
            )

    let isLocationImpassible (ent:Entities) (excludeEID:EntityID option) (location:Location) =
        location
        |> getAtLocationWithComponent ent FormComponent ToForm excludeEID
        |> Array.exists (fun f -> not f.IsPassable)

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
                Log = Logging.log1 game.Log "Ok" "Game" "removeEntity" eid None None
        }

    let tryGetComponent (ent:Entities) (ct:ComponentTypes) (eid:EntityID) : Option<Component> = 
        match (get ent eid) |> Array.filter (fun c -> getComponentType c = ct) with
        | [||] -> None
        | cts -> Some cts.[0]

    let updateComponent (game:Game) (c:Component) (logstring:string option) = 
        {
            game with 
                Entities = updateComponents_Internal game.Entities [|c|]
                Log = if logstring.IsSome then Logging.log game.Log logstring.Value else game.Log
        }

    let updateComponents (game:Game) (cts:Component[]) (logstring:string option) = 
        {
            game with 
                Entities = updateComponents_Internal game.Entities cts
                Log = if logstring.IsSome then Logging.log game.Log logstring.Value else game.Log
        }

    
