module Entities
open CommonFunctions
open CommonTypes
open Component
open ComponentEnums


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


let createEntity (ent:Entities) (cts:Component[]) = 
    {
        Components = 
            cts
            |> Array.fold (fun (m:Map<ComponentID,Component>) (c:Component) -> 
                m.Add(getComponentID c,c)
                ) ent.Components

        ComponentTypes = 
            cts
            |> Array.fold (fun (m:Map<ComponentTypes,ComponentID[]>) (c:Component) ->
                map_AppendValueToArrayUnique m (getComponentType c) (getComponentID c)
                ) ent.ComponentTypes

        Entities = ent.Entities.Add(ent.MaxEntityID + 1u,cts |> Array.map getComponentID)

        Locations = 
            match (cts |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)) with
            | [||] -> ent.Locations
            | a -> 
                let (Form f) = a.[0]
                map_AppendValueToArrayUnique ent.Locations f.Location f.ID

        MaxComponentID = ent.MaxComponentID + ComponentID(uint32 cts.Length)

        MaxEntityID = ent.MaxEntityID + 1u
    }

//let getComponent ctid eid =
//    ()

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
    
   