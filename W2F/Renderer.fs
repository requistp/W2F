module Renderer
open CommonTypes
open ComponentEnums
open Entities
open EntityAndGameTypes
open FormComponent
open LocationFunctions
open System


let renderWorld (game:Game) : unit = 
    Console.CursorVisible <- false
    Console.Title <- "World Map"
    Console.SetBufferSize(250,500)

    let allForms = getLocationMap game.Entities

    let selectForm (fds:FormComponent[]) = 
        fds
        |> Array.sortByDescending (fun f -> f.ID)
        |> Array.head

    mapLocations game.MapSize
    |> Array.iter (fun (l:Location) -> 
        let fd = selectForm (allForms.Item l)
        
        System.Console.SetCursorPosition(int l.X, int l.Y)
        
        System.Console.Write(fd.Symbol)
        )

let renderRound (game:Game) : unit = 
    System.Console.SetCursorPosition(0, int game.MapSize.Y + 1)
    printfn "Round: %i" game.Round.ToUint32




