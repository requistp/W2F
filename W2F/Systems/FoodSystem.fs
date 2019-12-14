module FoodSystem
open Components
open EngineTypes
open GameEvents
open System


let getFoodsAtLocation (ent:Entities) (loc:Location) =
    loc 
    |> Engine.Entities.getAtLocationWithComponent ent ComponentTypes.Food.TypeID None
    |> ToFoods


let onRegrowth (game:Game) (e:AbstractEventData) =
    match (Engine.Entities.tryGetComponent game.Entities ComponentTypes.Food.TypeID e.EntityID) with
    | None -> { game with Log = Logging.log1 game.Log "Err" "FoodSystem" "onRegrowth" e.EntityID None (Some "No food component") }
    | Some ac -> 
        let f = ac :?> FoodComponent
        let pg = Engine.Entities.getComponent game.Entities ComponentTypes.PlantGrowth.TypeID e.EntityID :?> PlantGrowthComponent
        let missing = f.QuantityMax - f.Quantity
        match (missing, pg.RegrowRate) with
        | (0,_  ) -> if game.Settings.LoggingOn then ({ game with Log = Logging.log1 game.Log "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some "Already fully grown") }) else game
        | (_,0.0) -> if game.Settings.LoggingOn then ({ game with Log = Logging.log1 game.Log "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some "Zero regrow rate") }) else game
        | (_,_  ) -> 
            let quantity = Math.Clamp((int (Math.Round(pg.RegrowRate * (float f.QuantityMax),0))), 1, missing)
            Engine.Entities.updateComponent 
                game 
                (FoodComponent(f.ID, f.EntityID, f.FoodType, f.Quantity + quantity, f.QuantityMax))
                (Some (Logging.format1 "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some (sprintf "Regrown quantity:%i" quantity))))

