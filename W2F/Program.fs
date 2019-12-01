open CommonFunctions
open CommonTypes
open Entities
open Game
open Renderer
open System


let rec gameLoop (game:Game) = 
    // Display Map (last frame)
    renderWorld game
    
    // Get input for all entities
        //if Exit then Exit
        
    // Run systems

    // Write log

    // Loop // Increase round
    gameLoop { game with Round = game.Round + 1u }


let game =
    Game.empty
    |> Game.setMapSize { X = 80s; Y = 30s; Z = 1s }
    |> Game.createTerrain 
    |> Game.makeGrass 5u
    |> Game.makeRabbits true 3u

//renderWorld game

gameLoop game





(*
    
let rec gameLoop level =
    let playerCommand = getPlayerCommand level.playerId
    let turnLevel = level |> runTurn playerCommand
    let player = level |> expectActor turnLevel.playerId
    if not (isAlive player) then
        ()
    else
        render turnLevel
        gameLoop turnLevel
*)