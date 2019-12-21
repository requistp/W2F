module BuildWorld
open CommonFunctions
open EngineTypes
open ComponentEnums
open Components
open LocationFunctions


let createTerrain (game:Game) =
    let mutable _eid = game.Entities.NewEntityID
    let mutable _cid = game.Entities.NewComponentID
    let make l = 
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
                FormComponent(_cid + 0u, _eid, RoundNumber(0u), canSeePast, t.IsPassable, l, "Terrain", t.Symbol).Abstract
                TerrainComponent(_cid + 1u, _eid, t).Abstract
            |] 
            // I left this mechanic in place because there will be some component that is appropriate to add to Terrain--like a burrow
            //match food.IsSome with
            //| false -> ()
            //| true -> baseTerrain <- Array.append baseTerrain [|food.Value:>AbstractComponent|]
        _eid <- _eid + 1u
        _cid <- _cid + baseTerrain.Length
        baseTerrain
    //start
    game.MapSize
    |> mapLocations 
    |> Array.collect (fun l -> make l)
    |> Engine.Entities.create game


let makeGrass (n:uint32) (game:Game) : Game =
    let mutable _eid = game.Entities.NewEntityID
    let mutable _cid = game.Entities.NewComponentID
    let make (l:Location) =
        let grass = 
            [| 
                FoodComponent(_cid + 0u, _eid, Food_Carrot, 20, 20).Abstract
                FormComponent(_cid + 1u, _eid, RoundNumber(0u), true, true, l, Food_Carrot.ToString(), Food_Carrot.Symbol.Value).Abstract
                PlantGrowthComponent(_cid + 2u, _eid, [|Dirt|], 0.1, 0.25, 5, 0.75).Abstract
            |] 
        _eid <- _eid + 1u
        _cid <- _cid + grass.Length
        grass
    //start
    match n with 
    | 0u -> game
    | _ -> 
        [|1u..n|] 
        |> Array.collect (fun _ -> make (Location.random game.MapSize))
        |> Engine.Entities.create game


let makeRabbits (firstIsHuman:bool) (total:uint32) (game:Game) : Game = 
    let mutable _eid = game.Entities.NewEntityID
    let mutable _cid = game.Entities.NewComponentID
    let make (l:Location) (i:uint32) = 
        let matingStatus = if i = 1u || random.Next(0,2) = 0 then Male else Female
        let symbol = if matingStatus = Male then 'R' else 'r' // Handy for debugging: n.ToString().ToCharArray().[0]
        let visionRange = 10s
        let rangeTemplate = rangeTemplate2D visionRange
        let visionMap = locationsWithinRange2D game.MapSize l rangeTemplate 
        let visionCalculationType = VisionCalculationTypes.Shadowcast1
        let baseBunny = 
            [|
                ControllerComponent(_cid + 0u, _eid, (if firstIsHuman && i = 1u then Keyboard else AI_Random), Idle, [|Idle|], [|Idle|]).Abstract
                EatingComponent(_cid + 1u, _eid, 150, 300, [|Food_Carrot;Food_Grass|], 75, 150, 1).Abstract
                FormComponent(_cid + 2u, _eid, RoundNumber(0u), true, true, l, "rabbit", symbol).Abstract
                MatingComponent(_cid + 3u, _eid, 0.9, RoundNumber(0u), matingStatus, Rabbit).Abstract
                MovementComponent(_cid + 4u, _eid, 1).Abstract
                VisionComponent(_cid + 5u, _eid, visionMap, visionRange, rangeTemplate, visionCalculationType, Map.empty, Map.empty).Abstract
            |]
        _eid <- _eid + 1u
        _cid <- _cid + baseBunny.Length
        baseBunny        
    //start
    match total with 
    | 0u -> game
    | _ -> 
        [|1u..total|]
        |> Array.collect (fun i -> make (Location.random game.MapSize) i)
        |> Engine.Entities.create game


