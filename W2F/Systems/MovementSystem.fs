module MovementSystem
open CommonTypes
open Component
open ControllerComponent
open Entities
open GameTypes
open LocationFunctions
open MovementComponent


let movementActionsAllowed (game:Game) (eid:EntityID) =
    let mutable _allowed = Array.empty<ActionTypes>
    let move = Entities.getComponent game.Entities MovementComponent ToMovement eid
    let loc = Entities.getLocation game.Entities eid
    match move.MovesPerTurn = 0 with 
    | true -> _allowed
    | false ->
        if (isOnMap2D game.MapSize (loc + North.Location)) && not (isLocationImpassible game.Entities (Some eid) (loc + North.Location)) then _allowed <- Array.append _allowed [|Move_North|]
        if (isOnMap2D game.MapSize (loc +  East.Location)) && not (isLocationImpassible game.Entities (Some eid) (loc +  East.Location)) then _allowed <- Array.append _allowed [|Move_East |]
        if (isOnMap2D game.MapSize (loc + South.Location)) && not (isLocationImpassible game.Entities (Some eid) (loc + South.Location)) then _allowed <- Array.append _allowed [|Move_South|]
        if (isOnMap2D game.MapSize (loc +  West.Location)) && not (isLocationImpassible game.Entities (Some eid) (loc +  West.Location)) then _allowed <- Array.append _allowed [|Move_West |]
        _allowed


let onMovement (game:Game) (Action_Movement cc:EventData) =
    let f = Entities.getComponent game.Entities FormComponent ToForm cc.EntityID
    let dest = 
        f.Location +
        match cc.CurrentAction with 
        | Move_North -> North.Location
        | Move_South -> South.Location
        | Move_East  -> East.Location
        | Move_West  -> West.Location
    match (isOnMap2D game.MapSize dest) && not (isLocationImpassible game.Entities (Some cc.EntityID) dest) with
    | false -> { game with Log = Logger.log2 game.Log "Err" "Movement System" "onMovement" cc.EntityID (Some cc.ID) (Some (dest.ToString())) }
    | true -> 
        {
            game with 
                Entities = Entities.updateComponent game.Entities (Form { f with Location = dest })
                Log = Logger.log2 game.Log "Ok" "Movement System" "onMovement" cc.EntityID (Some cc.ID) (Some (dest.ToString()))
        }

