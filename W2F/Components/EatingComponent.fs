module EatingComponent
open CalendarTimings
open CommonTypes
open FoodComponent
open System


type EatingComponent = 
    { 
        ID : ComponentID
        EntityID : EntityID
        Calories : int
        CaloriesPerDay : int
        Foods : FoodTypes[]
        Quantity : int
        QuantityMax : int
        QuantityPerAction : int 
    } 
    member me.CaloriesPerMetabolize = Math.Clamp(convertAmountByFrequency me.CaloriesPerDay Day MetabolismFrequency,1,me.CaloriesPerDay)
    member me.QuantityPerMetabolize = Math.Clamp(convertAmountByFrequency me.QuantityMax Day MetabolismFrequency,1,me.QuantityMax)
    member me.QuantityRemaining = me.QuantityMax - me.Quantity
    

let canEat (eat:EatingComponent) (fd:FoodTypes) = eat.Foods |> Array.contains fd    