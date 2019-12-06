open CommonFunctions
open CommonTypes
open EntityAndGameTypes
open System


let rec gameLoop (game:Game) = 
    Renderer.renderWorld game
    Renderer.renderRound game

    game
    |> ControllerSystem.getInputForAllEntities
    |> function
    | g when g.ExitGame -> 
        g
        |> LoadAndSave.save LoadAndSave.Binary
    | g ->
        g
        |> ControllerSystem.handleInputForAllEntities

        |> LogManager.write
        |> Game.incrementRound
        |> gameLoop

Game.empty
|> Game.setMapSize { X = 50s; Y = 50s; Z = 1s }
|> Game.createTerrain 
|> Game.makeGrass 5u
|> Game.makeRabbits true 3u
|> gameLoop


//LoadAndSave.load LoadAndSave.Binary "Save_201912051704_r18"
//|> gameLoop


