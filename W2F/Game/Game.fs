module Game
open CommonFunctions
open CommonTypes
open Component
open ControllerComponent
open EatingComponent
open EntityAndGameTypes
open FoodComponent
open FormComponent
open LocationFunctions
open MatingComponent
open TerrainComponent
open VisionComponent


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
                Form { ID = cid + 1u; EntityID = eid; Born = RoundNumber(0u); CanSeePast = canSeePast; IsPassable = terrain_IsPassable t; Name = t.ToString(); Symbol = terrain_Symbol t; Location = l }
                Terrain { ID = cid + 2u; EntityID = eid; Terrain = t }
            |] 
            // I left this mechanic in place because there will be some component that is appropriate to add to Terrain--like a burrow
            //match food.IsSome with
            //| false -> ()
            //| true -> baseTerrain <- Array.append baseTerrain [|food.Value:>AbstractComponent|]
        baseTerrain
    //start
    mapLocations game.MapSize
    |> Array.fold (fun (g:Game) l -> 
        { 
            g with 
                Entities = Entities.createEntity (g.Entities) (make l (g.Entities.MaxEntityID + 1u) g.Entities.MaxComponentID) 
                Log = LogManager.log_CreateEntity g.Log "Ok" "Game" "createTerrain" (g.Entities.MaxEntityID + 1u)
        } ) game

let incrementRound (game:Game) : Game =
    {
        game with 
            Round = game.Round + 1u 
    }

let makeGrass (n:uint32) (game:Game) : Game =
    let make (l:Location) (eid:EntityID) (cid:ComponentID) =
        [| 
            Food { ID = cid + 1u; EntityID = eid; FoodType = Food_Carrot; Quantity = 20; QuantityMax = 20 }
            Form { ID = cid + 2u; EntityID = eid; Born = RoundNumber(0u); CanSeePast = true; IsPassable = true; Name = Food_Carrot.ToString(); Symbol = Food_Carrot.Symbol.Value; Location = l }
            PlantGrowth { ID = cid + 3u; EntityID = eid; GrowsInTerrain = [|Dirt|]; RegrowRate = 0.1; ReproductionRate = 0.25; ReproductionRange = 5; ReproductionRequiredFoodQuantity = 0.75 }
        |] 
    //start
    match n with 
    | 0u -> game
    | _ -> 
        [|1u..n|] 
        |> Array.fold (fun (g:Game) i -> 
            { 
                g with 
                    Entities = Entities.createEntity (g.Entities) (make (Location.random game.MapSize) (g.Entities.MaxEntityID + 1u) g.Entities.MaxComponentID) 
                    Log = LogManager.log_CreateEntity g.Log "Ok" "Game" "makeGrass" (g.Entities.MaxEntityID + 1u)
            } ) game

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
                Eating { ID = cid + 2u; EntityID = eid; Calories = 150; CaloriesPerDay = 300; Foods = [|Food_Carrot;Food_Grass|]; Quantity = 75; QuantityMax = 150; QuantityPerAction = 1 }
                Form { ID = cid + 3u; EntityID = eid; Born = RoundNumber(0u); CanSeePast = true; IsPassable = true; Name = "rabbit"; Symbol = symbol; Location = l }
                Mating { ID = cid + 4u; EntityID = eid; ChanceOfReproduction = 0.9; LastMatingAttempt = RoundNumber(0u); MatingStatus = matingStatus; Species = Rabbit }
                Movement { ID = cid + 5u; EntityID = eid; MovesPerTurn = 1 }
                Vision { ID = cid + 6u; EntityID = eid; LocationsWithinRange = visionMap; Range = visionRange; RangeTemplate = rangeTemplate; ViewedHistory = Map.empty; VisibleLocations = Map.empty; VisionCalculationType = visionCalculationType }
            |]
        Array.append 
            baseBunny
            [| Controller { ID = cid + 1u; EntityID = eid; ControllerType = (if firstIsHuman && i = 1u then Keyboard else AI_Random); CurrentAction = Idle; CurrentActions = [|Idle|]; PotentialActions = ControllerSystem.getPotentialActions baseBunny } |]
    //start
    match total with 
    | 0u -> game
    | _ -> 
        [|1u..total|]
        |> Array.fold (fun (g:Game) i -> 
            { 
                g with 
                    Entities = Entities.createEntity (g.Entities) (make (Location.random game.MapSize) (g.Entities.MaxEntityID + 1u) g.Entities.MaxComponentID i) 
                    Log = LogManager.log_CreateEntity g.Log "Ok" "Game" "makeRabbits" (g.Entities.MaxEntityID + 1u)
            } ) game

let setMapSize (l:Location) (game:Game) : Game =
    {
        game with MapSize = l
    }



