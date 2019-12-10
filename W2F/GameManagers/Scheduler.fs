﻿module Scheduler
open CalendarTimings
open CommonFunctions
open CommonTypes
open Component
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
            Log = Logger.log2 game.Log "Ok" "Scheduler" "add" se.Event.EntityID None (Some (se.Event.Type.ToString()))
    }

let executeSchedule (game:Game) =
    match (game.ScheduledEvents.ContainsKey game.Round) with
    | false -> game
    | true ->
        game.ScheduledEvents.Item(game.Round)
        |> Array.fold (fun (g:Game) se ->
            match (Entities.exists g.Entities se.Event.EntityID) with
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

let onCreateEntity (game:Game) (CreateEntity cts:EventData) =
    cts
    |> Array.fold (fun (g:Game) c ->
        match c with
        | Eating eat -> add g true { ScheduleType = RepeatIndefinitely; Frequency = MetabolismFrequency; Event = Metabolize eat.EntityID }
        | _ -> g
        ) game

