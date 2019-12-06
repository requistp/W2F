module rec Entities
open CommonFunctions
open CommonTypes
open Component
open ComponentEnums
open EntityAndGameTypes
open FormComponent
        

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
        map_AppendValueToArrayUnique m (getComponentType c) (getComponentID c)
        ) start

let private addLocation (start:Map<Location,ComponentID[]>) (cts:Component[]) = 
    cts 
    |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)
    |> Array.map ToForm
    |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:FormComponent) ->
        map_AppendValueToArrayUnique m f.Location f.ID
        ) start

let private moveLocations (ent:Entities) (cts:Component[]) = 
    cts 
    |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)
    |> Array.map ToForm
    |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:FormComponent) ->
        let (Form oldF) = ent.Components.Item f.ID
        match (oldF = f) with
        | true -> m
        | false -> map_AppendValueToArrayUnique (map_RemoveValueFromArray m oldF.Location oldF.ID) f.Location f.ID
        ) ent.Locations

//---------------------------------------------------------------------------------------

let createEntity (ent:Entities) (cts:Component[]) = 
    {
        Components = addComponents ent.Components cts
        ComponentTypes = addComponentTypes ent.ComponentTypes cts
        Entities = ent.Entities.Add(ent.MaxEntityID + 1u,cts |> Array.map getComponentID)
        Locations = addLocation ent.Locations cts
        MaxComponentID = ent.MaxComponentID + ComponentID(uint32 cts.Length)
        MaxEntityID = ent.MaxEntityID + 1u
    }

let get (ent:Entities) eid = ent.Entities.Item eid |> getComponents_ByIDs ent

let getAtLocationWithComponent (ent:Entities) (ct:ComponentTypes) (excludeEID:EntityID option) (location:Location) = 
    location
    |> getEntityIDsAtLocation ent
    |> Array.filter (fun eid -> excludeEID.IsNone || eid <> excludeEID.Value) // Not excluded or not me
    |> Array.choose (tryGetComponent ent ct)

let getComponent (ent:Entities) ct eid = get ent eid |> Array.find (fun (c:Component) -> getComponentType c = ct)

let getComponent_ByID (ent:Entities) cid = ent.Components.Item cid

let getComponents_ByIDs (ent:Entities) (cids:ComponentID[]) = cids |> Array.map (getComponent_ByID ent)

let getComponents_OfType (ent:Entities) ct = ent.ComponentTypes.Item ct |> Array.map (getComponent_ByID ent)

let getComponentTypes (ent:Entities) (eid:EntityID) =
    eid
    |> get ent
    |> Array.map getComponentType

let getEntityIDsAtLocation (ent:Entities) location =
    ent.Locations.Item(location) 
    |> getComponents_ByIDs ent
    |> Array.map getComponentEntityID

let getLocation (ent:Entities) (eid:EntityID) = (ToForm (getComponent ent FormComponent eid)).Location

let getLocationMap (ent:Entities) = 
    ent.Locations
    |> Map.map (fun _ cids -> 
        cids
        |> Array.map (getComponent_ByID ent)
        |> Array.map ToForm
        )

let impassableLocation (ent:Entities) (excludeEID:EntityID option) (location:Location) =
    location
    |> getAtLocationWithComponent ent FormComponent excludeEID
    |> Array.exists (fun (Form f) -> not f.IsPassable)

let tryGetComponent (ent:Entities) (ct:ComponentTypes) (eid:EntityID) : Option<Component> = 
    match (get ent eid) |> Array.filter (fun c -> getComponentType c = ct) with
    | [||] -> None
    | cts -> Some cts.[0]

let updateComponent (ent:Entities) (c:Component) = updateComponents ent [|c|]

let updateComponents (ent:Entities) (cts:Component[]) = 
    {
        ent with
            Components = addComponents ent.Components cts
            Locations = moveLocations ent cts
    }




//let getEntity (ent:Entities) (eid:EntityID) = 
//    match (ent.Entities.ContainsKey eid) with
//    | false -> [||]
//    | true -> ent.Entities.Item eid
