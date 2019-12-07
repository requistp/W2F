module rec EatingSystem
open CommonTypes
open Component
open EatingComponent
open FoodComponent
open EntityAndGameTypes
open System

let eat (game:Game) (eid:EntityID) =
    let (Eating eat) = Entities.getComponent game.Entities EatingComponent eid
    eat
    |> getEdibleFoodsAtLocation game.Entities
    |> Array.sortByDescending (fun f -> f.FoodType.Calories) // Highest caloric food first
    |> function
    | [||] -> { game with Log = LogManager.log_ComponentUpdate game.Log "Err" "Eating System" "eat" eid eat.ID (Some "No food at location") }
    | fs -> 
        let f = fs.[0]
        let eatenQuantity = Math.Clamp(eat.QuantityPerAction, 0, Math.Min(f.Quantity,eat.QuantityRemaining)) // Clamp by how much food is left and how much stomach space is left
        let calories = eatenQuantity * f.FoodType.Calories
        let newFoodQuantity = Math.Clamp(f.Quantity - eatenQuantity, 0, f.QuantityMax)
        let allEaten = newFoodQuantity = 0
        let killFood = allEaten && f.FoodType.KillOnAllEaten
        let note = sprintf "EateeID: %i. EatenQuantity: +%i=%i. Calories: +%i=%i. FoodQuantity:%i. All eaten:%b, kill:%b" (f.EntityID.ToUint32) eatenQuantity (eat.Quantity+eatenQuantity) calories (eat.Calories+calories) newFoodQuantity allEaten killFood
        {
            game with 
                Entities = 
                    Entities.updateComponents 
                        game.Entities
                        [|
                            Eating { eat with Quantity = eat.Quantity + eatenQuantity; Calories = eat.Calories+calories }
                            Food { f with Quantity = newFoodQuantity }
                        |]
                    |> (fun e -> if killFood then Entities.removeEntity e f.EntityID else e)
                Log = LogManager.log_ComponentUpdate game.Log "Ok" "Eating System" "eat" eid eat.ID (Some note)
        }

let getEdibleFoods (eat:EatingComponent) (foods:FoodComponent[]) =
    foods
    |> Array.filter (fun f -> f.Quantity > 0 && canEat eat f.FoodType) // Types I can eat & Food remaining

let getEdibleFoodsAtLocation (ent:Entities) (eat:EatingComponent) =
    eat.EntityID
    |> Entities.getLocation ent 
    |> FoodSystem.getFoodsAtLocation ent
    |> getEdibleFoods eat
   
let eatActionEnabled (ent:Entities) (eid:EntityID) =
    let (Eating eat) = Entities.getComponent ent EatingComponent eid
    (eat.QuantityRemaining > 0) && ((getEdibleFoodsAtLocation ent eat).Length > 0)

let metabolize (game:Game) (eid:EntityID) = 
    let (Eating eat) = Entities.getComponent  game.Entities EatingComponent eid
    let newC = eat.Calories - eat.CaloriesPerMetabolize
    let newQ = eat.Quantity - eat.QuantityPerMetabolize
    let starving = newC < 0
    let note = sprintf "Quantity:-%i=%i. Calories:-%i=%i. Starving:%b" eat.QuantityPerMetabolize newQ eat.CaloriesPerMetabolize newC starving
    //if starving then evm.RaiseEvent (Starving eat) 
    {
        game with 
            Entities = Entities.updateComponent game.Entities (Eating { eat with Quantity = newQ; Calories = newC })
            Log = LogManager.log_ComponentUpdate game.Log "Ok" "Eating System" "metabolize" eat.EntityID eat.ID (Some note)
    }
    


(*
member private me.onComponentAdded round (ComponentAdded_Eating eat:GameEventData) =
    evm.AddToSchedule { ScheduleType = RepeatIndefinitely; Frequency = MetabolismFrequency; GameEvent = Metabolize eat }
    Ok (Some (sprintf "Queued Metabolize to schedule. EntityID:%i" eat.EntityID.ToUint32))
    
member private me.onMetabolize round (Metabolize eat:GameEventData) =
    let newC = eat.Calories - eat.CaloriesPerMetabolize
    let newQ = eat.Quantity - eat.QuantityPerMetabolize
    let starving = newC < 0
    let result = sprintf "Quantity:-%i=%i. Calories:-%i=%i. Starving:%b" eat.QuantityPerMetabolize newQ eat.CaloriesPerMetabolize newC starving
    if starving then evm.RaiseEvent (Starving eat) 
    enm.UpdateComponent (Eating { eat with Quantity = newQ; Calories = newC })
    Ok (Some result)
*)