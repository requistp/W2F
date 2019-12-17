﻿open EngineTypes
open GameEvents

let rec gameLoop (game:Game) = 
    game
    |> function
    | g when g.ExitGame -> 
        g
        |> Engine.Persistance.save
        |> ignore // Exits
    | g ->
        g
        // Custom to game
        |> Renderer.renderWorld 
        |> Renderer.renderRound
        |> ControllerSystem.getInputs
        |> ControllerSystem.processInputs

        // Engine - generic
        |> Engine.Scheduler.executeSchedule
        |> Engine.Log.write
        |> Engine.Settings.saveAfterRound
        |> Engine.Round.increment
        |> gameLoop

Game.empty
|> Engine.Events.registerListeners
    [|
        EventListener("Eating->Action",              EatingSystem.onEat,                 EventTypes.Action_Eat.TypeID)
        EventListener("Controller->ExitGame",        ControllerSystem.onExitGame,        EventTypes.Action_ExitGame.TypeID)
        EventListener("Movement->Action",            MovementSystem.onMovement,          EventTypes.Action_Movement.TypeID)
        EventListener("Eating->ComponentAdded",      EatingSystem.onComponentAdded,      EngineEvent_ComponentAdded.TypeID)
        EventListener("PlantGrowth->ComponentAdded", PlantGrowthSystem.onComponentAdded, EngineEvent_ComponentAdded.TypeID)
        EventListener("Eating->Metabolize",          EatingSystem.onMetabolize,          EventTypes.Metabolize.TypeID)
        EventListener("Food->Regrowth",              FoodSystem.onRegrowth,              EventTypes.PlantRegrowth.TypeID)
        EventListener("PlantGrowth->Reproduce",      PlantGrowthSystem.onReproduce,      EventTypes.PlantReproduce.TypeID)
    |]
|> Engine.Settings.setLoggingOn true
|> Engine.Settings.setMapSize { X = 1000s; Y = 1000s; Z = 1s }
|> Engine.Settings.setRenderMode RenderTypes.Skip
|> Engine.Settings.setSaveEveryRound false
|> Engine.Settings.setSaveFormat SaveGameFormats.XML
//|> Engine.Settings.exitGame

// Initialize
|> BuildWorldBulk.createTerrain 
|> BuildWorld.makeGrass 5u
|> BuildWorld.makeRabbits true 3u

// Game loop
|> gameLoop



//|> Renderer.renderWorld
//|> Game.Persistance.save LoadAndSave.XML


//Game.Persistance.load LoadAndSave.XML "Save_201912051704_r18"
//|> gameLoop


