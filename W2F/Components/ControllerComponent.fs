module ControllerComponent
open CommonTypes
open ComponentEnums


type ControllerTypes = 
    | AI_Random
    | Keyboard


type ActionTypes = 
    | Eat
    | ExitGame
    | Idle
    | Mate
    | Move_East
    | Move_North
    | Move_South
    | Move_West
    static member AsArray = 
        [|
            Eat
            ExitGame
            Idle
            Mate
            Move_East
            Move_North
            Move_South
            Move_West
        |]


type ControllerComponent = 
    { 
        ID : ComponentID
        EntityID : EntityID
        ControllerType : ControllerTypes
        CurrentAction : ActionTypes
        CurrentActions : ActionTypes[]   // Actions that can be done this turn
        PotentialActions : ActionTypes[] // Actions that can be done by entities components
    }


let requiredComponents (at:ActionTypes) =
    match at with
    | Eat -> [| EatingComponent |]
    | ExitGame -> [||]
    | Idle -> [||]
    | Mate -> [| MatingComponent |]
    | Move_East -> [| FormComponent; MovementComponent |]
    | Move_North -> [| FormComponent; MovementComponent |]
    | Move_South -> [| FormComponent; MovementComponent |]
    | Move_West -> [| FormComponent; MovementComponent |]

