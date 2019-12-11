module ControllerSystem
open CommonFunctions
open CommonTypes
open Component
open ControllerComponent
open GameTypes
open System


let private actionIsAllowed (cc:ControllerComponent) (action:ActionTypes) = cc.CurrentActions |> Array.contains action

let private requiredComponents (at:ActionTypes) =
    match at with
    | Eat -> [| EatingComponent |]
    | ExitGame -> [||]
    | Idle -> [||]
    | Mate -> [| MatingComponent |]
    | Move_East -> [| FormComponent; MovementComponent |]
    | Move_North -> [| FormComponent; MovementComponent |]
    | Move_South -> [| FormComponent; MovementComponent |]
    | Move_West -> [| FormComponent; MovementComponent |]


let getPotentialActions (cts:Component[]) = 
    let ects = cts |> Array.map getComponentType
    ActionTypes.AsArray 
    |> Array.choose (fun a -> if requiredComponents a |> Array.forall (fun ct -> ects |> Array.contains ct) then Some a else None)


let getInputs (game:Game) : Game = 
    let awaitKeyboardInput (cc:ControllerComponent) =
        let mutable _action = None            
        // Uncomment for Entity-view... 
        //if renderer.IsSome then
        //        renderer.Value enm (controller.EntityID)
        let handleKeyPressed (k:ConsoleKeyInfo) = 
            while Console.KeyAvailable do //Might help clear double movement keys entered in one turn
                Console.ReadKey(true).Key |> ignore        
            match k.Key with 
            | ConsoleKey.Escape -> Some ExitGame
            | ConsoleKey.Spacebar -> Some Idle
            | ConsoleKey.E -> if actionIsAllowed cc Eat  then Some Eat  else None
            | ConsoleKey.M -> if actionIsAllowed cc Mate then Some Mate else None
            | ConsoleKey.RightArrow -> if actionIsAllowed cc Move_East  then Some Move_East  else None
            | ConsoleKey.UpArrow    -> if actionIsAllowed cc Move_North then Some Move_North else None
            | ConsoleKey.DownArrow  -> if actionIsAllowed cc Move_South then Some Move_South else None
            | ConsoleKey.LeftArrow  -> if actionIsAllowed cc Move_West  then Some Move_West  else None
            | _ -> None
        while _action.IsNone do
            while not Console.KeyAvailable do
                System.Threading.Thread.Sleep 2
            _action <- handleKeyPressed (Console.ReadKey(true))        
        _action.Value

    let getCurrentActions (cc:ControllerComponent) = 
        let movesAllowed = MovementSystem.movementActionsAllowed game cc.EntityID
        let newCurrent =
            cc.PotentialActions 
            |> Array.choose (function
                | Eat -> if EatingSystem.eatActionEnabled game.Entities cc.EntityID then Some Eat else None
                | ExitGame -> if cc.ControllerType = Keyboard then Some ExitGame else None
                | Idle -> Some Idle
                | Mate -> None //if MateActionEnabled enm entityID game.round then Some Mate else None
                | Move_North -> if Array.contains Move_North movesAllowed then Some Move_North else None
                | Move_East ->  if Array.contains Move_East  movesAllowed then Some Move_East  else None
                | Move_South -> if Array.contains Move_South movesAllowed then Some Move_South else None
                | Move_West ->  if Array.contains Move_West  movesAllowed then Some Move_West  else None
                )
        match (arraysMatch newCurrent cc.CurrentActions) with
        | true -> cc
        | false -> { cc with CurrentActions = newCurrent }

    let getActionForEntity (cc:ControllerComponent) =
        {
            cc with
                CurrentAction = 
                    match cc.ControllerType with
                    | AI_Random -> 
                        cc.CurrentActions.[random.Next(cc.CurrentActions.Length)]
                    | Keyboard -> 
                        awaitKeyboardInput cc
        }

    let handleSplitInputTypes (keyboard:ControllerComponent[], ai:ControllerComponent[]) =
        ai 
        |> Array.map getActionForEntity
        |> Array.append (keyboard |> Array.map getActionForEntity)

    //start
    let newControllers = 
        ControllerComponent
        |> Game.Entities.getComponents_OfType game.Entities ToController
        |> Array.map getCurrentActions
        |> Array.partition (fun c -> c.ControllerType = Keyboard)
        |> handleSplitInputTypes
    newControllers
    |> Array.fold (fun g c ->
        Game.Entities.updateComponent g (Controller c) (Some (Logging.format1 "Ok" "Controller System" "getInputs" c.EntityID (Some c.ID) (Some (c.CurrentAction,c.CurrentActions))))
        ) game


let processInputs (game:Game) : Game =
    let convertToEventData (c:ControllerComponent) =
        match c.CurrentAction with
        | Eat -> Action_Eat c.EntityID
        | ExitGame -> Action_ExitGame
        //| Mate -> 
        | Move_North | Move_East | Move_South | Move_West -> Action_Movement c
    ControllerComponent
    |> Game.Entities.getComponents_OfType game.Entities ToController
    |> Array.filter (fun c -> not (Array.contains c.CurrentAction [|Idle|]))
    |> Array.sortBy (fun c -> c.CurrentAction ) 
    |> Array.fold (fun g c -> Events.execute (convertToEventData c) g) game


let onExitGame (game:Game) (_:EventData) : Game = 
    { game with ExitGame = true }
