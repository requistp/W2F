module Renderer
open CommonTypes
open Entities
open GameTypes
open FormComponent
open LocationFunctions
open System

let renderWorld (game:Game) : Game = 
    match game.RenderType with
    | Skip | Entity -> game
    | World ->
        Console.CursorVisible <- false
        Console.Title <- "World Map"
        Console.SetBufferSize(250,500)
        System.Console.SetWindowPosition(0,0)
    
        let allForms = getLocationMap game.Entities
        let selectForm (fds:FormComponent[]) = 
            fds
            |> Array.sortByDescending (fun f -> f.ID)
            |> Array.head
        game.MapSize
        |> mapLocations 
        |> Array.iter (fun (l:Location) -> 
            let fd = selectForm (allForms.Item l)        
            System.Console.SetCursorPosition(int l.X, int l.Y)        
            //if fd.Symbol <> '.' then 
            System.Console.Write(fd.Symbol)
            )
        game


let renderRound (game:Game) : Game = 
    System.Console.SetCursorPosition(0, int game.MapSize.Y + 1)
    printfn "Round: %i" game.Round.ToUint32
    game


(*
member me.UpdateEntity (enm:EntityManager) (entityID:EntityID) = 
    Console.CursorVisible <- false
    Console.Title <- "Entity Viewer"
    Console.Clear()

    let centerX = 30
    let centerY = 10

    let (Vision v) = enm.GetComponent VisionComponent entityID
    let (Form f) = enm.GetComponent FormComponent entityID

    let addX = centerX - f.Location.X
    let addY = centerY - f.Location.Y
    
    v.ViewedHistory
    |> Map.iter (fun location round -> 
        let drawX = location.X + addX
        let drawY = location.Y + addY
        match drawX >= 0 && drawY >= 0 with
        | false -> ()
        | true -> 
            let drawCall = 
                match (v.VisibleLocations.ContainsKey location) with
                | false -> ColoredConsole.Console.DrawDarkGray
                | true -> ColoredConsole.Console.DrawWhite
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
            System.Console.SetCursorPosition(drawX,drawY)
            drawCall formChar
        )
*)

