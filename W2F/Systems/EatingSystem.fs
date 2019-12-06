module rec EatingSystem
open CommonTypes
open Component
open ComponentEnums
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
        let note = sprintf "EateeID: %i. EatenQuantity: +%i=%i. Calories: +%i=%i. FoodQuantity:%i. All eaten:%b" (f.EntityID.ToUint32) eatenQuantity (eat.Quantity+eatenQuantity) calories (eat.Calories+calories) newFoodQuantity allEaten
        {
            game with 
                Entities = 
                    Entities.updateComponents 
                        game.Entities
                        [|
                            Eating { eat with Quantity = eat.Quantity + eatenQuantity; Calories = eat.Calories+calories }
                            Food { f with Quantity = newFoodQuantity }
                        |]
                Log = LogManager.log_ComponentUpdate game.Log "Ok" "Eating System" "eat" eid eat.ID (Some note)
        }

let getEdibleFoods (eat:EatingComponent) (foods:FoodComponent[]) =
    foods
    |> Array.filter (fun f -> canEat eat f.FoodType && f.Quantity > 0) // Types I can eat & Food remaining

let getEdibleFoodsAtLocation (ent:Entities) (eat:EatingComponent) =
    eat.EntityID
    |> Entities.getLocation ent 
    |> FoodSystem.getFoodsAtLocation ent
    |> getEdibleFoods eat
   
let eatActionEnabled (ent:Entities) (eid:EntityID) =
    let (Eating eat) = Entities.getComponent ent EatingComponent eid
    (eat.QuantityRemaining > 0) && ((getEdibleFoodsAtLocation ent eat).Length > 0)



(*
    let eatFood (food:FoodComponent) =
        let quantity = Math.Clamp(eat.QuantityPerAction, 0, Math.Min(food.Quantity,eat.QuantityRemaining)) // Clamp by how much food is left and how much stomach space is left
        let calories = quantity * food.FoodType.Calories
        match quantity with
        | 0 -> Error "Stomach is full"
        | _ -> 
            enm.UpdateComponent (Eating { eat with Quantity = eat.Quantity+quantity; Calories = eat.Calories+calories })
            evm.RaiseEvent (Eaten (eat,food))
            Ok (Some (sprintf "EateeID: %i. Quantity: +%i=%i. Calories: +%i=%i" (food.EntityID.ToUint32) quantity (eat.Quantity+quantity) calories (eat.Calories+calories)))
*)