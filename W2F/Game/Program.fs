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
        |> Game.Persistance.save
        |> ignore // Exits
    | g ->
        g
        |> Scheduler.executeSchedule
        |> Game.Log.write
        |> Game.Settings.saveAfterRound
        |> Game.Round.increment
        |> gameLoop


Game.empty
|> Game.Settings.setMapSize { X = 100s; Y = 25s; Z = 1s }
|> Game.Settings.setRenderMode RenderTypes.Skip
|> Game.Settings.setSaveEveryRound false
|> Game.Settings.setSaveFormat SaveGameFormats.XML
|> Events.register
    [|
        { EventType = Event_Action_Eat;       Action = EatingSystem.onEat }
        { EventType = Event_Action_ExitGame;  Action = ControllerSystem.onExitGame }
        { EventType = Event_Action_Movement;  Action = MovementSystem.onMovement }
        { EventType = Event_ComponentAdded;   Action = EatingSystem.onComponentAdded }
        { EventType = Event_ComponentAdded;   Action = PlantGrowthSystem.onComponentAdded }
        { EventType = Event_Metabolize;       Action = EatingSystem.onMetabolize }
        { EventType = Event_PlantRegrowth;    Action = FoodSystem.onRegrowth }
        //{ EventType = Event_CreateEntity;     Action = Game.onCreateEntity }
        //{ EventType = Event_RemoveEntity;     Action = Game.onRemoveEntity }
        //{ EventType = Event_UpdateComponent;  Action = Game.onUpdateComponent }
        //{ EventType = Event_UpdateComponents; Action = Game.onUpdateComponents }
    |]
|> BuildWorld.createTerrain 
|> BuildWorld.makeGrass 5u
|> BuildWorld.makeRabbits false 3u
|> gameLoop
//|> Renderer.renderWorld
//|> Game.Persistance.save LoadAndSave.XML


//Game.Persistance.load LoadAndSave.XML "Save_201912051704_r18"
//|> gameLoop


