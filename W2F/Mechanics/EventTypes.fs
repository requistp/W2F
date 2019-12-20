module EventTypes
open EngineTypes
open Components


type EventTypes = 
    // Engine
    | Engine_EntityCreated
    | Engine_ComponentAdded
    | Engine_EntityRemoved 

    // Mechanics
    | Action_Eat 
    | Action_ExitGame 
    | Action_Movement 
    | Metabolize
    | PlantRegrowth
    | PlantReproduce
    member me.TypeID : EventTypeID = 
        match me with
        | Engine_EntityCreated -> 0uy
        | Engine_ComponentAdded -> 1uy
        | Engine_EntityRemoved -> 2uy
        | Action_Eat -> 20uy
        | Action_ExitGame -> 23uy
        | Action_Movement -> 26uy
        | Metabolize -> 28uy
        | PlantRegrowth -> 30uy
        | PlantReproduce -> 32uy


type Action_Eat(eid:EntityID) =
    inherit AbstractEventData(EventTypes.Action_Eat.TypeID, EventTypes.Action_Eat.ToString(), eid)

    
type Action_ExitGame() =
    inherit AbstractEventData(EventTypes.Action_ExitGame.TypeID, EventTypes.Action_ExitGame.ToString(), EntityID 0u)
    

type Action_Movement(cc:ControllerComponent) =
    inherit AbstractEventData(EventTypes.Action_Movement.TypeID, EventTypes.Action_Movement.ToString(), cc.EntityID)
    member _.ControllerComponent with get() = cc


type Metabolize(eid:EntityID) =
    inherit AbstractEventData(EventTypes.Metabolize.TypeID, EventTypes.Metabolize.ToString(), eid)


type PlantRegrowth(eid:EntityID) =
    inherit AbstractEventData(EventTypes.PlantRegrowth.TypeID, EventTypes.PlantRegrowth.ToString(), eid)


type PlantReproduce(eid:EntityID) =
    inherit AbstractEventData(EventTypes.PlantReproduce.TypeID, EventTypes.PlantReproduce.ToString(), eid)


