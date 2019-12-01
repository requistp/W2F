module Game
open CommonFunctions
open CommonTypes
open Component
open ControllerComponent
open EatingComponent
open Entities
open FoodComponent
open FormComponent
open LocationFunctions
open MatingComponent
open TerrainComponent
open VisionComponent


type Game = 
    {
        Entities : Entities
        MapSize : Location
        Round : RoundNumber
    }
    static member empty = 
        {
            Entities = Entities.empty
            MapSize = Location.empty
            Round = RoundNumber(0u)
        }
        

let createTerrain (game:Game) =
    let addTerrain l =         
        let eid = game.Entities.MaxEntityID + 1u
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
                Form { EntityID = eid; Born = RoundNumber(0u); CanSeePast = canSeePast; IsPassable = terrain_IsPassable t; Name = t.ToString(); Symbol = terrain_Symbol t; Location = l }
                Terrain { EntityID = eid; Terrain = t }
            |] 
            // I left this mechanic in place because there will be some component that is appropriate to add to Terrain--like a burrow
            //match food.IsSome with
            //| false -> ()
            //| true -> baseTerrain <- Array.append baseTerrain [|food.Value:>AbstractComponent|]
        baseTerrain
    //start
    mapLocations game.MapSize
    |> Array.fold (fun (g:Game) l -> { g with Entities = Entities.createEntity (g.Entities) (addTerrain l) } ) game

let makeGrass (n:uint32) (game:Game) =
    let make (l:Location) =
        let eid = game.Entities.MaxEntityID + 1u
        [| 
            Food { EntityID = eid; FoodType = Food_Carrot; Quantity = 20; QuantityMax = 20 }
            Form { EntityID = eid; Born = RoundNumber(0u); CanSeePast = true; IsPassable = true; Name = Food_Carrot.ToString(); Symbol = Food_Carrot.Symbol.Value; Location = l }
            PlantGrowth { EntityID = eid; GrowsInTerrain = [|Dirt|]; RegrowRate = 0.1; ReproductionRate = 0.25; ReproductionRange = 5; ReproductionRequiredFoodQuantity = 0.75 }
        |] 
    //start
    match n with 
    | 0u -> game
    | _ -> 
        [|1u..n|] 
        |> Array.fold (fun (g:Game) i -> { g with Entities = Entities.createEntity (g.Entities) (make (Location.random game.MapSize)) } ) game

let makeRabbits (firstIsHuman:bool) (n:uint32) (game:Game) = 
    let make (l:Location) (i:uint32) = 
        let eid = game.Entities.MaxEntityID + 1u
        let controller = 
            match n with
            | 1u -> Controller { EntityID = eid; ControllerType = (if firstIsHuman then Keyboard else AI_Random); CurrentAction = Idle; CurrentActions = [|Idle|]; PotentialActions = [|Idle|] }
            | _ -> Controller { EntityID = eid; ControllerType = AI_Random; CurrentAction = Idle; CurrentActions = [|Idle|]; PotentialActions = [|Idle|] }
        let matingStatus = if i = 1u || random.Next(0,2) = 0 then Male else Female
        let symbol = if matingStatus = Male then 'R' else 'r' // Handy for debugging: n.ToString().ToCharArray().[0]
        let visionRange = 10s
        let rangeTemplate = rangeTemplate2D visionRange
        let visionMap = locationsWithinRange2D game.MapSize l rangeTemplate //(rangeTemplate2D visionRange)
        let visionCalculationType = Shadowcast1
        let baseBunny = 
            [|
                controller
                Eating { EntityID = eid; Calories = 150; CaloriesPerDay = 300; Foods = [|Food_Carrot;Food_Grass|]; Quantity = 75; QuantityMax = 150; QuantityPerAction = 1 }
                Form { EntityID = eid; Born = RoundNumber(0u); CanSeePast = true; IsPassable = true; Name = "rabbit"; Symbol = symbol; Location = l }
                Mating { EntityID = eid; ChanceOfReproduction = 0.9; LastMatingAttempt = RoundNumber(0u); MatingStatus = matingStatus; Species = Rabbit }
                Movement { EntityID = eid; MovesPerTurn = 1 }
                Vision { EntityID = eid; LocationsWithinRange = visionMap; Range = visionRange; RangeTemplate = rangeTemplate; ViewedHistory = Map.empty; VisibleLocations = Map.empty; VisionCalculationType = visionCalculationType }
            |]
        baseBunny
    //start
    match n with 
    | 0u -> game
    | _ -> 
        [|1u..n|]
        |> Array.fold (fun (g:Game) i -> { g with Entities = Entities.createEntity (g.Entities) (make (Location.random game.MapSize) i) } ) game

let setMapSize (l:Location) (game:Game) =
    {
        game with MapSize = l
    }



