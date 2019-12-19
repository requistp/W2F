module agent_Entities
open agent_IDManager
open CommonTypes


type private agent_CurrentMsg = 
    | Add of AbstractComponent[]
    | AddMany of AbstractComponent[][]
    | Get of EntityID * AsyncReplyChannel<ComponentID[]>
    | GetMany of EntityID[] * AsyncReplyChannel<ComponentID[][]>
    | GetMap of AsyncReplyChannel<Map<EntityID,ComponentID[]> >
    | Init of Map<EntityID,ComponentID[]>
    | Remove of EntityID


type agent_Entities() =
    let idMan = new agent_IDManager()
    
    let agent =
        let mutable _map = Map.empty<EntityID,ComponentID[]>
        MailboxProcessor<agent_CurrentMsg>.Start(
            fun inbox ->
                async { 
                    while true do
                        let! msg = inbox.Receive()
                        let toIDs (cts:AbstractComponent[]) = cts |> Array.map (fun c -> c.ID)
                        let add (cts:AbstractComponent[]) =
                            let eid = cts.[0].EntityID
                            _map <-
                                match _map.ContainsKey eid with
                                | true -> _map.Remove(eid).Add(eid,toIDs cts)
                                | false -> _map.Add(eid,toIDs cts)
                        let get eid =
                            match _map.ContainsKey eid with
                            | false -> Array.empty
                            | true -> _map.Item eid
                        match msg with
                        | Add cts -> add cts
                        | AddMany ctss -> ctss |> Array.iter add
                        | Get (eid,replyChannel) -> replyChannel.Reply(get eid)
                        | GetMany (eids,replyChannel) -> replyChannel.Reply(eids |> Array.map get)
                        | GetMap replyChannel -> replyChannel.Reply(_map)
                        | Init startMap -> _map <- startMap
                        | Remove eid ->
                            if (_map.ContainsKey eid) then
                                _map <- _map.Remove(eid)
                }
            )

    member _.add cts = agent.Post (Add cts)
    member _.adds ctss = agent.Post (AddMany ctss)
    member _.get eid = agent.PostAndReply (fun replyChannel -> Get (eid,replyChannel))
    member _.getID() = EntityID(idMan.get())
    member _.getMap = agent.PostAndReply GetMap
    member _.gets eids = agent.PostAndReply (fun replyChannel -> GetMany (eids,replyChannel))
    member _.init (start:Map<EntityID,ComponentID[]>) (maxID:EntityID) = 
        agent.Post (Init start)
        idMan.init maxID.ToUint32
    member _.newID() = EntityID(idMan.newID())
    member _.remove eid = agent.Post (Remove eid)
