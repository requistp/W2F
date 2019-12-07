open CommonFunctions
open CommonTypes
open EntityAndGameTypes


let rec gameLoop (game:Game) = 
    Renderer.renderWorld game
    Renderer.renderRound game

    game
    |> ControllerSystem.getInputForAllEntities
    |> function
    | g when g.ExitGame -> 
        g
        |> LoadAndSave.save LoadAndSave.XML
    | g ->
        g
        |> ControllerSystem.handleInputForAllEntities
        |> SchedulingSystem.executeScheduledEvents

        |> LogManager.write
        |> Game.incrementRound
        |> gameLoop

Game.empty
|> Game.setMapSize { X = 100s; Y = 25s; Z = 1s }
|> Game.createTerrain 
|> Game.makeGrass 5u
|> Game.makeRabbits false 3u
|> gameLoop


//LoadAndSave.load LoadAndSave.XML "Save_201912051704_r18"
//|> gameLoop


