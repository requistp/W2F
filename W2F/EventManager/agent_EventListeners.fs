module agent_EventListeners
open CommonFunctions
open CommonTypes
open Game


type private agentListenersMsg =
    | Get of EventTypeID * AsyncReplyChannel<EventListener[]>
    | GetMap of AsyncReplyChannel<Map<EventTypeID,EventListener[]> >
    | Init of Map<EventTypeID,EventListener[]>
    | Register of EventListener
    | RegisterMany of EventListener[]


type agent_EventListeners() =

    let agent =
        let mutable _map = Map.empty:Map<EventTypeID,EventListener[]>
        MailboxProcessor<agentListenersMsg>.Start(
            fun inbox ->
                async { 
                    while true do
                        let! msg = inbox.Receive()
                        let add (listener:EventListener) = 
                            _map <- map_AppendToArray_NonUnique _map listener.Type listener
                        match msg with 
                        | Get (etid,replyChannel) -> 
                            replyChannel.Reply(
                                match _map.ContainsKey etid with
                                | false -> Array.empty
                                | true -> _map.Item etid)
                        | GetMap replyChannel -> 
                            replyChannel.Reply(_map)
                        | Init start ->
                            _map <- start
                        | Register listener -> 
                            add listener
                        | RegisterMany listeners -> listeners |> Array.iter add
                }
            )

    member _.get etid = agent.PostAndReply (fun replyChannel -> Get (etid,replyChannel)) 
    member _.getMap = agent.PostAndReply GetMap
    member _.init start = agent.Post (Init start)
    member _.register l = agent.Post (Register l)
    member _.registers ls = agent.Post (RegisterMany ls)

