module Events
open CommonFunctions
open GameTypes


let private add (el:EventListener) (game:Game) : Game =
    { 
        game with 
            EventListeners = map_AppendToArray_NonUnique game.EventListeners el.EventType el
            Log = Logger.log game.Log (sprintf "%-3s | %-20s -> %A : %s" "Ok" "Event Listener" el.EventType (el.Action.ToString()))
    }
    

let register (els:EventListener[]) (game:Game) : Game =
    els 
    |> Array.fold (fun g el -> add el g) game


let execute (e:EventData) (game:Game) : Game =
    match (game.EventListeners.ContainsKey e.Type) with
    | false -> game
    | true -> 
        game.EventListeners.Item(e.Type)
        |> Array.fold (fun (g:Game) el ->
            el.Action g e
            ) game

