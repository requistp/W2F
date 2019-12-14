module rec Events
//open CommonFunctions
//open CommonTypes


//let private add (el:EventListener) (game:Game) : Game =
//    { 
//        game with 
//            EventListeners = map_AppendToArray_NonUnique game.EventListeners el.Type el
//            Log = Logging.log game.Log (sprintf "%-3s | %-20s -> %A : %s" "Ok" "Event Listener" el.Type el.Description)
//    }
    

//let registerListeners (els:EventListener[]) (game:Game) : Game =
//    els 
//    |> Array.fold (fun g el -> add el g) game


//let execute (e:AbstractEventData) (game:Game) : Game =
//    match (game.EventListeners.ContainsKey e.Type) with
//    | false -> game
//    | true -> 
//        game.EventListeners.Item(e.Type)
//        |> Array.fold (fun (g:Game) el ->
//            el.Action g e
//            ) game



//type EventAction = Game -> EventData -> Game
//type EventData = 
//    | Action_Eat of EntityID
//    | Action_ExitGame
//    | Action_Movement of ControllerComponent
//    | ComponentAdded of Component2
//    | Metabolize of EntityID
//    | PlantRegrowth of EntityID
//    member me.EntityID =
//        match me with
//        | Action_Eat eid -> eid
//        | Action_ExitGame -> EntityID(0u)
//        | Action_Movement cc -> cc.EntityID
//        | ComponentAdded c -> c.EntityID
//        | Metabolize eid -> eid
//        | PlantRegrowth eid -> eid
//    member me.Type = 
//        match me with 
//        | Action_Eat _ -> Event_Action_Eat
//        | Action_ExitGame -> Event_Action_ExitGame
//        | Action_Movement _ -> Event_Action_Movement
//        | ComponentAdded _ -> Event_ComponentAdded
//        | Metabolize _ -> Event_Metabolize
//        | PlantRegrowth _ -> Event_PlantRegrowth
//type EventTypes = 
//    | Event_Action_Eat
//    | Event_Action_ExitGame
//    | Event_Action_Movement
//    | Event_ComponentAdded
//    | Event_Metabolize
//    | Event_PlantRegrowth
//type EventListener = 
//    {
//        Action : EventAction
//        EventType : EventTypes
//    }
//type ScheduledEvent = 
//    {
//        ScheduleType : ScheduleTypes
//        Frequency : RoundNumber
//        Event : EventData
//    }  