open CommonFunctions
open CommonTypes
open EntityAndGameTypes
open System


let rec gameLoop (game:Game) = 
    // Display Game
    Renderer.renderWorld game
    Renderer.renderRound game

    // Get input for all entities
    let g = ControllerSystem.getInputForAllEntities game
       
    // Exit or Continue Game Loop
    match g.ExitGame with
    | true -> ()
    | false -> 
        g
        // Run systems

        // Write log

        // Increase round
        |> Game.incrementRound

        // Loop
        |> gameLoop


Game.empty
|> Game.setMapSize { X = 80s; Y = 30s; Z = 1s }
|> Game.createTerrain 
|> Game.makeGrass 5u
|> Game.makeRabbits false 5u
|> gameLoop




