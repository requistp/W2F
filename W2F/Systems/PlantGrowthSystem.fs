module PlantGrowthSystem
open CalendarTimings
open CommonFunctions
open Components
open Engine
open EngineTypes
open GameEvents
open LocationFunctions


let onComponentAdded (game:Game) (e:AbstractEventData) = 
    match (e :?> EngineEvent_ComponentAdded).Component.ComponentType = PlantGrowth.TypeID with
    | false -> game
    | true -> 
        let pg = Entities.getComponent game.Entities PlantGrowth.TypeID e.EntityID |> ToPlantGrowth
        game
        |> ifBind (pg.RegrowRate > 0.0)       (Scheduler.addToSchedule { ScheduleType = RepeatIndefinitely; Frequency = PlantGrowthFrequency; Event = PlantRegrowth(e.EntityID) })
        |> ifBind (pg.ReproductionRate > 0.0) (Scheduler.addToSchedule { ScheduleType = RepeatIndefinitely; Frequency = PlantGrowthFrequency; Event = PlantReproduce(e.EntityID) })



let onReproduce (game:Game) (e:AbstractEventData) =
    let pg = Entities.getComponent game.Entities ComponentTypes.PlantGrowth.TypeID e.EntityID :?> PlantGrowthComponent
    let createPlant (l:Location) = 
        let adjustComponents (c:AbstractComponent) =
            c
            |> ifBind 
                (c.ComponentType = ComponentTypes.Food.TypeID) 
                (fun ac -> 
                    let f = ToFood ac
                    FoodComponent(f.ID, f.EntityID, f.FoodType, 1, f.QuantityMax).Abstract)
            |> ifBind 
                (c.ComponentType = ComponentTypes.Form.TypeID) 
                (fun ac -> 
                    let f = ToForm ac
                    FormComponent(f.ID, f.EntityID, game.Round, f.CanSeePast, f.IsPassable, l, f.Name, f.Symbol).Abstract)
        e.EntityID
        |> Entities.copy game 
        |> Array.map adjustComponents
        |> Entities.create game

    let checkReproductionRate r = 
        match pg.ReproductionRate >= r with
        | false -> Error (sprintf "Failed: reproduction rate (%f<%f)" pg.ReproductionRate r)
        | true -> Ok None

    let checkOnMap _ =
        let newLocation = addOffset (Entities.getLocation game.Entities pg.EntityID) pg.ReproductionRange pg.ReproductionRange 0 false true
        match isOnMap2D game.MapSize newLocation with
        | false -> Error (sprintf "Failed: location not on map:%s" (newLocation.ToString())) 
        | true -> Ok newLocation

    let checkPlantAtLocation newLocation = 
        match (Engine.Entities.getAtLocationWithComponent game.Entities PlantGrowth.TypeID None newLocation).Length with 
        | x when x > 0 -> Error (sprintf "Failed: plant exists at location:%s" (newLocation.ToString()))
        | _ -> Ok newLocation

    let terrainIsSuitable newLocation = 
        match pg.GrowsInTerrain |> Array.contains (ToTerrain (Engine.Entities.getAtLocationWithComponent game.Entities Terrain.TypeID None newLocation).[0]).Terrain with
        | false -> Error "Failed: terrain is not suitable"
        | true -> Ok newLocation

    let checkFoodOnParent newLocation = 
        match (Engine.Entities.tryGetComponent game.Entities ComponentTypes.Food.TypeID pg.EntityID) with
        | None -> Ok (createPlant newLocation)
        | Some ac ->
            let f = ToFood ac
            let pct = float f.Quantity / float f.QuantityMax
            match pg.ReproductionRequiredFoodQuantity < pct with
            | false -> Error (sprintf "Failed: food component quantity below requirement (%f<%f)" pct pg.ReproductionRequiredFoodQuantity)
            | true -> Ok (createPlant newLocation)

    Ok (random.NextDouble())
    |> Result.bind checkReproductionRate
    |> Result.bind checkOnMap
    |> Result.bind checkPlantAtLocation
    |> Result.bind terrainIsSuitable
    |> Result.bind checkFoodOnParent
    |> Result.mapError (fun e -> Engine.Log.append game (Logging.format1 "Err" "Plant Growth" "onReproduce" pg.EntityID (Some pg.ID) (Some e)) )
    |> function
        | Error ge -> ge
        | Ok go -> go
    




