module Scheduler
open CalendarTimings
open CommonFunctions
open GameTypes


let private add (game:Game) (isNew:bool) (se:ScheduledEvent) = 
    let scheduledRound = 
        game.Round +
        match (isNew && se.ScheduleType = RepeatIndefinitely) with
        | true -> TimingOffset se.Frequency
        | false -> se.Frequency
    { 
        game with 
            ScheduledEvents = map_AppendToArray_NonUnique game.ScheduledEvents scheduledRound se 
            Log = Logging.log1 game.Log "Ok" "Scheduler" "add" se.Event.EntityID None (Some (se.Event.Type.ToString()))
    }


let addToSchedule (game:Game) (se:ScheduledEvent) = add game true se


let executeSchedule (game:Game) =
    match (game.ScheduledEvents.ContainsKey game.Round) with
    | false -> game
    | true ->
        game.ScheduledEvents.Item(game.Round)
        |> Array.fold (fun (g:Game) se ->
            match (Game.Entities.exists g.Entities se.Event.EntityID) with
            | false -> g
            | true -> 
                g
                |> Events.execute se.Event 
                |> function // Reschedule
                | g when se.ScheduleType = RunOnce -> g
                | g -> 
                    match se.ScheduleType with
                    | RepeatFinite x when x = 1u -> g
                    | RepeatFinite x -> add g false { se with ScheduleType = RepeatFinite (x - 1u) }
                    | RepeatIndefinitely -> add g false se
            ) game
        |> function
        | g -> { g with ScheduledEvents = g.ScheduledEvents |> Map.filter (fun k _ -> k > g.Round) }



