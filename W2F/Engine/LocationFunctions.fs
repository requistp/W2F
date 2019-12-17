module LocationFunctions
open EngineTypes
open System


let addLocations (l1:Location) (l2:Location) = l1 + l2


let subtractLocations (l1:Location) (l2:Location) = l1 - l2


let addOffset (l:Location) (rangeX:int) (rangeY:int) (rangeZ:int) (allow000:bool) (doubleRandom:bool) =
    l + (Location.Offset rangeX rangeY rangeZ allow000 doubleRandom)


let isOnMap2D (size:Location) (l:Location) = 
    l.X >= 0s && l.X <= size.X-1s && l.Y >=0s && l.Y <= size.Y-1s


let isDirectionOnMap2D (size:Location) (l:Location) (dir:MovementDirection) = 
    isOnMap2D size (l + dir.Location)


let distance2D (l1:Location) (l2:Location) =
    Math.Pow (Math.Pow (float (l1.X - l2.X), 2.0) + Math.Pow (float (l1.Y - l2.Y), 2.0), 0.5)
    

let withinRange2D (l1:Location) (range:int16) (l2:Location) =
    int16 (Math.Round(distance2D l1 l2)) <= range


let rangeTemplate2D (range:int16) =
    [| -range .. range |] 
    |> Array.collect (fun y -> [| -range .. range |] |> Array.map (fun x -> { X=x; Y=y; Z=0s } ))
    |> Array.filter (withinRange2D Location.empty range)


let locationsWithinRange2D (size:Location) (location:Location) (rangeTemplate:Location[]) = 
    rangeTemplate 
    |> Array.map (addLocations location)
    |> Array.filter (isOnMap2D size)


let mapLocations (size:Location) =
    [|0s..size.X-1s|] |> Array.Parallel.collect (fun x -> [|0s..size.Y-1s|] |> Array.Parallel.map (fun y -> { X=x; Y=y; Z=0s } ))