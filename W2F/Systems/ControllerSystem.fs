module ControllerSystem
open CommonFunctions
open CommonTypes
open Component
open ComponentEnums
open ControllerComponent
open Entities
open System

let private getCurrentActions (ent:Entities) (round:RoundNumber) (c:ControllerComponent) = 
    //let movesAllowed = MovementActionsAllowed enm entityID
    let newCurrentActions =
        c.CurrentActions 
        |> Array.choose (fun a -> //actionEnabledTest
            match a with
            | Eat -> Some Eat //if EatActionEnabled enm entityID then Some Eat else None
            | Idle -> Some Idle
            | Mate -> Some Mate //if MateActionEnabled enm entityID round then Some Mate else None
            | Move_North -> Some Move_North //if Array.contains Move_North movesAllowed then Some Move_North else None
            | Move_East ->  Some Move_East //if Array.contains Move_East  movesAllowed then Some Move_East  else None
            | Move_South -> Some Move_South //if Array.contains Move_South movesAllowed then Some Move_South else None
            | Move_West ->  Some Move_West //if Array.contains Move_West  movesAllowed then Some Move_West  else None
            )
    match (arrayContentsMatch newCurrentActions c.CurrentActions) with
    | true -> c
    | false -> { c with CurrentActions = newCurrentActions }

let getInputForAllEntities (ent:Entities) (*log:agent_GameLog*) (round:RoundNumber) (*renderer:(EntityManager->EntityID->unit) option*) = 
    let getActionForEntity (cc:ControllerComponent) =
        //let awaitKeyboardInput (*renderer:(EntityManager->EntityID->unit) option*) : ActionTypes =
        //    let mutable _action = None            
        //    // Uncomment for Entity-view... 
        //    //if renderer.IsSome then
        //    //    Async.Parallel
        //    //    (
        //    //        renderer.Value enm (controller.EntityID)
        //    //    )        
        //    let handleKeyPressed (k:ConsoleKeyInfo) = 
        //        while Console.KeyAvailable do //Might help clear double movement keys entered in one turn
        //            Console.ReadKey(true).Key |> ignore        
        //        match k.Key with 
        //        | ConsoleKey.Escape -> Some (Idle,false)
        //        | ConsoleKey.Spacebar -> Some (Idle,true)
        //        | ConsoleKey.E -> if actionIsAllowed cc Eat  then Some (Eat,true) else None
        //        | ConsoleKey.M -> if actionIsAllowed cc Mate then Some (Mate,true) else None
        //        | ConsoleKey.RightArrow -> if actionIsAllowed cc Move_East  then Some (Move_East,true) else None
        //        | ConsoleKey.UpArrow    -> if actionIsAllowed cc Move_North then Some (Move_North,true) else None
        //        | ConsoleKey.DownArrow  -> if actionIsAllowed cc Move_South then Some (Move_South,true) else None
        //        | ConsoleKey.LeftArrow  -> if actionIsAllowed cc Move_West  then Some (Move_West,true) else None
        //        | _ -> None
        //    while _action.IsNone do
        //        while not Console.KeyAvailable do
        //            System.Threading.Thread.Sleep 2
        //        _action <- handleKeyPressed (Console.ReadKey(true))        
        //    _action.Value
        {
            cc with
                CurrentAction = 
                    match cc.ControllerType with
                    | AI_Random -> 
                        cc.CurrentActions.[random.Next(cc.CurrentActions.Length)]
                    | Keyboard -> 
                        cc.CurrentActions.[random.Next(cc.CurrentActions.Length)]
                        //awaitKeyboardInput 
                    | _ -> Idle // Should raise an error?
        }
    let handleSplitInputTypes (keyboard:ControllerComponent[], ai:ControllerComponent[]) =
        ai 
        |> Array.Parallel.map getActionForEntity
        |> Array.append (
            keyboard 
            |> Array.map getActionForEntity)
    //start
    ControllerComponent
    |> getComponents_OfType ent
    |> Array.Parallel.map ToController
    |> Array.Parallel.map (getCurrentActions ent round)
    |> Array.Parallel.partition (fun c -> c.ControllerType = Keyboard)
    |> handleSplitInputTypes




//|> Array.Parallel.map (fun (Controller c) -> setCurrentActions c) 
//|> Array.fold (fun (m:Map<ComponentID,Component>) (cc:ControllerComponent) ->
//    let newCC = getInputForEntity cc
//    match newCC = cc with
//    | true -> m
//    | false -> m.Remove(newCC.ID).Add(newCC.ID,Controller newCC)
//    ) ent.Components

//let updateActionIfChanged (controller:ControllerComponent) newAction = 
//    if (newAction <> controller.CurrentAction) then
//        enm.UpdateComponent (Controller { controller with CurrentAction = newAction })
//        log.Log round (sprintf "%-3s | %-20s -> %-30s #%7i : %A" "Ok" "Controller System" "Current action" controller.EntityID.ToUint32 newAction)
//|> handleSplitInputTypes 
//let handleSplitInputTypes (keyboard:ControllerComponent[], ai:ControllerComponent[]) =
    //ai |> Array.iter getAIInputForEntity
    //keyboard 
    //|> Array.map getKeyboardInputForEntity
    //|> Array.forall (fun b -> b)

//let getKeyboardInputForEntity (cc:ControllerComponent) =
//    {
//        cc with
//            CurrentAction = 
//                match cc.ControllerType with
//                | Keyboard -> 
//                    awaitKeyboardInput enm controller renderer round
//                | _ -> Idle // Should raise an error?
//    }
//    let newAction,cont =
//        match controller.ControllerType with
//        | Keyboard -> 
//            AwaitKeyboardInput enm controller renderer round
//        | _ -> Idle,false // Should raise an error
//    match cont with
//    | false -> false
//    | true -> 
//        updateActionIfChanged controller newAction
//        //if (newAction <> controller.CurrentAction) then
//        //    enm.UpdateComponent (Controller { controller with CurrentAction = newAction })
//        //    log.Log round (sprintf "%-3s | %-20s -> %-30s #%7i.%i : %A" "Ok" "Controller System" "Current action" controller.EntityID.ToUint32 controller.ID.ToUint32 newAction)
//        true