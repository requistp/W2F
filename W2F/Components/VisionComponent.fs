module VisionComponent
open CommonTypes
open FormComponent


type VisionCalculationTypes =
    | Basic_Cheating
    | Shadowcast1


type VisionComponent = 
    { 
        //ID : ComponentID
        EntityID : EntityID
        LocationsWithinRange : Location[]                // Locations within range--regardless of being blocked/visible/etc.
        Range : int16
        RangeTemplate : Location[]
        VisionCalculationType : VisionCalculationTypes
        ViewedHistory : Map<Location,FormComponent[]>    // All locations that entity has ever seen, and when
        VisibleLocations : Map<Location,FormComponent[]> // Locations that are visible taking into account occlusion, etc. (i.e. a subset of VisionMap)
    }