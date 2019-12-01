module Component
//open CommonTypes
//open CommonGenericFunctions
open ComponentEnums
open ControllerComponent
open EatingComponent
open FoodComponent
open FormComponent
open MatingComponent
open MovementComponent
open PlantGrowthComponent
open TerrainComponent
open VisionComponent

type Component = 
    | Controller of ControllerComponent
    | Eating of EatingComponent
    | Food of FoodComponent
    | Form of FormComponent
    | Mating of MatingComponent
    | Movement of MovementComponent
    | PlantGrowth of PlantGrowthComponent
    | Terrain of TerrainComponent
    | Vision of VisionComponent

let ToController (Controller c) = c
let ToEating (Eating c) = c
let ToFood (Food c) = c
let ToForm (Form c) = c
let ToMating (Mating c) = c
let ToMovement (Movement c) = c
let ToPlantGrowth (PlantGrowth c) = c
let ToTerrain (Terrain c) = c
let ToVision (Vision c) = c

//let Copy (newEID:EntityID) (idGen:unit->ComponentID) (round:RoundNumber) (c:Component) =
//    match c with
//    | Controller d -> Controller { d with ID = idGen(); EntityID = newEID }
//    | Eating d -> Eating { d with ID = idGen(); EntityID = newEID }
//    | Food d -> Food { d with ID = idGen(); EntityID = newEID }
//    | Form d -> Form { d with ID = idGen(); EntityID = newEID; Born = round }
//    | Mating d -> Mating { d with ID = idGen(); EntityID = newEID }
//    | Movement d -> Movement { d with ID = idGen(); EntityID = newEID }
//    | PlantGrowth d -> PlantGrowth { d with ID = idGen(); EntityID = newEID }
//    | Terrain d -> Terrain { d with ID = idGen(); EntityID = newEID }
//    | Vision d -> Vision { d with ID = idGen(); EntityID = newEID }

//let GetComponentID (c:Component) =
//    match c with 
//    | Controller d -> d.ID
//    | Eating d -> d.ID
//    | Food d -> d.ID
//    | Form d -> d.ID
//    | Mating d -> d.ID
//    | Movement d -> d.ID
//    | PlantGrowth d -> d.ID
//    | Terrain d -> d.ID
//    | Vision d -> d.ID

let getComponentType (c:Component) =
    match c with
    | Controller _ -> ControllerComponent
    | Eating _ -> EatingComponent
    | Food _ -> FoodComponent
    | Form _ -> FormComponent
    | Mating _ -> MatingComponent
    | Movement _ -> MovementComponent
    | PlantGrowth _ -> PlantGrowthComponent
    | Terrain _ -> TerrainComponent
    | Vision _ -> VisionComponent

let getComponentEntityID (c:Component) =
    match c with
    | Controller d -> d.EntityID
    | Eating d -> d.EntityID
    | Food d -> d.EntityID
    | Form d -> d.EntityID
    | Mating d -> d.EntityID
    | Movement d -> d.EntityID
    | PlantGrowth d -> d.EntityID
    | Terrain d -> d.EntityID
    | Vision d -> d.EntityID



