module agent_ComponentTypes
open CommonTypes


type private agent_CurrentMsg = 
    | Add of AbstractComponent
    | AddMany of AbstractComponent[]
    | Get of ComponentTypeID * AsyncReplyChannel<ComponentID[]>
    | GetMap of AsyncReplyChannel<Map<ComponentTypeID,ComponentID[]> >
    | Init of Map<ComponentTypeID,ComponentID[]>
    | Remove of AbstractComponent
    | RemoveMany of AbstractComponent[]

        
type agent_ComponentTypes() = //compMan:agent_Components) = 

    let agent =
        let mutable _map = Map.empty<ComponentTypeID,ComponentID[]>
        MailboxProcessor<agent_CurrentMsg>.Start(
            fun inbox ->
                async { 
                    while true do
                        let! msg = inbox.Receive()
                        let others ct cid = 
                            _map.Item(ct) |> Array.filter (fun c -> c <> cid)
                        let add (comp:AbstractComponent) = 
                            _map <- 
                                match _map.ContainsKey(comp.ComponentType) with
                                | false -> _map.Add(comp.ComponentType,[|comp.ID|])
                                | true -> 
                                    let o = others comp.ComponentType comp.ID // In case component was already here
                                    _map.Remove(comp.ComponentType).Add(comp.ComponentType,Array.append o [|comp.ID|]) 
                        let remove (comp:AbstractComponent) =
                            if (_map.ContainsKey(comp.ComponentType)) then
                                _map <- _map.Remove(comp.ComponentType).Add(comp.ComponentType,others comp.ComponentType comp.ID) 
                        match msg with
                        | Add comp -> add comp
                        | AddMany cts -> cts |> Array.iter add
                        | Get (ctid,replyChannel) -> 
                            replyChannel.Reply(
                                match _map.ContainsKey ctid with
                                | false -> Array.empty
                                | true -> _map.Item ctid)
                        | GetMap replyChannel -> replyChannel.Reply(_map)
                        | Init startMap -> _map <- startMap
                        | Remove comp -> remove comp
                        | RemoveMany cts -> cts |> Array.iter remove
                }
            )
    
    member _.add comp = agent.Post (Add comp)
    member _.adds cts = agent.Post (AddMany cts)
    member _.get ctid = agent.PostAndReply (fun replyChannel -> Get (ctid,replyChannel)) 
    member _.getMap = agent.PostAndReply GetMap
    member _.init (start:Map<ComponentTypeID,ComponentID[]>) = agent.Post (Init start)
    member _.remove comp = agent.Post (Remove comp)
    member _.removes cts = agent.Post (RemoveMany cts)

