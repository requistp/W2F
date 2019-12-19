module agent_Components
open agent_IDManager
open CommonFunctions
open CommonTypes


type private agent_CurrentMsg = 
    | Add of AbstractComponent
    | AddMany of AbstractComponent[]
    | Get of ComponentID * AsyncReplyChannel<AbstractComponent option>
    | GetMany of ComponentID[] * AsyncReplyChannel<AbstractComponent[]>
    | GetMap of AsyncReplyChannel<Map<ComponentID,AbstractComponent> >
    | Init of Map<ComponentID,AbstractComponent>
    | Remove of AbstractComponent
    | RemoveMany of AbstractComponent[]
    | Update of AbstractComponent
    | UpdateMany of AbstractComponent[]

    
type agent_Components() =
    let idMan = new agent_IDManager()

    let agent =
        let mutable _map = Map.empty<ComponentID,AbstractComponent>
        MailboxProcessor<agent_CurrentMsg>.Start(
            fun inbox ->
                async { 
                    while true do
                        let! msg = inbox.Receive()
                        let add (c:AbstractComponent) = 
                            _map <-
                                match _map.ContainsKey(c.ID) with
                                | false -> _map.Add(c.ID,c)
                                | true -> _map.Remove(c.ID).Add(c.ID,c)
                        let get cid =
                            match _map.ContainsKey cid with
                            | false -> None
                            | true -> Some (_map.Item cid)
                        let remove (c:AbstractComponent) =
                            if (_map.ContainsKey(c.ID)) then
                                _map <- _map.Remove(c.ID)
                        let update (c:AbstractComponent) =
                            _map <-
                                match _map.ContainsKey(c.ID) with
                                | false -> _map.Add(c.ID,c)
                                | true -> _map.Remove(c.ID).Add(c.ID,c)

                        match msg with
                        | Add comp -> add comp
                        | AddMany cts -> cts |> Array.iter add
                        | Get (cid,replyChannel) -> replyChannel.Reply(get cid)
                        | GetMany (cids,replyChannel) -> replyChannel.Reply(cids |> Array.choose get)
                        | GetMap replyChannel -> replyChannel.Reply(_map)
                        | Init startMap -> _map <- startMap
                        | Remove c -> remove c
                        | RemoveMany cts -> cts |> Array.iter remove
                        | Update c -> update c
                        | UpdateMany cts -> cts |> Array.iter update
                }
            )

    member _.add c = agent.Post (Add c)
    member _.adds cts = agent.Post (AddMany cts)
    member _.get cid = agent.PostAndReply (fun replyChannel -> Get (cid,replyChannel))
    member _.getID() = ComponentID(idMan.get())
    member _.getMap = agent.PostAndReply GetMap
    member _.gets cids = agent.PostAndReply (fun replyChannel -> GetMany (cids,replyChannel))
    member _.newID() = ComponentID(idMan.newID())
    member _.init (start:Map<ComponentID,AbstractComponent>) (maxID:ComponentID) =
        agent.Post (Init start)
        idMan.init maxID.ToUint32
    member _.remove c = agent.Post (Remove c)
    member _.removes cts = agent.Post (RemoveMany cts)
    member _.update c = agent.Post (Update c)

