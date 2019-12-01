module Entities
open CommonFunctions
open CommonTypes
open Component
open ComponentEnums


type Entities = 
    {
        ComponentTypes : Map<ComponentTypes,EntityID[]>
        Entities : Map<EntityID,Component[]>
        Locations : Map<Location,EntityID[]>
        MaxEntityID : EntityID
    }
    static member empty = 
        {
            ComponentTypes = Map.empty
            Entities = Map.empty
            Locations = Map.empty
            MaxEntityID = EntityID(0u)
        }


let createEntity (ent:Entities) (cts:Component[]) =       
    {
        ComponentTypes = 
            cts
            |> Array.fold (fun (m:Map<ComponentTypes,EntityID[]>) (c:Component) ->
                map_AppendValueToArrayUnique m (getComponentType c) (getComponentEntityID c)
                ) ent.ComponentTypes

        Entities = ent.Entities.Add(ent.MaxEntityID + 1u,cts)

        Locations = 
            match (cts |> Array.filter (fun (c:Component) -> getComponentType c = FormComponent)) with
            | [||] -> ent.Locations
            | a -> 
                let (Form f) = a.[0]
                map_AppendValueToArrayUnique ent.Locations f.Location f.EntityID

        MaxEntityID = ent.MaxEntityID + 1u
    }

let getComponent ctid eid =
    ()

let getEntity (ent:Entities) (eid:EntityID) = 
    match (ent.Entities.ContainsKey eid) with
    | false -> [||]
    | true -> ent.Entities.Item eid

let getLocationMap (ent:Entities) = 
    ent.Locations
    |> Map.map (fun _ eids -> 
        eids
        |> Array.Parallel.collect (getEntity ent)
        |> Array.filter (fun c -> getComponentType c = FormComponent)
        |> Array.Parallel.map ToForm
        )
    
   