module LogManager
open CommonTypes
open EntityAndGameTypes


let log (l:string[]) (s:string) = Array.append l [|s|]


let log_ComponentUpdate (l:string[]) (result:string) (system:string) (step:string) (eid:EntityID) (cid:ComponentID) (oo:'a option) = 
    match oo with 
    | None   -> sprintf "%-3s | %-20s -> %-30s #%7i.%-7i" result system step eid.ToUint32 cid.ToUint32
    | Some o -> sprintf "%-3s | %-20s -> %-30s #%7i.%-7i : %A" result system step eid.ToUint32 cid.ToUint32 o
    |> log l


let log_CreateEntity (l:string[]) (result:string) (system:string) (step:string) (eid:EntityID) = 
    sprintf "%-3s | %-20s -> %-30s #%7i" result system step eid.ToUint32
    |> log l 


let write (game:Game) = 
    game.Log
    |> Array.iter (fun s -> Logging.writeLog (sprintf "%7i | %s" game.Round.ToUint32 s))
    { game with Log = Array.empty }
