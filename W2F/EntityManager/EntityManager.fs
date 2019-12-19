module EntityManager
open agent_Components
open agent_ComponentTypes
open agent_Entities
open agent_Locations
open CommonTypes


type EntitiesForSave = 
    {
        Components : Map<ComponentID,AbstractComponent>
        ComponentTypes : Map<ComponentTypeID,ComponentID[]>
        Entities : Map<EntityID,ComponentID[]>
        Locations : Map<Location,ComponentID[]>
        MaxComponentID : ComponentID
        MaxEntityID : EntityID
    }


type EntityManager() = 
    let agent_Components = new agent_Components()
    let agent_Entities = new agent_Entities()
    let agent_Locations = new agent_Locations()
    let agent_ComponentTypes = new agent_ComponentTypes()
    
    member  _.create (cts:AbstractComponent[]) = 
        Async.Parallel 
        (
            agent_Components.adds cts

            agent_ComponentTypes.adds cts

            cts |> Array.filter (fun c -> c.ComponentType = 0uy) |> Array.map (fun c -> c :?> AbstractComponent_WithLocation) |> agent_Locations.adds

            agent_Entities.add cts
        ) 
    member  _.creates (ctss:AbstractComponent[][]) = 
        let cts = ctss |> Array.collect (fun cts -> cts)
        Async.Parallel 
        (
            agent_Components.adds cts

            agent_ComponentTypes.adds cts

            cts |> Array.filter (fun c -> c.ComponentType = 0uy) |> Array.map (fun c -> c :?> AbstractComponent_WithLocation) |> agent_Locations.adds

            agent_Entities.adds ctss
        ) 
    member  _.exists eid = 
        match agent_Entities.get eid with
        | [||] -> false
        | _ -> true
    member  _.get eid = agent_Entities.get eid |> agent_Components.gets
    member  _.get_AtLocation location = agent_Locations.get location |> agent_Components.gets |> Array.map (fun c -> c :?> AbstractComponent_WithLocation)
    member me.get_AtLocationWithComponent (ct:ComponentTypeID) (excludeEID:EntityID option) (location:Location) =
        location
        |> me.get_EntityIDsAtLocation
        |> Array.filter (fun eid -> excludeEID.IsNone || eid <> excludeEID.Value) // Not excluded or not me
        |> Array.choose (me.tryGetComponent ct)
    member me.get_Component ct eid = 
        eid
        |> me.get
        |> Array.find (fun (c:AbstractComponent) -> c.ComponentType = ct)
    member  _.get_ComponentsOfType ct = agent_ComponentTypes.get ct |> agent_Components.gets
    member me.get_ComponentTypes (eid:EntityID) =
        eid
        |> me.get
        |> Array.map (fun c -> c.ComponentType) 
    member me.get_EntityIDsAtLocation location =
        location
        |> me.get_AtLocation
        |> Array.map (fun c -> c.EntityID) 
    member me.get_Location (eid:EntityID) = 
        (me.get_Component 0uy eid :?> AbstractComponent_WithLocation).Location 
    member  _.get_LocationMap = 
        agent_Locations.getMap
        |> Map.map (fun _ cids -> cids |> agent_Components.gets)
    member me.get_Save = 
        {
            Components = agent_Components.getMap
            ComponentTypes = agent_ComponentTypes.getMap
            Entities = agent_Entities.getMap
            Locations = agent_Locations.getMap
            MaxComponentID = agent_Components.getID()
            MaxEntityID = agent_Entities.getID()
        }
    member  _.init (save:EntitiesForSave) =
        agent_Components.init save.Components save.MaxComponentID
        agent_ComponentTypes.init save.ComponentTypes
        agent_Entities.init save.Entities save.MaxEntityID
        agent_Locations.init save.Locations
    member me.isLocationImpassible (excludeEID:EntityID option) (location:Location) =
        location
        |> me.get_AtLocation
        |> Array.filter (fun c -> excludeEID.IsNone || c.EntityID <> excludeEID.Value)
        |> Array.exists (fun c -> not c.IsPassable)
    member  _.NewComponentID() = agent_Components.newID()
    member  _.NewEntityID() = agent_Entities.newID()
    member me.remove eid = 
        let cts = me.get eid
        
        Async.Parallel
        (
            agent_Components.removes cts

            agent_ComponentTypes.removes cts

            cts |> Array.filter (fun c -> c.ComponentType = 0uy) |> Array.map (fun c -> c :?> AbstractComponent_WithLocation) |> agent_Locations.removes

            agent_Entities.remove eid
        )
        Ok None
    member me.tryGetComponent (ct:ComponentTypeID) (eid:EntityID) : Option<AbstractComponent> = 
        match me.get eid |> Array.filter (fun c -> c.ComponentType = ct) with
        | [||] -> None
        | cts -> Some cts.[0]
    member me.updateComponent (c:AbstractComponent) = 
        match c.ComponentType with
        | 0uy -> 
            let oldForm = me.get_Component 0uy c.EntityID :?> AbstractComponent_WithLocation
            let newForm = c :?> AbstractComponent_WithLocation
            if (oldForm.Location <> newForm.Location) then
                agent_Locations.move oldForm newForm
        | _ -> ()
        
        agent_Components.update c
        
