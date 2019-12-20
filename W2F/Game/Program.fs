open EngineTypes


Game.empty
|> Mechanics.Configuration.set 
    // Pre steps
    [|
        Renderer.renderWorld 
        Renderer.renderRound
    |]
    // Post steps
    [||]
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




