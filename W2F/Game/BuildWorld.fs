module BuildWorld
open CommonFunctions
open EngineTypes
open ComponentEnums
open Components
open LocationFunctions


let createTerrain (game:Game) : Game =
    let make l (eid:EntityID) (cid:ComponentID) = 
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
                FormComponent(cid + 1u, eid, RoundNumber(0u), canSeePast, t.IsPassable, l, "Terrain", t.Symbol).Abstract
                TerrainComponent(cid + 2u, eid, t).Abstract
            |] 
            // I left this mechanic in place because there will be some component that is appropriate to add to Terrain--like a burrow
            //match food.IsSome with
            //| false -> ()
            //| true -> baseTerrain <- Array.append baseTerrain [|food.Value:>AbstractComponent|]
        baseTerrain
    //start
    game.MapSize
    |> mapLocations 
    |> Array.fold (fun (g:Game) l -> 
        Engine.Entities.create g (make l g.Entities.NewEntityID g.Entities.MaxComponentID)
        ) game


let makeGrass (n:uint32) (game:Game) : Game =
    let make (l:Location) (eid:EntityID) (cid:ComponentID) =
        [| 
            FoodComponent(cid + 1u, eid, Food_Carrot, 20, 20).Abstract
            FormComponent(cid + 2u, eid, RoundNumber(0u), true, true, l, Food_Carrot.ToString(), Food_Carrot.Symbol.Value).Abstract
            PlantGrowthComponent(cid + 3u, eid, [|Dirt|], 0.1, 0.25, 5, 0.75).Abstract
        |] 
    //start
    match n with 
    | 0u -> game
    | _ -> 
        [|1u..n|] 
        |> Array.fold (fun (g:Game) _ -> 
            Engine.Entities.create g (make (Location.random game.MapSize) g.Entities.NewEntityID g.Entities.MaxComponentID)
            ) game


let makeRabbits (firstIsHuman:bool) (total:uint32) (game:Game) : Game = 
    let make (l:Location) (eid:EntityID) (cid:ComponentID) (i:uint32) = 
        let matingStatus = if i = 1u || random.Next(0,2) = 0 then Male else Female
        let symbol = if matingStatus = Male then 'R' else 'r' // Handy for debugging: n.ToString().ToCharArray().[0]
        let visionRange = 10s
        let rangeTemplate = rangeTemplate2D visionRange
        let visionMap = locationsWithinRange2D game.MapSize l rangeTemplate 
        let visionCalculationType = Shadowcast1
        let baseBunny = 
            [|
                EatingComponent(cid + 2u, eid, 150, 300, [|Food_Carrot;Food_Grass|], 75, 150, 1).Abstract
                FormComponent(cid + 3u, eid, RoundNumber(0u), true, true, l, "rabbit", symbol).Abstract
                MatingComponent(cid + 4u, eid, 0.9, RoundNumber(0u), matingStatus, Rabbit).Abstract
                MovementComponent(cid + 5u, eid, 1).Abstract
                VisionComponent(cid + 6u, eid, visionMap, visionRange, rangeTemplate, visionCalculationType, Map.empty, Map.empty).Abstract
            |]
        Array.append 
            baseBunny
            [| ControllerComponent(cid + 1u, eid, (if firstIsHuman && i = 1u then Keyboard else AI_Random), Idle, [|Idle|], ControllerSystem.getPotentialActions baseBunny).Abstract |]
    //start
    match total with 
    | 0u -> game
    | _ -> 
        [|1u..total|]
        |> Array.fold (fun (g:Game) i -> 
            Engine.Entities.create g (make (Location.random game.MapSize) g.Entities.NewEntityID g.Entities.MaxComponentID i)
            ) game


