module FoodSystem
open CommonTypes
open Component
open GameTypes


let getFoodsAtLocation (ent:Entities) (loc:Location) =
    loc |> Entities.getAtLocationWithComponent ent FoodComponent ToFood None


(*
member private me.onRegrowth round (PlantRegrowth pg:GameEventData) =
    let tryRegrowFood (f:FoodComponent) = 
        let missing = f.QuantityMax - f.Quantity
        match (missing, pg.RegrowRate) with
        | (0,_) -> Ok (Some "Already maxed")
        | (_,0.0) -> Ok (Some "Zero regrow rate")
        | (_,_) -> 
            let quantity = Math.Clamp((int (Math.Round(pg.RegrowRate * (float f.QuantityMax),0))), 1, missing)
            enm.UpdateComponent (Food { f with Quantity = f.Quantity + quantity })
            Ok (Some (sprintf "EntityID:%i. Regrown quantity:%i" pg.EntityID.ToUint32 quantity))
    match (EntityExt.TryGetComponent enm FoodComponent pg.EntityID) with
    | None -> Ok None
    | Some (Food c) -> tryRegrowFood c
    | Some _ -> Error "Should not happen"
*)