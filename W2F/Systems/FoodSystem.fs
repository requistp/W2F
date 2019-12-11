module FoodSystem
open CommonTypes
open Component
open GameTypes
open System


let getFoodsAtLocation (ent:Entities) (loc:Location) =
    loc |> Game.Entities.getAtLocationWithComponent ent FoodComponent ToFood None


let onRegrowth (game:Game) (PlantRegrowth eid:EventData) =
    match (Game.Entities.tryGetComponent game.Entities FoodComponent eid) with
    | None -> { game with Log = Logging.log1 game.Log "Err" "FoodSystem" "onRegrowth" eid None (Some "No food component") }
    | Some (Food f) -> 
        let pg = Game.Entities.getComponent game.Entities PlantGrowthComponent ToPlantGrowth eid
        let missing = f.QuantityMax - f.Quantity
        match (missing, pg.RegrowRate) with
        | (0,_  ) -> { game with Log = Logging.log1 game.Log "Ok" "FoodSystem" "onRegrowth" eid (Some pg.ID) (Some "Already fully grown") }
        | (_,0.0) -> { game with Log = Logging.log1 game.Log "Ok" "FoodSystem" "onRegrowth" eid (Some pg.ID) (Some "Zero regrow rate") }
        | (_,_  ) -> 
            let quantity = Math.Clamp((int (Math.Round(pg.RegrowRate * (float f.QuantityMax),0))), 1, missing)
            let forlog = Logging.format1 "Ok" "FoodSystem" "onRegrowth" eid (Some pg.ID) (Some (sprintf "Regrown quantity:%i" quantity))
            Game.Entities.updateComponent game (Food { f with Quantity = f.Quantity + quantity }) (Some forlog)
            //Events.execute (UpdateComponent (Food { f with Quantity = f.Quantity + quantity }, Some log) game
            //{
            //    game with
            //        Entities = Entities.updateComponent game.Entities (Food { f with Quantity = f.Quantity + quantity })
            //        Log = Logger.log2 game.Log "Ok" "FoodSystem" "onRegrowth" eid (Some pg.ID) (Some (sprintf "Regrown quantity:%i" quantity))
            //}

