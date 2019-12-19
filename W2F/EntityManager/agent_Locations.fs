module agent_Locations
open CommonTypes


type private agent_CurrentMsg =
    | Add of AbstractComponent_WithLocation
    | AddMany of AbstractComponent_WithLocation[]
    | Get of Location * AsyncReplyChannel<ComponentID[]>
    | GetMap of AsyncReplyChannel<Map<Location,ComponentID[]> >
    | Init of Map<Location,ComponentID[]>
    | Move of oldForm:AbstractComponent_WithLocation * newForm:AbstractComponent_WithLocation
    | Remove of AbstractComponent_WithLocation
    | RemoveMany of AbstractComponent_WithLocation[]


type agent_Locations() = 

    let agent =
        let mutable _map = Map.empty<Location,ComponentID[]>
        MailboxProcessor<agent_CurrentMsg>.Start(
            fun inbox ->
                async { 
                    while true do
                        let! msg = inbox.Receive()
                        let others l cid = 
                            _map.Item(l) |> Array.filter (fun c -> c <> cid)
                        let add (c:AbstractComponent_WithLocation) = 
                            _map <-
                                match _map.ContainsKey c.Location with
                                | false -> _map.Add(c.Location,[|c.ID|])
                                | true ->
                                    let o = others c.Location c.ID // In case component was already here
                                    _map.Remove(c.Location).Add(c.Location,Array.append o [|c.ID|]) 
                        let remove (c:AbstractComponent_WithLocation) =
                            if (_map.ContainsKey c.Location) then
                                _map <- _map.Remove(c.Location).Add(c.Location,others c.Location c.ID) 
                        match msg with
                        | Add c -> add c
                        | AddMany cts -> cts |> Array.iter add
                        | Get (location,replyChannel) -> 
                            replyChannel.Reply(
                                match _map.ContainsKey location with
                                | false -> Array.empty
                                | true -> _map.Item location
                                )
                        | GetMap replyChannel -> replyChannel.Reply(_map)
                        | Init startMap -> _map <- startMap
                        | Move (oldForm,newForm) -> 
                            remove oldForm
                            add newForm
                        | Remove c -> remove c
                        | Remove cts -> cts |> remove 
                }
            )
            
    member _.add c = agent.Post (Add c)
    member _.adds cts = agent.Post (AddMany cts)
    member _.get l = agent.PostAndReply (fun replyChannel -> Get (l,replyChannel))
    member _.getMap = agent.PostAndReply GetMap
    member _.init (start:Map<Location,ComponentID[]>) = agent.Post (Init start)
    member _.move oldC newC = agent.Post (Move (oldC,newC))
    member _.remove c = agent.Post (Remove c)
    member _.removes cts = agent.Post (RemoveMany cts)
