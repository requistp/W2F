module agent_EventSchedule
open CommonFunctions
open CommonTypes


type private agent_ScheduleMsg =
    | Add of RoundNumber * ScheduledEvent
    | Get  of RoundNumber * AsyncReplyChannel<ScheduledEvent[]>
    | GetMap  of AsyncReplyChannel<Map<RoundNumber,ScheduledEvent[]> >
    | Init of Map<RoundNumber,ScheduledEvent[]>


type agent_EventSchedule() = 

    let agent =
        let mutable _map = Map.empty<RoundNumber,ScheduledEvent[]>
        MailboxProcessor<agent_ScheduleMsg>.Start(
            fun inbox ->
                async { 
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | Add (r,se) -> 
                            _map <- map_AppendToArray_NonUnique _map r se
                        | Get (r,replyChannel) ->
                            replyChannel.Reply(
                                match (_map.ContainsKey r) with
                                | false -> [||]
                                | true -> 
                                    let a = _map.Item(r)
                                    _map <- _map.Remove(r)
                                    a
                                    )
                        | GetMap replyChannel ->
                            replyChannel.Reply(_map)
                        | Init start ->
                            _map <- start
                }
            )

    member _.add r se = agent.Post (Add (r,se))
    member _.get r = agent.PostAndReply (fun replyChannel -> Get (r,replyChannel)) 
    member _.getMap = agent.PostAndReply GetMap
    member _.init start = agent.Post (Init start)


                                        
//addToSchedule round se
//| ExecuteScheduled round ->
//    let reschedule (se:ScheduledEventData) =
//        match se.ScheduleType with
//        | RunOnce -> ()
//        | RepeatIndefinitely -> addToSchedule round se false 
//        | RepeatFinite remaining -> if (remaining > 1) then addToSchedule round { se with ScheduleType = RepeatFinite (remaining - 1) } false
//    let executeAndReschedule (se:ScheduledEventData) =
//        if (enm.EntityExists (GetGameEvent_EntityID se.GameEvent)) then
//            agentListeners.Post (Execute (round,se.GameEvent))
//            reschedule se
//    if (_schedule.ContainsKey round) then
//        _schedule.Item(round) |> Array.Parallel.iter executeAndReschedule
//        _schedule <- _schedule.Remove(round)
