module ControllerSystem
open CommonFunctions
open CommonTypes
open Component
open ComponentEnums
open ControllerComponent
open Entities

let private getCurrentActions (ent:Entities) (actions:ActionTypes[]) (entityID:EntityID) (round:RoundNumber) = 
    //let movesAllowed = MovementActionsAllowed enm entityID
    actions 
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

let getInputForAllEntities (ent:Entities) (*log:agent_GameLog*) (round:RoundNumber) (*renderer:(EntityManager->EntityID->unit) option*) = 
    let setCurrentActions (c:ControllerComponent) = 
        let newCurrent = getCurrentActions ent c.PotentialActions c.EntityID round
        match (arrayContentsMatch newCurrent c.CurrentActions) with
        | true -> c
        | false -> { c with CurrentActions = newCurrent }

    let getAIInputForEntity (cc:ControllerComponent) =
        {
            cc with
                CurrentAction = 
                    match cc.ControllerType with
                    | AI_Random -> 
                        cc.CurrentActions.[random.Next(cc.CurrentActions.Length)]
                    | _ -> Idle // Should raise an error?
        }

    let getKeyboardInputForEntity (cc:ControllerComponent) =
        {
            cc with
                CurrentAction = 
                    match cc.ControllerType with
                    | Keyboard -> 
                        awaitKeyboardInput enm controller renderer round
                    | _ -> Idle // Should raise an error?
        }
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

    //start
    ControllerComponent
    |> getComponents_OfType ent
    |> Array.map (fun (Controller c) -> setCurrentActions c) 
    |> Array.fold (fun (m:Map<ComponentID,Component>) (cc:ControllerComponent) ->
        let newCC = 
            match cc.ControllerType with
            | AI_Random -> getAIInputForEntity cc
            | Keyboard -> getAIInputForEntity cc
        match newCC = cc with
        | true -> m
        | false -> m.Remove(newCC.ID).Add(newCC.ID,Controller newCC)
        ) ent.Components




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

