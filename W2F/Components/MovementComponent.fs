module MovementComponent
open CommonTypes


type MovementComponent = 
    {
        ID : ComponentID
        EntityID : EntityID
        MovesPerTurn : int 
    }