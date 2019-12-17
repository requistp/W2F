module rec EatingSystem
open CalendarTimings
open CommonFunctions
open ComponentEnums
open Components
open Engine
open EngineTypes
open GameEvents
open System


let private canEat (eater:EatingComponent) (food:FoodTypes) = Array.contains food eater.Foods


let private getEdibleFoods (eat:EatingComponent) (foods:FoodComponent[]) =
    foods
    |> Array.filter (fun f -> f.Quantity > 0 && canEat eat f.FoodType) // Types I can eat & Food remaining


let private getEdibleFoodsAtLocation (ent:Entities) (eat:EatingComponent) =
    eat.EntityID
    |> Engine.Entities.getLocation ent 
    |> FoodSystem.getFoodsAtLocation ent
    |> getEdibleFoods eat
   

let eatActionEnabled (ent:Entities) (eid:EntityID) =
    let eat = Engine.Entities.getComponent ent ComponentTypes.Eating.TypeID eid |> ToEating
    (eat. QuantityRemaining > 0) && ((getEdibleFoodsAtLocation ent eat).Length > 0)


let onComponentAdded (game:Game) (e:AbstractEventData) = 
    match (e :?> EngineEvent_ComponentAdded).Component.ComponentType = ComponentTypes.Eating.TypeID with
    | false -> game
    | true -> 
        Scheduler.addToSchedule 
            { ScheduleType = RepeatIndefinitely; Frequency = MetabolismFrequency; Event = Metabolize(e.EntityID) }
            game
    
    
let onEat (game:Game) (e:AbstractEventData) =
    let eat = Engine.Entities.getComponent game.Entities Eating.TypeID e.EntityID |> ToEating
    eat
    |> getEdibleFoodsAtLocation game.Entities
    |> Array.sortByDescending (fun f -> f.FoodType.Calories) // Highest caloric food first
    |> function
    | [||] -> Engine.Log.append game (Logging.format1 "Err" "Eating System" "eat" e.EntityID (Some eat.ID) (Some "No food at location"))
    | fs -> 
        let f = fs.[0]
        let eatenQuantity = Math.Clamp(eat.QuantityPerAction, 0, Math.Min(f.Quantity,eat.QuantityRemaining)) // Clamp by how much food is left and how much stomach space is left
        let calories = eatenQuantity * f.FoodType.Calories
        let newFoodQuantity = Math.Clamp(f.Quantity - eatenQuantity, 0, f.QuantityMax)
        let allEaten = newFoodQuantity = 0
        let killFood = allEaten && f.FoodType.KillOnAllEaten
        let note = sprintf "EateeID: %i. EatenQuantity: +%i=%i. Calories: +%i=%i. FoodQuantity:%i. All eaten:%b, kill:%b" (f.EntityID.ToUint32) eatenQuantity (eat.Quantity+eatenQuantity) calories (eat.Calories+calories) newFoodQuantity allEaten killFood
        Engine.Entities.updateComponents 
            game
            [|
                EatingComponent(eat.ID, eat.EntityID, eat.Calories + calories, eat.CaloriesPerDay, eat.Foods, eat.Quantity + eatenQuantity, eat.QuantityMax, eat.QuantityPerAction)
                FoodComponent(f.ID, f.EntityID, f.FoodType, newFoodQuantity, f.QuantityMax)
            |]
            (Some (Logging.format1 "Ok" "Eating System" "eat" e.EntityID (Some eat.ID) (Some note)))
        |> ifBind killFood (Engine.Entities.remove f.EntityID)
        

let onMetabolize (game:Game) (e:AbstractEventData) = 
    let eat = Engine.Entities.getComponent game.Entities Eating.TypeID e.EntityID |> ToEating
    let newC = eat.Calories - eat.CaloriesPerMetabolize
    let newQ = eat.Quantity - eat.QuantityPerMetabolize
    let starving = newC < 0
    let note = sprintf "Quantity:-%i=%i. Calories:-%i=%i. Starving:%b" eat.QuantityPerMetabolize newQ eat.CaloriesPerMetabolize newC starving
    Engine.Entities.updateComponent 
        game 
        (EatingComponent(eat.ID, eat.EntityID, newC, eat.CaloriesPerDay, eat.Foods, newQ, eat.QuantityMax, eat.QuantityPerAction)) 
        (Some (Logging.format1 "Ok" "Eating System" "onMetabolize" e.EntityID None (Some note)))
    //if starving then evm.RaiseEvent (Starving eat) 
    //|> ifBind killFood (Events.execute (RemoveEntity f.EntityID))

