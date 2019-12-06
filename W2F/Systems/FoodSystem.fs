module FoodSystem
open CommonTypes
open Component
open ComponentEnums
open EntityAndGameTypes


let getFoodsAtLocation (ent:Entities) (loc:Location) =
    loc
    |> Entities.getAtLocationWithComponent ent FoodComponent None
    |> Array.map ToFood


