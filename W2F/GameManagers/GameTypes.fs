module rec GameTypes
open CommonTypes
open Component
open ControllerComponent

type Entities = 
    {
        Components : Map<ComponentID,Component>
        ComponentTypes : Map<ComponentTypes,ComponentID[]>
        Entities : Map<EntityID,ComponentID[]>
        Locations : Map<Location,ComponentID[]>
        MaxComponentID : ComponentID
        MaxEntityID : EntityID
    }
    static member empty = 
        {
            Components = Map.empty
            ComponentTypes = Map.empty
            Entities = Map.empty
            Locations = Map.empty
            MaxComponentID = ComponentID(0u)
            MaxEntityID = EntityID(0u)
        }
    member me.NewEntityID = me.MaxEntityID + 1u

type EventAction = Game -> EventData -> Game

type EventData = 
    | Action_Eat of EntityID
    | Action_ExitGame
    | Action_Movement of ControllerComponent
    | ComponentAdded of Component
    | Metabolize of EntityID
    | PlantRegrowth of EntityID
    member me.EntityID =
        match me with
        | Action_Eat eid -> eid
        | Action_ExitGame -> EntityID(0u)
        | Action_Movement cc -> cc.EntityID
        | ComponentAdded c -> getComponentEntityID c
        | Metabolize eid -> eid
        | PlantRegrowth eid -> eid
    member me.Type = 
        match me with 
        | Action_Eat _ -> Event_Action_Eat
        | Action_ExitGame -> Event_Action_ExitGame
        | Action_Movement _ -> Event_Action_Movement
        | ComponentAdded _ -> Event_ComponentAdded
        | Metabolize _ -> Event_Metabolize
        | PlantRegrowth _ -> Event_PlantRegrowth

type EventTypes = 
    | Event_Action_Eat
    | Event_Action_ExitGame
    | Event_Action_Movement
    | Event_ComponentAdded
    | Event_Metabolize
    | Event_PlantRegrowth

type EventListener = 
    {
        Action : EventAction
        EventType : EventTypes
    }
        
type Game = 
    {
        Entities : Entities
        EventListeners : Map<EventTypes,EventListener[]>
        ExitGame : bool
        Log : string[]
        MapSize : Location
        RenderType : RenderTypes
        Round : RoundNumber
        SaveEveryRound : bool
        SaveFormat : SaveGameFormats
        ScheduledEvents : Map<RoundNumber,ScheduledEvent[]>
    }
    static member empty = 
        {
            Entities = Entities.empty
            EventListeners = Map.empty
            ExitGame = false
            Log = Array.empty
            MapSize = Location.empty
            RenderType = RenderTypes.World
            Round = RoundNumber(0u)
            SaveFormat = SaveGameFormats.XML
            SaveEveryRound = false
            ScheduledEvents = Map.empty
        }

type RenderTypes =
    | Entity
    | Skip
    | World


type ScheduleTypes =
    | RepeatFinite of uint32
    | RepeatIndefinitely
    | RunOnce

type ScheduledEvent = 
    {
        ScheduleType : ScheduleTypes
        Frequency : RoundNumber
        Event : EventData
    }

