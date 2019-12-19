module FoodSystem
open Components
open EngineTypes
open System


let getFoodsAtLocation (ent:Entities) (loc:Location) =
    loc 
    |> Engine.Entities.get_AtLocationWithComponent ent ComponentTypes.Food.TypeID None
    |> ToFoods


let onRegrowth (game:Game) (e:AbstractEventData) =
    match (Engine.Entities.tryGet_Component game.Entities ComponentTypes.Food.TypeID e.EntityID) with
    | None -> Engine.Log.append (Logging.format1 "Err" "FoodSystem" "onRegrowth" e.EntityID None (Some "No food component")) game
    | Some ac -> 
        let f = ac :?> FoodComponent
        let pg = Engine.Entities.get_Component game.Entities ComponentTypes.PlantGrowth.TypeID e.EntityID :?> PlantGrowthComponent
        let missing = f.QuantityMax - f.Quantity
        match (missing, pg.RegrowRate) with
        | (0,_  ) -> Engine.Log.append (Logging.format1 "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some "Already fully grown")) game
        | (_,0.0) -> Engine.Log.append (Logging.format1 "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some "Zero regrow rate")) game
        | (_,_  ) -> 
            let quantity = Math.Clamp((int (Math.Round(pg.RegrowRate * (float f.QuantityMax),0))), 1, missing)
            Engine.Entities.updateComponent 
                game 
                (FoodComponent(f.ID, f.EntityID, f.FoodType, f.Quantity + quantity, f.QuantityMax))
                (Some (Logging.format1 "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some (sprintf "Regrown quantity:%i" quantity))))

