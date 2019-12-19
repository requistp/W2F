module ControllerSystem
open CommonFunctions
open ComponentEnums
open Components
open EngineTypes
open GameEvents
open System


let private actionIsAllowed (cc:ControllerComponent) (action:ActionTypes) = 
    cc.CurrentActions 
    |> Array.contains action


let private requiredComponents (at:ActionTypes) =
    match at with
    | Eat -> [| ComponentTypes.Eating |]
    | ExitGame -> [||]
    | Idle -> [||]
    | Mate -> [| ComponentTypes.Mating |]
    | Move_East -> [| ComponentTypes.Form; ComponentTypes.Movement |]
    | Move_North -> [| ComponentTypes.Form; ComponentTypes.Movement |]
    | Move_South -> [| ComponentTypes.Form; ComponentTypes.Movement |]
    | Move_West -> [| ComponentTypes.Form; ComponentTypes.Movement |]


let getPotentialActions (cts:AbstractComponent[]) = 
    let ects = cts |> Array.map (fun (c:AbstractComponent) -> c.ComponentType)
    ActionTypes.AsArray 
    |> Array.choose (fun a -> if requiredComponents a |> Array.forall (fun ct -> ects |> Array.contains ct.TypeID) then Some a else None)


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
                | Mate -> if MatingSystem.mateActionEnabled game.Entities cc.EntityID game.Round then Some Mate else None
                | Move_North -> if Array.contains Move_North movesAllowed then Some Move_North else None
                | Move_East ->  if Array.contains Move_East  movesAllowed then Some Move_East  else None
                | Move_South -> if Array.contains Move_South movesAllowed then Some Move_South else None
                | Move_West ->  if Array.contains Move_West  movesAllowed then Some Move_West  else None
                )
        match (arraysMatch newCurrent cc.CurrentActions) with
        | true -> cc
        | false -> ControllerComponent(cc.ID, cc.EntityID, cc.ControllerType, cc.CurrentAction, newCurrent, cc.PotentialActions)

    let getActionForEntity (cc:ControllerComponent) =
        let currentAction = 
            match cc.ControllerType with
            | AI_Random -> 
                cc.CurrentActions.[random.Next(cc.CurrentActions.Length)]
            | Keyboard -> 
                awaitKeyboardInput cc
        ControllerComponent(cc.ID, cc.EntityID, cc.ControllerType, currentAction, cc.CurrentActions, cc.PotentialActions)

    let handleSplitInputTypes (keyboard:ControllerComponent[], ai:ControllerComponent[]) =
        ai 
        |> Array.map getActionForEntity
        |> Array.append (keyboard |> Array.map getActionForEntity)

    //start
    let newControllers = 
        ComponentTypes.Controller.TypeID
        |> Engine.Entities.get_Components_OfType game.Entities 
        |> ToControllers
        |> Array.map getCurrentActions
        |> Array.partition (fun c -> c.ControllerType = Keyboard)
        |> handleSplitInputTypes
    newControllers
    |> Array.fold (fun g c ->
        Engine.Entities.updateComponent 
            g 
            c.Abstract 
            (Some (Logging.format1 "Ok" "Controller System" "getInputs" c.EntityID (Some c.ID) (Some (c.CurrentAction,c.CurrentActions))))
        ) game


let processInputs (game:Game) : Game =
    let convertToEventData (c:ControllerComponent) =
        match c.CurrentAction with
        | Eat -> Action_Eat(c.EntityID).Abstract 
        | ExitGame -> Action_ExitGame().Abstract
        //| Mate -> 
        | Move_North | Move_East | Move_South | Move_West -> Action_Movement(c).Abstract
    ComponentTypes.Controller.TypeID
    |> Engine.Entities.get_Components_OfType game.Entities
    |> ToControllers
    |> Array.filter (fun c -> not (Array.contains c.CurrentAction [|Idle|]))
    |> Array.sortBy (fun c -> c.CurrentAction ) 
    |> Array.fold (fun g c -> Engine.Events.execute (convertToEventData c) g) game


let onExitGame (game:Game) (_:AbstractEventData) : Game = 
    Engine.Settings.exitGame game

    
let onComponentAdded (game:Game) (e:AbstractEventData) : Game = 
    let c = (e :?> EngineEvent_ComponentAdded).Component
    match c.ComponentType = Controller.TypeID with
    | false -> game
    | true -> 
        let old = c :?> ControllerComponent
        let newPotential = getPotentialActions (Engine.Entities.get game.Entities c.EntityID)
        Engine.Entities.updateComponent 
            game
            (ControllerComponent(old.ID, old.EntityID, old.ControllerType, old.CurrentAction, old.CurrentActions, newPotential))
            (Some (Logging.format1 "Ok" "Controller System" "onComponentAdded" c.EntityID (Some c.ID) (Some newPotential)))
    
