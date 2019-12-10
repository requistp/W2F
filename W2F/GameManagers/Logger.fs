module Logger
open CommonTypes
open GameTypes


let log (l:string[]) (s:string) = Array.append l [|s|]


let log2 (l:string[]) (result:string) (system:string) (step:string) (eid:EntityID) (cido:ComponentID option) (oo:'a option) = 
    let cids = 
        match cido with
        | None -> ""
        | Some cid -> cid.ToUint32.ToString()
    match oo with 
    | None   -> 
        sprintf "%-3s | %-20s -> %-30s #%7i.%-7s" result system step eid.ToUint32 cids
    | Some s when s.GetType() = typeof<string> -> 
        sprintf "%-3s | %-20s -> %-30s #%7i.%-7s : %s" result system step eid.ToUint32 cids (s.ToString())
    | Some o -> 
        sprintf "%-3s | %-20s -> %-30s #%7i.%-7s : %A" result system step eid.ToUint32 cids o
    |> log l


let write (game:Game) = 
    Async.Parallel
    (
        game.Log |> Array.iter (fun s -> Logging.writeLog (sprintf "%7i | %s" game.Round.ToUint32 s))
    )
    { game with Log = Array.empty }
