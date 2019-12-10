module Game
open CommonTypes
open GameTypes


let incrementRound (game:Game) : Game =
    {
        game with 
            Round = game.Round + 1u 
    }


let saveAfterRound (game:Game) : Game = 
    match game.SaveEveryRound with
    | false -> game
    | true -> LoadAndSave.save game


let setMapSize (l:Location) (game:Game) : Game = { game with MapSize = l }


let setRenderMode (mode:RenderTypes) (game:Game) : Game = { game with RenderType = mode }


let setSaveEveryRound (toggle:bool) (game:Game) : Game = { game with SaveEveryRound = toggle }


let setSaveFormat (format:SaveGameFormats) (game:Game) : Game = { game with SaveFormat = format }


