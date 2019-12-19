module rec Game
open agent_Round
open CommonTypes
open EntityManager


type EventAction = Game -> AbstractEventData -> Game


type EventListener = 
    {
        Action : EventAction
        Description : string
        Type : EventTypeID
    }

type Game(entities:EntityManager, mapSize:Location) =
    let roundManager = new agent_Round()

    member _.Entities = entities
    member _.MapSize = mapSize
    member _.Round = roundManager
    

    static member empty = 
        Game(new EntityManager(), Location.empty)


//Log : string[] // Maybe seperate out
//Log = Array.empty
        //EventListeners : Map<EventTypeID,EventListener[]>
        //ExitGame : bool
//Settings : Settings // Maybe seperate out
//ScheduledEvents : Map<RoundNumber,ScheduledEvent[]>
//EventListeners = new agent_
//ExitGame = false
//Settings = Settings.empty
//ScheduledEvents = Map.empty


