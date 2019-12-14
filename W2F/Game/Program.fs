open EngineTypes
open GameEvents

let rec gameLoop (game:Game) = 
    game
    |> Renderer.renderWorld 
    |> Renderer.renderRound
    |> ControllerSystem.getInputs
    |> ControllerSystem.processInputs
    |> function
        | g when g.ExitGame -> 
            g
            |> Engine.Persistance.save
            |> ignore // Exits
        | g ->
            g
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
|> Engine.Settings.setMapSize { X = 1000s; Y = 10s; Z = 1s }
|> Engine.Settings.setRenderMode RenderTypes.Skip
|> Engine.Settings.setSaveEveryRound false
|> Engine.Settings.setSaveFormat SaveGameFormats.XML
|> BuildWorld.createTerrain 
|> BuildWorld.makeGrass 5u
|> BuildWorld.makeRabbits false 3u
|> gameLoop


//|> Renderer.renderWorld
//|> Game.Persistance.save LoadAndSave.XML


//Game.Persistance.load LoadAndSave.XML "Save_201912051704_r18"
//|> gameLoop


