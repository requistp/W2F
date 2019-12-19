module rec EngineTypes
open CommonTypes
open EventManager
open Game


type Engine(events:EventManager, game:Game) =
        //ExitGame : bool
        //Log : string[] // Maybe seperate out
        //MapSize : Location
        //Round : RoundNumber
        //Settings : Settings
   
    static member empty = 
        Engine(new EventManager(), Game.empty)


type Settings = 
    {   
        LoggingOn : bool
        RenderType : RenderTypes
        SaveEveryRound : bool
        SaveFormat : SaveGameFormats
    }
    static member empty = 
        {   
            LoggingOn = false
            RenderType = RenderTypes.World
            SaveEveryRound = false
            SaveFormat = SaveGameFormats.XML
        }


