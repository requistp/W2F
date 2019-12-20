open EngineTypes
open EventTypes


Game.empty
|> Engine.Events.registerListeners
    [|
        EventListener("Eating->Action",              EatingSystem.onEat,                 EventTypes.Action_Eat.TypeID)
        EventListener("Controller->ExitGame",        ControllerSystem.onExitGame,        EventTypes.Action_ExitGame.TypeID)
        EventListener("Movement->Action",            MovementSystem.onMovement,          EventTypes.Action_Movement.TypeID)
        EventListener("Eating->ComponentAdded",      EatingSystem.onComponentAdded,      Engine_ComponentAdded.TypeID)
        EventListener("Controller->ComponentAdded",  ControllerSystem.onComponentAdded,  Engine_ComponentAdded.TypeID)
        EventListener("PlantGrowth->ComponentAdded", PlantGrowthSystem.onComponentAdded, Engine_ComponentAdded.TypeID)
        EventListener("Eating->Metabolize",          EatingSystem.onMetabolize,          EventTypes.Metabolize.TypeID)
        EventListener("Food->Regrowth",              FoodSystem.onRegrowth,              EventTypes.PlantRegrowth.TypeID)
        EventListener("PlantGrowth->Reproduce",      PlantGrowthSystem.onReproduce,      EventTypes.PlantReproduce.TypeID)
    |]
|> Engine.GameLoop.setSteps
    [|
        Renderer.renderWorld 
        Renderer.renderRound
        ControllerSystem.getInputs
        ControllerSystem.processInputs
    |]
|> Engine.Settings.setLogging true
|> Engine.Settings.setRenderMode RenderTypes.World
|> Engine.Settings.setSaveEveryRound false
|> Engine.Settings.setSaveFormat SaveGameFormats.XML
|> Engine.Settings.setSaveComponentsOnly false
|> Engine.Settings.setSaveOnExitGameLoop true
//|> Engine.GameLoop.exit

// Setup map & world stuff
|> Engine.Settings.setMapSize { X = 100s; Y = 25s; Z = 1s }
|> BuildWorld.createTerrain 
|> BuildWorld.makeGrass 5u
|> BuildWorld.makeRabbits true 3u

// Start game loop
|> Engine.GameLoop.start
|> ignore




//"Save_201912192332_r6"
//|> Engine.Persistance.load SaveGameFormats.XML 
//|> Engine.Events.registerListeners
//    [|
//        EventListener("Eating->Action",              EatingSystem.onEat,                 EventTypes.Action_Eat.TypeID)
//        EventListener("Controller->ExitGame",        ControllerSystem.onExitGame,        EventTypes.Action_ExitGame.TypeID)
//        EventListener("Movement->Action",            MovementSystem.onMovement,          EventTypes.Action_Movement.TypeID)
//        EventListener("Eating->ComponentAdded",      EatingSystem.onComponentAdded,      Engine_ComponentAdded.TypeID)
//        EventListener("Controller->ComponentAdded",  ControllerSystem.onComponentAdded,  Engine_ComponentAdded.TypeID)
//        EventListener("PlantGrowth->ComponentAdded", PlantGrowthSystem.onComponentAdded, Engine_ComponentAdded.TypeID)
//        EventListener("Eating->Metabolize",          EatingSystem.onMetabolize,          EventTypes.Metabolize.TypeID)
//        EventListener("Food->Regrowth",              FoodSystem.onRegrowth,              EventTypes.PlantRegrowth.TypeID)
//        EventListener("PlantGrowth->Reproduce",      PlantGrowthSystem.onReproduce,      EventTypes.PlantReproduce.TypeID)
//    |]
//|> Engine.GameLoop.setSteps
//    [|
//        Renderer.renderWorld 
//        Renderer.renderRound
//        ControllerSystem.getInputs
//        ControllerSystem.processInputs
//    |]
//|> Engine.GameLoop.start
//|> ignore




