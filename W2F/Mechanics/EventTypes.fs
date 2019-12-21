module EventTypes
open EngineTypes
open Components

type EventTypes = 
    // Engine
    | Engine_EntityCreated
    | Engine_ComponentAdded
    | Engine_EntityRemoved 
    | Engine_ComponentUpdated
    // Mechanics
    | Action_Eat 
    | Action_ExitGame 
    | Action_Movement 
    | LocationChanged
    | Metabolize
    | PlantRegrowth
    | PlantReproduce
    member me.TypeID : EventTypeID = 
        match me with
        // Engine
        | Engine_EntityCreated -> 0uy
        | Engine_ComponentAdded -> 1uy
        | Engine_EntityRemoved -> 2uy
        | Engine_ComponentUpdated -> 3uy
        // Mechanics
        | Action_Eat -> 20uy
        | Action_ExitGame -> 30uy
        | Action_Movement -> 40uy
        | LocationChanged -> 50uy
        | Metabolize -> 60uy
        | PlantRegrowth -> 70uy
        | PlantReproduce -> 80uy

type Action_Eat(eid:EntityID) =
    inherit AbstractEventData(EventTypes.Action_Eat.TypeID, EventTypes.Action_Eat.ToString(), eid)
    
type Action_ExitGame() =
    inherit AbstractEventData(EventTypes.Action_ExitGame.TypeID, EventTypes.Action_ExitGame.ToString(), EntityID 0u)

type Action_Movement(cc:ControllerComponent) =
    inherit AbstractEventData(EventTypes.Action_Movement.TypeID, EventTypes.Action_Movement.ToString(), cc.EntityID)
    member _.ControllerComponent with get() = cc

type LocationChanged(oldf:FormComponent,newf:FormComponent) =
    inherit AbstractEventData(EventTypes.LocationChanged.TypeID, EventTypes.LocationChanged.ToString(), newf.EntityID)
    member _.OldForm with get() = oldf
    member _.NewForm with get() = newf

type Metabolize(eid:EntityID) =
    inherit AbstractEventData(EventTypes.Metabolize.TypeID, EventTypes.Metabolize.ToString(), eid)

type PlantRegrowth(eid:EntityID) =
    inherit AbstractEventData(EventTypes.PlantRegrowth.TypeID, EventTypes.PlantRegrowth.ToString(), eid)

type PlantReproduce(eid:EntityID) =
    inherit AbstractEventData(EventTypes.PlantReproduce.TypeID, EventTypes.PlantReproduce.ToString(), eid)


