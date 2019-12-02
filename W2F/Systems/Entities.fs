module Entities
open CommonFunctions
open CommonTypes
open Component
open ComponentEnums
open FormComponent

type Entities = 
    {
        Components : Map<ComponentID,Component>
        ComponentTypes : Map<ComponentTypes,ComponentID[]>
        Entities : Map<EntityID,ComponentID[]>
        Locations : Map<Location,ComponentID[]>
        MaxComponentID : ComponentID
        MaxEntityID : EntityID
    }
    static member empty = 
        {
            Components = Map.empty
            ComponentTypes = Map.empty
            Entities = Map.empty
            Locations = Map.empty
            MaxComponentID = ComponentID(0u)
            MaxEntityID = EntityID(0u)
        }

let private addComponents (start:Map<ComponentID,Component>) (cts:Component[]) =
    cts
    |> Array.fold (fun (m:Map<ComponentID,Component>) (c:Component) -> 
        m.Add(getComponentID c,c)
        ) start

let private addComponentTypes (start:Map<ComponentTypes,ComponentID[]>) (cts:Component[]) =
    cts
    |> Array.fold (fun (m:Map<ComponentTypes,ComponentID[]>) (c:Component) ->
        map_AppendValueToArrayUnique m (getComponentType c) (getComponentID c)
        ) start

let private addLocation (start:Map<Location,ComponentID[]>) (cts:Component[]) = 
    cts 
    |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)
    |> Array.Parallel.map ToForm
    |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:FormComponent) ->
        map_AppendValueToArrayUnique m f.Location f.ID
        ) start

let private moveLocation (start:Map<Location,ComponentID[]>) (cts:Component[]) = 
    cts 
    |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)
    |> Array.Parallel.map ToForm
    |> Array.fold (fun (m:Map<Location,ComponentID[]>) (f:FormComponent) ->
        //get old form
        //check if location is the same
        //move if needed
        map_AppendValueToArrayUnique m f.Location f.ID
        ) start

let createEntity (ent:Entities) (cts:Component[]) = 
    {
        Components = addComponents ent.Components cts
        ComponentTypes = addComponentTypes ent.ComponentTypes cts
        Entities = ent.Entities.Add(ent.MaxEntityID + 1u,cts |> Array.map getComponentID)
        Locations = addLocation ent.Locations cts
        MaxComponentID = ent.MaxComponentID + ComponentID(uint32 cts.Length)
        MaxEntityID = ent.MaxEntityID + 1u
    }

let getComponent_ByID (ent:Entities) cid = ent.Components.Item cid

let getComponents_OfType (ent:Entities) ct =
    ent.ComponentTypes.Item ct
    |> Array.map (getComponent_ByID ent)

let getEntity (ent:Entities) (eid:EntityID) = 
    match (ent.Entities.ContainsKey eid) with
    | false -> [||]
    | true -> ent.Entities.Item eid

let getLocationMap (ent:Entities) = 
    ent.Locations
    |> Map.map (fun _ cids -> 
        cids
        |> Array.map (getComponent_ByID ent)
        |> Array.map ToForm
        )
    
let updateComponents (ent:Entities) (cts:Component[]) = 
    {
        ent with
            Components = 
                cts
                |> Array.fold (fun (m:Map<ComponentID,Component>) (c:Component) -> 
                    m.Remove(getComponentID c).Add(getComponentID c,c)
                    ) ent.Components
            Locations =
                ent.Locations
    }

