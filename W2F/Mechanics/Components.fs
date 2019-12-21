module rec Components
open CalendarTimings
open EngineTypes
open ComponentEnums
open System


type ComponentTypes = 
    | Controller 
    | Food 
    | Form
    | Eating 
    | Mating 
    | Movement
    | PlantGrowth
    | Terrain 
    | Vision 
    member me.TypeID : ComponentTypeID =
        match me with 
        | Form -> 0uy // Don't change
        | Controller -> 1uy
        | Food -> 2uy
        | Eating ->3uy
        | Mating -> 4uy
        | Movement -> 5uy
        | PlantGrowth -> 6uy
        | Terrain -> 7uy
        | Vision -> 8uy


type ControllerComponent(id:ComponentID, eid:EntityID, controllerType:ControllerTypes, currentAction:ActionTypes, currentActions:ActionTypes[], potentialActions:ActionTypes[]) = 
    inherit AbstractComponent(id, eid, ComponentTypes.Controller.TypeID)
    member _.ControllerType with get() = controllerType
    member _.CurrentAction with get() = currentAction
    member _.CurrentActions with get() = currentActions
    member _.PotentialActions with get() = potentialActions
    override me.Copy cid eid =
        ControllerComponent(cid, eid, me.ControllerType, Idle, [|Idle|], me.PotentialActions).Abstract
    

type FoodComponent(id:ComponentID, eid:EntityID, foodType:FoodTypes, quantity:int, quantityMax:int) = 
    inherit AbstractComponent(id, eid, ComponentTypes.Food.TypeID)
    member _.FoodType with get() = foodType
    member _.Quantity with get() = quantity
    member _.QuantityMax with get() = quantityMax
    override me.Copy cid eid =
        FoodComponent(cid, eid, me.FoodType, me.Quantity, me.QuantityMax).Abstract


type FormComponent(id:ComponentID, eid:EntityID, born:RoundNumber, canSeePast:bool, isPassable:bool, location:Location, name:string, symbol:char) =
    inherit AbstractComponent_WithLocation(id, eid, ComponentTypes.Form.TypeID, location, isPassable)
    member _.Born with get() = born
    member _.CanSeePast with get() = canSeePast
    member _.Name with get() = name
    member _.Symbol with get() = symbol
    override me.Copy cid eid =
        FormComponent(cid, eid, me.Born, me.CanSeePast, me.IsPassable, me.Location, me.Name, me.Symbol).Abstract


type EatingComponent(id:ComponentID, eid:EntityID, calories:int, caloriesPerDay:int, foods:FoodTypes[], quantity:int, quantityMax:int, quantityPerAction:int) = 
    inherit AbstractComponent(id, eid, ComponentTypes.Eating.TypeID)
    member _.Calories with get() = calories
    member _.CaloriesPerDay with get() = caloriesPerDay
    member _.Foods with get() = foods
    member _.Quantity with get() = quantity
    member _.QuantityMax with get() = quantityMax
    member _.QuantityPerAction with get() = quantityPerAction
    member me.CaloriesPerMetabolize = Math.Clamp(convertAmountByFrequency me.CaloriesPerDay Day MetabolismFrequency,1,me.CaloriesPerDay)
    member me.QuantityPerMetabolize = Math.Clamp(convertAmountByFrequency me.QuantityMax Day MetabolismFrequency,1,me.QuantityMax)
    member me.QuantityRemaining = me.QuantityMax - me.Quantity
    override me.Copy cid eid =
        EatingComponent(cid, eid, me.Calories, me.CaloriesPerDay, me.Foods, me.Quantity, me.QuantityMax, me.QuantityPerAction).Abstract


type MatingComponent(id:ComponentID, eid:EntityID, chanceOfReproduction:float, lastMatingAttempt:RoundNumber, matingStatus:MatingStatus, species:Species) = 
    inherit AbstractComponent(id, eid, ComponentTypes.Mating.TypeID)
    member _.ChanceOfReproduction with get() = chanceOfReproduction
    member _.LastMatingAttempt with get() = lastMatingAttempt
    member _.MatingStatus with get() = matingStatus
    member _.Species with get() = species
    override me.Copy cid eid =
        MatingComponent(cid, eid, me.ChanceOfReproduction, me.LastMatingAttempt, me.MatingStatus, me.Species).Abstract


type MovementComponent(id:ComponentID, eid:EntityID, movesPerTurn:int) = 
    inherit AbstractComponent(id, eid, ComponentTypes.Movement.TypeID)
    member _.MovesPerTurn with get() = movesPerTurn
    override me.Copy cid eid = 
        MovementComponent(cid, eid, me.MovesPerTurn).Abstract


type PlantGrowthComponent(id:ComponentID, eid:EntityID, growsInTerrain:TerrainTypes[], regrowRate:float, reproductionRate:float, reproductionRange:int, reproductionRequiredFoodQuantity:float) = 
    inherit AbstractComponent(id, eid, ComponentTypes.PlantGrowth.TypeID)
    member _.GrowsInTerrain with get() = growsInTerrain
    member _.RegrowRate with get() = regrowRate
    member _.ReproductionRate with get() = reproductionRate
    member _.ReproductionRange with get() = reproductionRange
    member _.ReproductionRequiredFoodQuantity with get() = reproductionRequiredFoodQuantity
    override me.Copy cid eid = 
        PlantGrowthComponent(cid, eid, me.GrowsInTerrain, me.RegrowRate, me.ReproductionRate, me.ReproductionRange, me.ReproductionRequiredFoodQuantity).Abstract


type TerrainComponent(id:ComponentID, eid:EntityID, terrain:TerrainTypes) = 
    inherit AbstractComponent(id, eid, ComponentTypes.Terrain.TypeID)
    member _.Terrain with get() = terrain
    override me.Copy cid eid = 
        TerrainComponent(cid, eid, me.Terrain).Abstract


type VisionComponent(id:ComponentID, eid:EntityID, locationsWithinRange:Location[], range:int16, rangeTemplate:Location[], visionCalculationType:VisionCalculationTypes, viewedHistory:Map<Location,FormComponent[]>, visibleLocations:Map<Location,FormComponent[]>) = 
    inherit AbstractComponent(id, eid, ComponentTypes.Vision.TypeID)
    member _.LocationsWithinRange with get() = locationsWithinRange
    member _.Range with get() = range
    member _.RangeTemplate with get() = rangeTemplate
    member _.VisionCalculationType with get() = visionCalculationType
    member _.ViewedHistory with get() = viewedHistory
    member _.VisibleLocations with get() = visibleLocations
    override me.Copy cid eid = 
        VisionComponent(cid, eid, me.LocationsWithinRange, me.Range, me.RangeTemplate, me.VisionCalculationType, me.ViewedHistory, me.VisibleLocations).Abstract


let ToController (ac:AbstractComponent) = ac :?> ControllerComponent
let ToControllers (a:AbstractComponent[]) = Array.map ToController a
let ToEating (ac:AbstractComponent) = ac :?> EatingComponent
let ToEating2 (a:AbstractComponent[]) = Array.map ToEating a
let ToFood (ac:AbstractComponent) = ac :?> FoodComponent
let ToFoods (a:AbstractComponent[]) = Array.map ToFood a
let ToForm (ac:AbstractComponent) = ac :?> FormComponent
let ToForms (a:AbstractComponent[]) = Array.map ToForm a
let ToMating (ac:AbstractComponent) = ac :?> MatingComponent
let ToMatings (a:AbstractComponent[]) = Array.map ToMating a
let ToMovement (ac:AbstractComponent) = ac :?> MovementComponent
let ToMovements (a:AbstractComponent[]) = Array.map ToMovement a
let ToPlantGrowth (ac:AbstractComponent) = ac :?> PlantGrowthComponent
let ToPlantGrowths (a:AbstractComponent[]) = Array.map ToPlantGrowth a
let ToTerrain (ac:AbstractComponent) = ac :?> TerrainComponent
let ToTerrains (a:AbstractComponent[]) = Array.map ToTerrain a
let ToVision (ac:AbstractComponent) = ac :?> VisionComponent
let ToVisions (a:AbstractComponent[]) = Array.map ToVision a


