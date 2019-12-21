module rec Renderer
open EngineTypes
open Components
open LocationFunctions
open System


let renderWorld (game:Game) : Game = 
    match game.Settings.RenderType with
    | Skip | Entity -> game
    | World ->
        Console.CursorVisible <- false
        Console.Title <- "World Map"
        Console.SetBufferSize(250,500)
        System.Console.SetWindowPosition(0,0)
    
        let allForms = 
            Engine.Entities.get_LocationMap game.Entities
        let selectForm (fds:FormComponent[]) = 
            fds
            |> Array.sortByDescending (fun f -> f.ID)
            |> Array.head
        game.MapSize
        |> mapLocations 
        |> Array.iter (fun (l:Location) -> 
            let fd = selectForm (allForms.Item l |> ToForms)        
            System.Console.SetCursorPosition(int l.X, int l.Y)        
            //if fd.Symbol <> '.' then 
            System.Console.Write(fd.Symbol)
            )
        game


let renderRound (game:Game) : Game = 
    match game.Settings.RenderType with
    | Skip -> System.Console.SetCursorPosition(0, 0)
    | _ -> System.Console.SetCursorPosition(0, int game.MapSize.Y + 1)
    printfn "Round: %i" game.Round.ToUint32
    game


let renderEntity (game:Game) (eid:EntityID) : unit =
    Console.CursorVisible <- false
    Console.Title <- "Entity Viewer"
    Console.Clear()

    let centerX = 30s
    let centerY = 10s

    let v = Engine.Entities.get_Component game.Entities ComponentTypes.Vision.TypeID eid |> ToVision
    let f = Engine.Entities.get_Component game.Entities ComponentTypes.Form.TypeID eid |> ToForm

    let addX = centerX - f.Location.X
    let addY = centerY - f.Location.Y
    
    v.ViewedHistory
    |> Map.iter (fun location fs -> 
        let drawX = location.X + int16 addX
        let drawY = location.Y + int16 addY
        match drawX >= 0s && drawY >= 0s with
        | false -> ()
        | true -> 
            let drawCall = 
                match (v.VisibleLocations.ContainsKey location) with
                | false -> Renderer.Console.DrawDarkGray
                | true -> Renderer.Console.DrawWhite
            let formChar = 
                (
                match (v.VisibleLocations.ContainsKey location) with
                | false -> 
                    v.ViewedHistory.Item location
                    |> Array.sortByDescending (fun f -> f.ID)
                    |> Array.head
                | true -> 
                    v.ViewedHistory.Item location
                    |> Array.sortByDescending (fun f -> f.ID)
                    |> Array.head
                ).Symbol
            let countAsChar = // Useful for debugging
                match (v.VisibleLocations.ContainsKey location) with
                | false -> 
                    (v.ViewedHistory.Item location).Length.ToString().ToCharArray().[0]
                | true -> 
                    (v.ViewedHistory.Item location
                    |> Array.sortByDescending (fun f -> f.ID)
                    |> Array.head).Symbol
            System.Console.SetCursorPosition(int drawX,int drawY)
            drawCall formChar
        )
    
module Console =
//http://www.fssnip.net/7Vy/title/Supersimple-thread-safe-colored-console-output

// go here when I expand:
// https://blog.vbfox.net/2016/10/17/more-fsharp-colors-in-terminal.html
    open System

    let private log =
        let lockObj = obj()
        fun color (s:char) ->
            lock lockObj (fun _ ->
                //Console.BackgroundColor <- ConsoleColor.DarkYellow
                Console.ForegroundColor <- color
                Console.Write(s)
                //printfn "%s" s
                Console.ResetColor())

    let DrawMagenta = log ConsoleColor.Magenta
    let DrawGreen = log ConsoleColor.Green
    let DrawCyan = log ConsoleColor.Cyan
    let DrawYellow = log ConsoleColor.Yellow
    let DrawRed = log ConsoleColor.Red
    let DrawBlack = log ConsoleColor.Black
    let DrawBlue = log ConsoleColor.Blue
    let DrawDarkBlue = log ConsoleColor.DarkBlue
    let DrawDarkCyan = log ConsoleColor.DarkCyan
    let DrawDarkGray = log ConsoleColor.DarkGray
    let DrawDarkGreen = log ConsoleColor.DarkGreen
    let DrawDarkMagenta = log ConsoleColor.DarkMagenta
    let DrawDarkRed = log ConsoleColor.DarkRed
    let DrawDarkYellow = log ConsoleColor.DarkYellow
    let DrawGray = log ConsoleColor.Gray
    let DrawWhite = log ConsoleColor.White