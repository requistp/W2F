module GameEvents
open EngineTypes
open Components


type EventTypes = 
    | EngineEvent_EntityCreated
    | EngineEvent_ComponentAdded
    | EngineEvent_EntityRemoved 

    | Action_Eat 
    | Action_ExitGame 
    | Action_Movement 
    | Metabolize
    | PlantRegrowth
    | PlantReproduce
    member me.TypeID : EventTypeID = 
        match me with
        | EngineEvent_EntityCreated -> 0uy
        | EngineEvent_ComponentAdded -> 1uy
        | EngineEvent_EntityRemoved -> 2uy
        | Action_Eat -> 20uy
        | Action_ExitGame -> 23uy
        | Action_Movement -> 26uy
        | Metabolize -> 28uy
        | PlantRegrowth -> 30uy
        | PlantReproduce -> 32uy


type Action_Eat(eid:EntityID) =
    inherit AbstractEventData(EventTypes.Action_Eat.TypeID, "Action_Eat", eid)

    
type Action_ExitGame() =
    inherit AbstractEventData(EventTypes.Action_ExitGame.TypeID, "Action_ExitGame", EntityID 0u)
    

type Action_Movement(cc:ControllerComponent) =
    inherit AbstractEventData(EventTypes.Action_Movement.TypeID, "Action_Movement", cc.EntityID)
    member _.ControllerComponent with get() = cc


type Metabolize(eid:EntityID) =
    inherit AbstractEventData(EventTypes.Metabolize.TypeID, "Metabolize", eid)


type PlantRegrowth(eid:EntityID) =
    inherit AbstractEventData(EventTypes.PlantRegrowth.TypeID, "PlantRegrowth", eid)


type PlantReproduce(eid:EntityID) =
    inherit AbstractEventData(EventTypes.PlantReproduce.TypeID, "PlantRegrowth", eid)


