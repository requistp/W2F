module BuildWorld
open CommonFunctions
open CommonTypes
open ComponentEnums
open Components
open Game
open LocationFunctions


let createTerrain (game:Game) : Game =
    let make l =
        let eid = game.Entities.NewEntityID()
        let t = 
            match random.Next(1,50) with
            | 1 -> Rock
            | _ -> Dirt
        let canSeePast = 
            match t with
            | Rock -> false
            | _ -> true            
        let mutable baseTerrain =
            [| 
                FormComponent(game.Entities.NewComponentID(), eid, RoundNumber(0u), canSeePast, t.IsPassable, l, "Terrain", t.Symbol).Abstract
                TerrainComponent(game.Entities.NewComponentID(), eid, t).Abstract
            |] 
            // I left this mechanic in place because there will be some component that is appropriate to add to Terrain--like a burrow
            //match food.IsSome with
            //| false -> ()
            //| true -> baseTerrain <- Array.append baseTerrain [|food.Value:>AbstractComponent|]
        baseTerrain
    //start
    game.MapSize
    |> mapLocations 
    |> Array.fold (fun m l -> Engine.Entities.create m (make l) ) game

let makeGrass (n:uint32) (game:Game) : Game =
    let make (l:Location) =
        let eid = game.Entities.NewEntityID()
        [| 
            FoodComponent(game.Entities.NewComponentID(), eid, Food_Carrot, 20, 20).Abstract
            FormComponent(game.Entities.NewComponentID(), eid, RoundNumber(0u), true, true, l, Food_Carrot.ToString(), Food_Carrot.Symbol.Value).Abstract
            PlantGrowthComponent(game.Entities.NewComponentID(), eid, [|Dirt|], 0.1, 0.25, 5, 0.75).Abstract
        |] 
    //start
    match n with 
    | 0u -> game
    | _ -> 
        [|1u..n|] 
        |> Array.fold (fun m i -> Engine.Entities.create m (make (Location.random game.MapSize)) ) game
            
let makeRabbits (firstIsHuman:bool) (total:uint32) (game:Game) : Game = 
    let make (l:Location) (i:uint32) = 
        let eid = game.Entities.NewEntityID()
        let matingStatus = if i = 1u || random.Next(0,2) = 0 then Male else Female
        let symbol = if matingStatus = Male then 'R' else 'r' // Handy for debugging: n.ToString().ToCharArray().[0]
        let visionRange = 10s
        let rangeTemplate = rangeTemplate2D visionRange
        let visionMap = locationsWithinRange2D game.MapSize l rangeTemplate 
        let visionCalculationType = Shadowcast1
        let baseBunny = 
            [|
                EatingComponent(game.Entities.NewComponentID(), eid, 150, 300, [|Food_Carrot;Food_Grass|], 75, 150, 1).Abstract
                FormComponent(game.Entities.NewComponentID(), eid, RoundNumber(0u), true, true, l, "rabbit", symbol).Abstract
                MatingComponent(game.Entities.NewComponentID(), eid, 0.9, RoundNumber(0u), matingStatus, Rabbit).Abstract
                MovementComponent(game.Entities.NewComponentID(), eid, 1).Abstract
                VisionComponent(game.Entities.NewComponentID(), eid, visionMap, visionRange, rangeTemplate, visionCalculationType, Map.empty, Map.empty).Abstract
            |]
        Array.append 
            baseBunny
            [| ControllerComponent(game.Entities.NewComponentID(), eid, (if firstIsHuman && i = 1u then Keyboard else AI_Random), Idle, [|Idle|], [|Idle|](*ControllerSystem.getPotentialActions baseBunny*)).Abstract |]
    //start
    match total with 
    | 0u -> game
    | _ -> 
        [|1u..total|]
        |> Array.fold (fun m i -> Engine.Entities.create m (make (Location.random game.MapSize) i) ) game


