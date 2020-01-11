open EngineTypes


Game.empty
|> Mechanics.Configuration.set 
    // Pre steps
    [|
        //Mechanics.Vision.updateViewable
        Renderer.renderWorld 
        Renderer.renderRound
        Mechanics.Controller.getInputs
        Mechanics.Controller.processInputs

    |]
    // Post steps
    [||]
|> Engine.Settings.setLogging false
|> Engine.Settings.setRenderEntity (Some Renderer.renderEntity)
|> Engine.Settings.setRenderMode RenderTypes.Skip
|> Engine.Settings.setSaveEveryRound false
|> Engine.Settings.setSaveFormat SaveGameFormats.XML
|> Engine.Settings.setSaveComponentsOnly true
|> Engine.Settings.setSaveOnExitGameLoop false
|> Engine.GameLoop.exit

// Setup map & world stuff
|> Engine.Settings.setMapSize { X = 1000s; Y = 1000s; Z = 1s }
|> BuildWorld.createTerrain 
|> BuildWorld.makeGrass 5u
|> BuildWorld.makeRabbits false 3u
|> Engine.Entities.testCreateIndex

// Start game loop
|> Engine.GameLoop.start
|> ignore




//"Save_201912192332_r6"
//|> Engine.Persistance.load SaveGameFormats.XML 
//|> Mechanics.Configuration.set 
//    //Pre steps
//    [|
//        Renderer.renderWorld 
//        Renderer.renderRound
//    |]
//    //Post steps
//    [||]
//|> Engine.GameLoop.start
//|> ignore




