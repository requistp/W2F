module rec EngineTypes
open CommonFunctions
open System


[<AbstractClass>]
type AbstractEventData(et:EventTypeID, description:string, eid:EntityID) = 
    member _.Description with get() = description
    member _.EntityID with get() = eid
    member _.Type with get() = et
    member me.Abstract =
        me :> AbstractEventData  


[<AbstractClass>]
type AbstractComponent(cid:ComponentID, eid:EntityID, ct:ComponentTypeID) =
    member _.ID with get() = cid
    member _.EntityID with get() = eid
    member _.ComponentType with get() = ct
    member me.Abstract =
        me :> AbstractComponent  
    abstract Copy : ComponentID -> EntityID -> AbstractComponent


[<AbstractClass>]
type AbstractComponent_WithLocation(id:ComponentID, eid:EntityID, ct:ComponentTypeID, location:Location, isPassable:bool) =
    inherit AbstractComponent(id, eid, ct)
    member _.IsPassable with get() = isPassable
    member _.Location with get() = location
    member me.Abstract =
        me :> AbstractComponent

let ToWithLocation (ac:AbstractComponent) = ac :?> AbstractComponent_WithLocation
let ToWithLocations (acs:AbstractComponent[]) = Array.map ToWithLocation acs


type ComponentID = 
    | ComponentID of uint32
    static member (+) (ComponentID m1, ComponentID m2) = ComponentID(m1 + m2)
    static member (+) (ComponentID m1, m2:uint32) = ComponentID(m1 + m2)
    static member (+) (m1:uint32, ComponentID m2) = ComponentID(m1 + m2)
    static member (+) (ComponentID m1, m2:int) = ComponentID(m1 + uint32 m2)
    member me.ToUint32 = 
        let (ComponentID v) = me
        v


type DistanceType = int16


type ComponentTypeID = byte


type EngineEvent_EntityCreated (eid:EntityID) =
    inherit AbstractEventData(0uy, "EngineEvent_EntityCreated", eid)
    
type EngineEvent_ComponentAdded (ac:AbstractComponent) =
    inherit AbstractEventData(1uy, "EngineEvent_ComponentAdded", ac.EntityID)
    member _.Component with get() = ac

type EngineEvent_EntityRemoved (eid:EntityID) =
    inherit AbstractEventData(2uy, "EngineEvent_EntityRemoved", eid)
    
type EngineEvent_ComponentUpdated (oldc:AbstractComponent, newc:AbstractComponent) =
    inherit AbstractEventData(3uy, "EngineEvent_ComponentUpdated", newc.EntityID)
    member _.OldComponent with get() = oldc
    member _.NewComponent with get() = newc
    

type Entities = 
    {
        Components : Map<ComponentID,AbstractComponent>
        ComponentTypes : Map<ComponentTypeID,ComponentID[]>
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
    member me.NewEntityID = EntityID(me.MaxEntityID.ToUint32 + 1u)
    member me.NewComponentID = ComponentID(me.MaxComponentID.ToUint32 + 1u)


type EntityID = 
    | EntityID of uint32
    static member (+) (EntityID m1, EntityID m2) = EntityID(m1 + m2)
    static member (+) (EntityID m1, m2:uint32) = EntityID(m1 + m2)
    static member (+) (m1:uint32, EntityID m2) = EntityID(m1 + m2)
    static member (+) (EntityID m1, m2:int) = EntityID(m1 + uint32 m2)
    static member (+) (m1:int, EntityID m2) = EntityID(uint32 m1 + m2)
    member me.ToUint32 = 
        let (EntityID v) = me
        v


type EventAction = Game -> AbstractEventData -> Game


type EventListener(description:string, action:EventAction, eventType:EventTypeID) = 
    member _.Action with get() = action
    member _.Description with get() = description
    member _.Type with get() = eventType
   

type EventTypeID = byte


type Game = 
    {
        Entities : Entities
        EventListeners : Map<EventTypeID,EventListener[]>
        ExitGame : bool
        GameLoopSteps : GameLoopStep[]
        Log : string[]
        MapSize : Location
        Renderer_Entity : (Game -> EntityID -> unit) option
        Round : RoundNumber
        ScheduledEvents : Map<RoundNumber,ScheduledEvent[]>
        Settings : Settings
    }
    static member empty = 
        {
            Entities = Entities.empty
            EventListeners = Map.empty
            ExitGame = false
            GameLoopSteps = Array.empty
            Log = Array.empty
            MapSize = Location.empty
            Renderer_Entity = None
            Round = RoundNumber 0u
            ScheduledEvents = Map.empty
            Settings = Settings.empty
        }
    member me.toSave = 
        {
            Entities = me.Entities
            MapSize = me.MapSize
            Round = me.Round
            ScheduledEvents = me.ScheduledEvents
            Settings = me.Settings
        }


type GameSave = 
    {
        Entities : Entities
        MapSize : Location
        Round : RoundNumber
        ScheduledEvents : Map<RoundNumber,ScheduledEvent[]>
        Settings : Settings
    }
    member me.toGame = 
        {
            Entities = me.Entities
            EventListeners = Map.empty
            ExitGame = false
            GameLoopSteps = Array.empty
            Log = Array.empty
            MapSize = me.MapSize
            Renderer_Entity = None
            Round = me.Round
            ScheduledEvents = me.ScheduledEvents
            Settings = me.Settings
        }


type GameLoopStep = Game -> Game


type Location = 
    {
        X : DistanceType
        Y : DistanceType
        Z : DistanceType
    } 
    static member (+) (l1:Location,l2:Location) = { X = l1.X + l2.X; Y = l1.Y + l2.Y; Z = l1.Z + l2.Z }
    static member (-) (l1:Location,l2:Location) = { X = l1.X - l2.X; Y = l1.Y - l2.Y; Z = l1.Z - l2.Z }
    static member empty = { X = 0s; Y = 0s; Z = 0s }
    static member Is000 l = (l = Location.empty)
    static member Offset (rangeX:int) (rangeY:int) (rangeZ:int) (allow000:bool) (doubleRandom:bool) =
        let getNewLocation rnd = 
            {
                X = int16 (random.Next(-rangeX,rangeX+1))
                Y = int16 (random.Next(-rangeY,rangeY+1))
                Z = int16 (random.Next(-rangeZ,rangeZ+1))
            }
        let getNewLocation_double rnd = 
            {
                X = int16 (Math.Round((float (random.Next(-rangeX,rangeX+1)) + float (random.Next(-rangeX,rangeX+1))) / 2.0, 0))
                Y = int16 (Math.Round((float (random.Next(-rangeY,rangeY+1)) + float (random.Next(-rangeY,rangeY+1))) / 2.0, 0))
                Z = int16 (Math.Round((float (random.Next(-rangeZ,rangeZ+1)) + float (random.Next(-rangeZ,rangeZ+1))) / 2.0, 0))
            }
        let newLocation rnd =
            if doubleRandom then getNewLocation_double rnd else getNewLocation rnd

        let mutable l = newLocation random.Next
        while (not allow000 && Location.Is000 l) do
            l <- newLocation random.Next
        l
    static member random (size:Location) = 
        {
            X = int16 (random.Next(0,int size.X))
            Y = int16 (random.Next(0,int size.Y))
            Z = 0s
        }
    override me.ToString() = sprintf "{X=%i,Y=%i,Z=%i}" me.X me.Y me.Z


type MovementDirection =
    | North
    | East
    | South
    | West
    member me.Location =
        match me with
        | North -> { X =  0s; Y = -1s; Z = 0s }
        | South -> { X =  0s; Y =  1s; Z = 0s }
        | East  -> { X =  1s; Y =  0s; Z = 0s }
        | West  -> { X = -1s; Y =  0s; Z = 0s }


type RenderTypes =
    | Entity
    | Skip
    | World


type RoundNumber =  
    | RoundNumber of uint32
    member me.ToUint32 = 
        let (RoundNumber v) = me
        v
    static member (+) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 + m2)
    static member (+) (RoundNumber m1, m2:uint32) = RoundNumber (m1 + m2)
    static member (+) (m1:uint32, RoundNumber m2) = RoundNumber (m1 + m2)
    static member (-) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 - m2)
    static member (-) (RoundNumber m1, m2:uint32) = RoundNumber (m1 - m2)
    static member (-) (m1:uint32, RoundNumber m2) = RoundNumber (m1 - m2)
    static member (*) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 * m2)
    static member (*) (RoundNumber m1, m2:uint32) = RoundNumber (m1 * m2)
    static member (*) (m1:uint32, RoundNumber m2) = RoundNumber (m1 * m2)
    static member (/) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 / m2)
    static member (/) (RoundNumber m1, m2:uint32) = RoundNumber (m1 / m2)
    static member (/) (m1:uint32, RoundNumber m2) = RoundNumber (m1 / m2)
    static member (%) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 % m2)
    static member (%) (RoundNumber m1, m2:uint32) = RoundNumber (m1 % m2)
    static member (%) (m1:uint32, RoundNumber m2) = RoundNumber (m1 % m2)


type SaveGameFormats =
    | Binary
    | XML
    member me.Ext =
        match me with   
        | Binary -> ".bin"
        | XML -> ".xml"


type ScheduledEvent = 
    {
        ScheduleType : ScheduleTypes
        Frequency : RoundNumber
        Event : AbstractEventData
    }


type ScheduleTypes =
    | RepeatFinite of uint32
    | RepeatIndefinitely
    | RunOnce


type Settings = 
    {   
        LoggingOn : bool
        LogLengthMax : int
        RenderType : RenderTypes
        SaveEveryRound : bool
        SaveFormat : SaveGameFormats
        SaveOnExitGameLoop : bool
        SaveComponentsOnly : bool
    }
    static member empty = 
        {   
            LoggingOn = false
            LogLengthMax = 100
            RenderType = RenderTypes.World
            SaveEveryRound = false
            SaveFormat = SaveGameFormats.XML
            SaveOnExitGameLoop = false
            SaveComponentsOnly = true
        }

