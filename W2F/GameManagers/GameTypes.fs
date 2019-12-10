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
        
type EventAction = Game -> EventData -> Game

type EventData = 
    | Action_Eat of EntityID
    | Action_ExitGame
    | Action_Movement of ControllerComponent
    | CreateEntity of Component[]
    | Metabolize of EntityID
    | RemoveEntity of EntityID
    member me.EntityID =
        match me with
        | Action_Eat eid -> eid
        | Action_ExitGame -> EntityID(0u)
        | Action_Movement cc -> cc.EntityID
        | CreateEntity cts -> getComponentEntityID cts.[0]
        | Metabolize eid -> eid
        | RemoveEntity eid -> eid
    member me.Type = 
        match me with 
        | Action_Eat _ -> Event_Action_Eat
        | Action_ExitGame -> Event_Action_ExitGame
        | Action_Movement _ -> Event_Action_Movement
        | CreateEntity _ -> Event_CreateEntity
        | Metabolize _ -> Event_Metabolize
        | RemoveEntity _ -> Event_RemoveEntity

type EventTypes = 
    | Event_Action_Eat
    | Event_Action_ExitGame
    | Event_Action_Movement
    | Event_CreateEntity
    | Event_Metabolize
    | Event_RemoveEntity

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

type SaveGameFormats =
    | Binary
    | XML
    member me.Ext =
        match me with   
        | Binary -> ".bin"
        | XML -> ".xml"

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

