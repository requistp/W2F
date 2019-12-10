open CommonFunctions
open CommonTypes
open GameTypes


let rec gameLoop (game:Game) = 
    game
    |> Renderer.renderWorld 
    |> Renderer.renderRound
    |> ControllerSystem.getInputs
    |> function
    | g when g.ExitGame -> 
        g
        |> LoadAndSave.save LoadAndSave.XML
    | g ->
        g
        |> ControllerSystem.processInputs
        |> Scheduler.executeSchedule
        |> Logger.write
        |> Game.incrementRound
        |> gameLoop

Game.empty
|> Game.setMapSize { X = 100s; Y = 25s; Z = 1s }
|> Game.setRenderMode World
|> Events.register
    [|
        { EventType = Event_Action_Eat; Action = EatingSystem.onEat }
        { EventType = Event_Action_Movement; Action = MovementSystem.onMovement }
        { EventType = Event_CreateEntity; Action = Entities.onCreateEntity }
        { EventType = Event_CreateEntity; Action = Scheduler.onCreateEntity }
        { EventType = Event_Metabolize; Action = EatingSystem.onMetabolize }
    |]
|> Game.createTerrain 
|> Game.makeGrass 5u
|> Game.makeRabbits false 3u
|> gameLoop
//|> Renderer.renderWorld
//|> LoadAndSave.save LoadAndSave.XML


//LoadAndSave.load LoadAndSave.XML "Save_201912051704_r18"
//|> gameLoop


