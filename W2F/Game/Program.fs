open CommonTypes
open GameTypes


let rec gameLoop (game:Game) = 
    game
    |> Renderer.renderWorld 
    |> Renderer.renderRound
    |> ControllerSystem.getInputs
    |> ControllerSystem.processInputs
    |> function
    | g when g.ExitGame -> 
        g
        |> LoadAndSave.save
        |> ignore // Exits
    | g ->
        g
        |> Scheduler.executeSchedule
        |> Logger.write
        |> Game.saveAfterRound
        |> Game.incrementRound
        |> gameLoop


Game.empty
|> Game.setMapSize { X = 100s; Y = 25s; Z = 1s }
|> Game.setRenderMode RenderTypes.World
|> Game.setSaveEveryRound false
|> Game.setSaveFormat SaveGameFormats.XML
|> Events.register
    [|
        { EventType = Event_Action_Eat;      Action = EatingSystem.onEat }
        { EventType = Event_Action_ExitGame; Action = ControllerSystem.onExitGame }
        { EventType = Event_Action_Movement; Action = MovementSystem.onMovement }
        { EventType = Event_CreateEntity;    Action = Entities.onCreateEntity }
        { EventType = Event_CreateEntity;    Action = Scheduler.onCreateEntity }
        { EventType = Event_Metabolize;      Action = EatingSystem.onMetabolize }
        { EventType = Event_RemoveEntity;    Action = Entities.onRemoveEntity }
    |]
|> BuildWorld.createTerrain 
|> BuildWorld.makeGrass 5u
|> BuildWorld.makeRabbits true 3u
|> gameLoop
//|> Renderer.renderWorld
//|> LoadAndSave.save LoadAndSave.XML


//LoadAndSave.load LoadAndSave.XML "Save_201912051704_r18"
//|> gameLoop


