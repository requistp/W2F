module MovementSystem
open EngineTypes
open ComponentEnums
open Components
open Engine.Entities
open GameEvents
open LocationFunctions


let movementActionsAllowed (game:Game) (eid:EntityID) =
    let mutable _allowed = Array.empty<ActionTypes>
    let move = ToMovement (getComponent game.Entities ComponentTypes.Movement.TypeID eid)
    let loc = getLocation game.Entities eid
    match move.MovesPerTurn = 0 with 
    | true -> _allowed
    | false ->
        if (isOnMap2D game.MapSize (loc + North.Location)) && not (Engine.Entities.isLocationImpassible game.Entities (Some eid) (loc + North.Location)) then _allowed <- Array.append _allowed [|Move_North|]
        if (isOnMap2D game.MapSize (loc +  East.Location)) && not (isLocationImpassible game.Entities (Some eid) (loc +  East.Location)) then _allowed <- Array.append _allowed [|Move_East |]
        if (isOnMap2D game.MapSize (loc + South.Location)) && not (isLocationImpassible game.Entities (Some eid) (loc + South.Location)) then _allowed <- Array.append _allowed [|Move_South|]
        if (isOnMap2D game.MapSize (loc +  West.Location)) && not (isLocationImpassible game.Entities (Some eid) (loc +  West.Location)) then _allowed <- Array.append _allowed [|Move_West |]
        _allowed


let onMovement (game:Game) (e:AbstractEventData) =
    let ed = e :?> Action_Movement
    let f = Engine.Entities.getComponent game.Entities ComponentTypes.Form.TypeID ed.ControllerComponent.EntityID |> ToForm
    let dest = 
        f.Location +
        match ed.ControllerComponent.CurrentAction with 
        | Move_North -> North.Location
        | Move_South -> South.Location
        | Move_East  -> East.Location
        | Move_West  -> West.Location
    match (isOnMap2D game.MapSize dest) && not (isLocationImpassible game.Entities (Some ed.ControllerComponent.EntityID) dest) with
    | false -> { game with Log = Logging.log1 game.Log "Err" "Movement System" "onMovement" ed.ControllerComponent.EntityID (Some ed.ControllerComponent.ID) (Some (dest.ToString())) }
    | true -> 
        let forlog = Logging.format1 "Ok" "Movement System" "onMovement" ed.ControllerComponent.EntityID (Some ed.ControllerComponent.ID) (Some (dest.ToString()))
        Engine.Entities.updateComponent game (FormComponent(f.ID, f.EntityID, f.Born, f.CanSeePast, f.IsPassable, dest, f.Name, f.Symbol)).Abstract (Some forlog)

