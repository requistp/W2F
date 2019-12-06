module FormComponent
open CommonTypes


type FormComponent = 
    { 
        ID : ComponentID
        EntityID : EntityID
        Born : RoundNumber
        CanSeePast : bool
        IsPassable : bool
        Location : Location
        Name : string
        Symbol : char 
    }
   