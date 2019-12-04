module EntityAndGameTypes
open CommonTypes
open Component
open ComponentEnums


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


type Game = 
    {
        Entities : Entities
        ExitGame : bool
        Log : string[]
        MapSize : Location
        Round : RoundNumber
    }
    static member empty = 
        {
            Entities = Entities.empty
            ExitGame = false
            Log = Array.empty
            MapSize = Location.empty
            Round = RoundNumber(0u)
        }



