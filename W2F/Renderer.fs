module Renderer
open CommonTypes
open ComponentEnums
open Entities
open FormComponent
open Game
open LocationFunctions
open System


let renderWorld (game:Game) : unit= 
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

    System.Console.SetCursorPosition(0, int game.MapSize.Y + 1)
    printfn "Round: %i" game.Round.ToUint32





    //let viewSizeX = Math.Clamp(100s,5s,game.MapSize.X)
    //let viewSizeY = Math.Clamp(100s,5s,game.MapSize.Y)
    

    //Console.SetBufferSize(100,500)

    //let rangeY = 
    //    [|0s..(snd _windowLocation + viewSizeY - 1s)|]

    //let rangeX = 
    //    [|(fst _windowLocation)..(fst _windowLocation + viewSizeX - 1s)|]
    
    //for y in rangeY do
    //    for x in rangeX do
    //        let selectForm (fds:FormComponent[]) = 
    //            fds
    //            |> Array.sortByDescending (fun f -> f.EntityID)
    //            |> Array.head
    //            //match fds.Length with
    //            //| 1 -> fds.[0]
    //            //| _ ->
    //            //    match fds |> Array.tryFind (fun c -> (EntityExt.TryGetComponent enm ControllerComponent c.EntityID).IsSome) with
    //            //    | Some f -> f
    //            //    | None ->
    //            //        (fds |> Array.sortBy (fun c -> (EntityExt.TryGetComponent enm TerrainComponent c.EntityID).IsSome)).[0]
            
    //        let formsAtLocation = allForms.Item({ X = x; Y = y; Z = 0s })

    //        match formsAtLocation.Length with
    //        | 0 -> ()
    //        | _ ->
    //            let fd = selectForm formsAtLocation
    //            System.Console.SetCursorPosition(int (x - fst _windowLocation), int (y - snd _windowLocation))
    //            System.Console.Write(fd.Symbol)