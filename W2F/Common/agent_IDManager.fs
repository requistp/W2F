module agent_IDManager


type private agent_IDManagerMsg = 
    | Get of AsyncReplyChannel<uint32>
    | Init of uint32
    | New of AsyncReplyChannel<uint32>


type agent_IDManager() =

    let agent_ID =
        let mutable _maxID = 0u        
        MailboxProcessor<agent_IDManagerMsg >.Start(
            fun inbox ->
                async { 
                    while true do
                        let! msg = inbox.Receive()
                        match msg with
                        | Get replyChannel -> 
                            replyChannel.Reply(_maxID)
                        | Init startID -> 
                            _maxID <- startID
                        | New replyChannel -> 
                            _maxID <- _maxID + 1u
                            replyChannel.Reply(_maxID)
                }
            )

    member _.get() = agent_ID.PostAndReply Get
    member _.init start = agent_ID.Post (Init start)
    member _.newID() = agent_ID.PostAndReply New
    