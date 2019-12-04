open CommonFunctions
open CommonTypes
open EntityAndGameTypes
open System


let rec gameLoop (game:Game) = 
    // Display Game
    Renderer.renderWorld game
    Renderer.renderRound game

    // Get input for all entities
    game
    |> ControllerSystem.getInputForAllEntities
    |> function
    | g when g.ExitGame -> ()
    | g ->
        g
        // Run systems


        |> LogManager.write
        |> Game.incrementRound
        |> gameLoop

Game.empty
|> Game.setMapSize { X = 80s; Y = 30s; Z = 1s }
|> Game.createTerrain 
|> Game.makeGrass 5u
|> Game.makeRabbits false 5u
|> gameLoop





(*
let g = ControllerSystem.getInputForAllEntities game

// Exit or Continue Game Loop
match g.ExitGame with
| true -> ()
| false -> 
    g
    // Run systems


    |> LogManager.write
    |> Game.incrementRound
    |> gameLoop
    *)
