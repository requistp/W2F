namespace rec Mechanics
open CalendarTimings
open CommonFunctions
open ComponentEnums
open Components
open Engine
open Engine.Entities
open EngineTypes
open EventTypes
open LocationFunctions
open System
open vision_Shadowcast


// -----------------------------------------------------------------------------------------------------------------
module Configuration = 

    let set (preSteps:GameLoopStep[]) (postSteps:GameLoopStep[]) (game:Game) : Game = 
        game
        |> Engine.Events.registerListeners
            [|
                EventListener("Common->CheckLocationChanged", Common.onComponentUpdated_CheckIfLocationChanged, EventTypes.Engine_ComponentUpdated.TypeID)

                EventListener("Eating->Action",               Eating.onEat,                 EventTypes.Action_Eat.TypeID)
                EventListener("Controller->ExitGame",         Controller.onExitGame,        EventTypes.Action_ExitGame.TypeID)
                EventListener("Movement->Action",             Movement.onMovement,          EventTypes.Action_Movement.TypeID)
                EventListener("Eating->ComponentAdded",       Eating.onComponentAdded,      Engine_ComponentAdded.TypeID)
                EventListener("Controller->ComponentAdded",   Controller.onComponentAdded,  Engine_ComponentAdded.TypeID)
                EventListener("PlantGrowth->ComponentAdded",  PlantGrowth.onComponentAdded, Engine_ComponentAdded.TypeID)
                EventListener("Eating->Metabolize",           Eating.onMetabolize,          EventTypes.Metabolize.TypeID)
                EventListener("Food->Regrowth",               Food.onRegrowth,              EventTypes.PlantRegrowth.TypeID)
                EventListener("PlantGrowth->Reproduce",       PlantGrowth.onReproduce,      EventTypes.PlantReproduce.TypeID)
                EventListener("Vision->LocationChanged",      Vision.onLocationChanged,     EventTypes.LocationChanged.TypeID)
            |]
        |> Engine.GameLoop.setSteps preSteps
        //|> Engine.GameLoop.appendSteps
        //    [|
        //        Controller.getInputs
        //        Controller.processInputs

        //        Vision.updateViewable
        //    |]
        //|> Engine.GameLoop.appendSteps postSteps


// -----------------------------------------------------------------------------------------------------------------
module Common =

    let onComponentUpdated_CheckIfLocationChanged (game:Game) (e:AbstractEventData) =
        let ce = e :?> EngineEvent_ComponentUpdated
        let oldc = ce.OldComponent
        match oldc.ComponentType = ComponentTypes.Form.TypeID with
        | false -> game
        | true ->
            let oldf = ToForm oldc
            let newf = ToForm ce.NewComponent
            match oldf.Location = newf.Location with
            | true -> game
            | false -> 
                Engine.Events.execute (LocationChanged(oldf,newf)) game

// -----------------------------------------------------------------------------------------------------------------
module Controller = 
    
    let private actionIsAllowed (cc:ControllerComponent) (action:ActionTypes) = 
        cc.CurrentActions 
        |> Array.contains action
    
    let private requiredComponents (at:ActionTypes) =
        match at with
        | Eat -> [| ComponentTypes.Eating |]
        | ExitGame -> [||]
        | Idle -> [||]
        | Mate -> [| ComponentTypes.Mating |]
        | Move_East -> [| ComponentTypes.Form; ComponentTypes.Movement |]
        | Move_North -> [| ComponentTypes.Form; ComponentTypes.Movement |]
        | Move_South -> [| ComponentTypes.Form; ComponentTypes.Movement |]
        | Move_West -> [| ComponentTypes.Form; ComponentTypes.Movement |]
    
    let getPotentialActions (cts:AbstractComponent[]) = 
        let ects = cts |> Array.map (fun (c:AbstractComponent) -> c.ComponentType)
        ActionTypes.AsArray 
        |> Array.choose (fun a -> if requiredComponents a |> Array.forall (fun ct -> ects |> Array.contains ct.TypeID) then Some a else None)
    
    let getInputs (game:Game) : Game = 
        let awaitKeyboardInput (cc:ControllerComponent) =
            let mutable _action = None            
            // Uncomment for Entity-view... 
            if game.Settings.RenderType = RenderTypes.Entity && game.Renderer_Entity.IsSome then game.Renderer_Entity.Value game cc.EntityID

            let handleKeyPressed (k:ConsoleKeyInfo) = 
                while Console.KeyAvailable do //Might help clear double movement keys entered in one turn
                    Console.ReadKey(true).Key |> ignore
                match k.Key with 
                | ConsoleKey.Escape -> Some ExitGame
                | ConsoleKey.Spacebar -> Some Idle
                | ConsoleKey.E -> if actionIsAllowed cc Eat  then Some Eat  else None
                | ConsoleKey.M -> if actionIsAllowed cc Mate then Some Mate else None
                | ConsoleKey.RightArrow -> if actionIsAllowed cc Move_East  then Some Move_East  else None
                | ConsoleKey.UpArrow    -> if actionIsAllowed cc Move_North then Some Move_North else None
                | ConsoleKey.DownArrow  -> if actionIsAllowed cc Move_South then Some Move_South else None
                | ConsoleKey.LeftArrow  -> if actionIsAllowed cc Move_West  then Some Move_West  else None
                | _ -> None
            while _action.IsNone do
                while not Console.KeyAvailable do
                    System.Threading.Thread.Sleep 2
                _action <- handleKeyPressed (Console.ReadKey(true))        
            _action.Value
    
        let getCurrentActions (cc:ControllerComponent) = 
            let movesAllowed = Mechanics.Movement.actionsAllowed game cc.EntityID
            let newCurrent =
                cc.PotentialActions 
                |> Array.choose (function
                    | Eat -> if Mechanics.Eating.actionEnabled game.Entities cc.EntityID then Some Eat else None
                    | ExitGame -> if cc.ControllerType = Keyboard then Some ExitGame else None
                    | Idle -> Some Idle
                    | Mate -> if Mechanics.Mating.actionEnabled game.Entities cc.EntityID game.Round then Some Mate else None
                    | Move_North -> if Array.contains Move_North movesAllowed then Some Move_North else None
                    | Move_East ->  if Array.contains Move_East  movesAllowed then Some Move_East  else None
                    | Move_South -> if Array.contains Move_South movesAllowed then Some Move_South else None
                    | Move_West ->  if Array.contains Move_West  movesAllowed then Some Move_West  else None
                    )
            match (arraysMatch newCurrent cc.CurrentActions) with
            | true -> cc
            | false -> ControllerComponent(cc.ID, cc.EntityID, cc.ControllerType, cc.CurrentAction, newCurrent, cc.PotentialActions)
    
        let getActionForEntity (cc:ControllerComponent) =
            let currentAction = 
                match cc.ControllerType with
                | AI_Random -> 
                    cc.CurrentActions.[random.Next(cc.CurrentActions.Length)]
                | Keyboard -> 
                    awaitKeyboardInput cc
            ControllerComponent(cc.ID, cc.EntityID, cc.ControllerType, currentAction, cc.CurrentActions, cc.PotentialActions)
    
        let handleSplitInputTypes (keyboard:ControllerComponent[], ai:ControllerComponent[]) =
            ai 
            |> Array.map getActionForEntity
            |> Array.append (keyboard |> Array.map getActionForEntity)
    
        //start
        ComponentTypes.Controller.TypeID
        |> Engine.Entities.get_Components_OfType game.Entities 
        |> ToControllers
        |> Array.map getCurrentActions
        |> Array.partition (fun c -> c.ControllerType = Keyboard)
        |> handleSplitInputTypes
        |> Array.fold (fun g c ->
            Engine.Entities.updateComponent 
                g 
                c.Abstract 
                (Some (Logging.format1 "Ok" "Controller System" "getInputs" c.EntityID (Some c.ID) (Some (c.CurrentAction,c.CurrentActions))))
            ) game
    
    let processInputs (game:Game) : Game =
        let convertToEventData (c:ControllerComponent) =
            match c.CurrentAction with
            | Eat -> Action_Eat(c.EntityID).Abstract 
            | ExitGame -> Action_ExitGame().Abstract
            //| Mate -> 
            | Move_North | Move_East | Move_South | Move_West -> Action_Movement(c).Abstract
        ComponentTypes.Controller.TypeID
        |> Engine.Entities.get_Components_OfType game.Entities
        |> ToControllers
        |> Array.filter (fun c -> not (Array.contains c.CurrentAction [|Idle|]))
        |> Array.sortBy (fun c -> c.CurrentAction ) 
        |> Array.fold (fun g c -> Engine.Events.execute (convertToEventData c) g) game
    
    let onExitGame (game:Game) (_:AbstractEventData) : Game = 
        Engine.GameLoop.exit game
        
    let onComponentAdded (game:Game) (e:AbstractEventData) : Game = 
        //game
        let c = (e :?> EngineEvent_ComponentAdded).Component
        match c.ComponentType = Controller.TypeID with
        | false -> game
        | true -> 
            let old = c :?> ControllerComponent
            let newPotential = getPotentialActions (Engine.Entities.get game.Entities c.EntityID)
            Engine.Entities.updateComponent 
                game
                (ControllerComponent(old.ID, old.EntityID, old.ControllerType, old.CurrentAction, old.CurrentActions, newPotential))
                (Some (Logging.format1 "Ok" "Controller System" "onComponentAdded" c.EntityID (Some c.ID) (Some newPotential)))
        
    
// -----------------------------------------------------------------------------------------------------------------
module Eating = 
    
    let private canEat (eater:EatingComponent) (food:FoodTypes) = Array.contains food eater.Foods    
    
    let private getEdibleFoods (eat:EatingComponent) (foods:FoodComponent[]) =
        foods
        |> Array.filter (fun f -> f.Quantity > 0 && canEat eat f.FoodType) // Types I can eat & Food remaining
        
    let private getEdibleFoodsAtLocation (ent:Entities) (eat:EatingComponent) =
        eat.EntityID
        |> Engine.Entities.get_Location ent 
        |> Mechanics.Food.getAtLocation ent
        |> getEdibleFoods eat
    
    let actionEnabled (ent:Entities) (eid:EntityID) =
        let eat = Engine.Entities.get_Component ent ComponentTypes.Eating.TypeID eid |> ToEating
        (eat. QuantityRemaining > 0) && ((getEdibleFoodsAtLocation ent eat).Length > 0)
    
    let onComponentAdded (game:Game) (e:AbstractEventData) = 
        match (e :?> EngineEvent_ComponentAdded).Component.ComponentType = ComponentTypes.Eating.TypeID with
        | false -> game
        | true -> 
            Scheduler.addToSchedule 
                { ScheduleType = RepeatIndefinitely; Frequency = MetabolismFrequency; Event = Metabolize(e.EntityID) }
                game
        
    let onEat (game:Game) (e:AbstractEventData) =
        let eat = Engine.Entities.get_Component game.Entities Eating.TypeID e.EntityID |> ToEating
        eat
        |> getEdibleFoodsAtLocation game.Entities
        |> Array.sortByDescending (fun f -> f.FoodType.Calories) // Highest caloric food first
        |> function
        | [||] -> Engine.Log.append (Logging.format1 "Err" "Eating System" "eat" e.EntityID (Some eat.ID) (Some "No food at location")) game
        | fs -> 
            let f = fs.[0]
            let eatenQuantity = Math.Clamp(eat.QuantityPerAction, 0, Math.Min(f.Quantity,eat.QuantityRemaining)) // Clamp by how much food is left and how much stomach space is left
            let calories = eatenQuantity * f.FoodType.Calories
            let newFoodQuantity = Math.Clamp(f.Quantity - eatenQuantity, 0, f.QuantityMax)
            let allEaten = newFoodQuantity = 0
            let killFood = allEaten && f.FoodType.KillOnAllEaten
            let note = sprintf "EateeID: %i. EatenQuantity: +%i=%i. Calories: +%i=%i. FoodQuantity:%i. All eaten:%b, kill:%b" (f.EntityID.ToUint32) eatenQuantity (eat.Quantity+eatenQuantity) calories (eat.Calories+calories) newFoodQuantity allEaten killFood
            Engine.Entities.updateComponents 
                game
                [|
                    EatingComponent(eat.ID, eat.EntityID, eat.Calories + calories, eat.CaloriesPerDay, eat.Foods, eat.Quantity + eatenQuantity, eat.QuantityMax, eat.QuantityPerAction)
                    FoodComponent(f.ID, f.EntityID, f.FoodType, newFoodQuantity, f.QuantityMax)
                |]
                (Some (Logging.format1 "Ok" "Eating System" "eat" e.EntityID (Some eat.ID) (Some note)))
            |> ifBind killFood (Engine.Entities.remove f.EntityID None)
    
    let onMetabolize (game:Game) (e:AbstractEventData) = 
        let eat = Engine.Entities.get_Component game.Entities Eating.TypeID e.EntityID |> ToEating
        let newC = eat.Calories - eat.CaloriesPerMetabolize
        let newQ = eat.Quantity - eat.QuantityPerMetabolize
        let starving = newC < 0
        let note = sprintf "Quantity:-%i=%i. Calories:-%i=%i. Starving:%b" eat.QuantityPerMetabolize newQ eat.CaloriesPerMetabolize newC starving
        Engine.Entities.updateComponent 
            game 
            (EatingComponent(eat.ID, eat.EntityID, newC, eat.CaloriesPerDay, eat.Foods, newQ, eat.QuantityMax, eat.QuantityPerAction)) 
            (Some (Logging.format1 "Ok" "Eating System" "onMetabolize" e.EntityID None (Some note)))
        //if starving then evm.RaiseEvent (Starving eat) 
        //|> ifBind killFood (Events.execute (RemoveEntity f.EntityID))
    
    
// -----------------------------------------------------------------------------------------------------------------
module Food = 
    
    let getAtLocation (ent:Entities) (loc:Location) =
        loc 
        |> Engine.Entities.get_AtLocationWithComponent ent ComponentTypes.Food.TypeID None
        |> ToFoods    
    
    let onRegrowth (game:Game) (e:AbstractEventData) =
        match (Engine.Entities.tryGet_Component game.Entities ComponentTypes.Food.TypeID e.EntityID) with
        | None -> Engine.Log.append (Logging.format1 "Err" "FoodSystem" "onRegrowth" e.EntityID None (Some "No food component")) game
        | Some ac -> 
            let f = ac :?> FoodComponent
            let pg = Engine.Entities.get_Component game.Entities ComponentTypes.PlantGrowth.TypeID e.EntityID :?> PlantGrowthComponent
            let missing = f.QuantityMax - f.Quantity
            match (missing, pg.RegrowRate) with
            | (0,_  ) -> Engine.Log.append (Logging.format1 "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some "Already fully grown")) game
            | (_,0.0) -> Engine.Log.append (Logging.format1 "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some "Zero regrow rate")) game
            | (_,_  ) -> 
                let quantity = Math.Clamp((int (Math.Round(pg.RegrowRate * (float f.QuantityMax),0))), 1, missing)
                Engine.Entities.updateComponent 
                    game 
                    (FoodComponent(f.ID, f.EntityID, f.FoodType, f.Quantity + quantity, f.QuantityMax))
                    (Some (Logging.format1 "Ok" "FoodSystem" "onRegrowth" e.EntityID (Some pg.ID) (Some (sprintf "Regrown quantity:%i" quantity))))
    

// -----------------------------------------------------------------------------------------------------------------
module Mating = 
    
    let private eligibleFemales (ent:Entities) (m:MatingComponent) round = 
        m.EntityID 
        |> Engine.Entities.get_Location ent
        |> Engine.Entities.get_AtLocationWithComponent ent Mating.TypeID (Some m.EntityID)
        |> ToMatings
        |> Array.filter (fun m -> m.Species = m.Species && m.MatingStatus = Female && canMate m round) // Same Species & Non-Pregnant Females & Can Retry
    
    let canMate (m:MatingComponent) (round:RoundNumber) =
        (m.MatingStatus <> MatingStatus.Female_Pregnant) && (m.LastMatingAttempt = RoundNumber(0u) || m.LastMatingAttempt + m.Species.MaxMatingFrequency <= round)
    
    let actionEnabled (ent:Entities) (eid:EntityID) (round:RoundNumber) =
        let m = Engine.Entities.get_Component ent ComponentTypes.Mating.TypeID eid |> ToMating
        match m.MatingStatus with
        | Male when canMate m round -> 
            (eligibleFemales ent m round).Length > 0
        | _ -> false
    
    (*
    type MatingSystem(description:string, isActive:bool, enm:EntityManager, evm:EventManager) =
        inherit AbstractSystem(description,isActive) 
        
        member me.onActionMate round (Action_Mate mc:GameEventData) =
            let selectFemale = 
                let mates = 
                    EligibleFemales enm mc round
                    |> Array.sortByDescending (fun m -> m.ChanceOfReproduction)
                match mates with 
                | [||] -> Error "No eligible females present"
                | mc2 -> Ok mc2.[0]
    
            let getEligiblesDecision (mc2:MatingComponent) =
                // Add a better decision process here: can any mates with higher reproductive chance be seen? am I hungry?
                match random.Next(10) with
                | 0 -> Error "Female denied advances"
                | _ -> Ok mc2
    
            let tryMating (mc2:MatingComponent) =
                let chance = mc.ChanceOfReproduction * mc2.ChanceOfReproduction
                let rnd = random.NextDouble()
                enm.UpdateComponent (Mating { mc with LastMatingAttempt = round })
                match chance >= rnd with
                | false -> 
                    enm.UpdateComponent (Mating { mc with LastMatingAttempt = round })
                    Error (sprintf "Reproduction failed (%f<%f)" chance rnd)
                | true ->
                    evm.AddToSchedule { ScheduleType = RunOnce; Frequency = mc2.Species.Gestation; GameEvent = Birth (mc2,mc) }
                    enm.UpdateComponent (Mating { mc2 with LastMatingAttempt = round; MatingStatus = Female_Pregnant }) 
                    Ok (Some (sprintf "Reproduction succeeded (%f >= %f)" chance rnd))
    
            selectFemale
            |> Result.bind getEligiblesDecision
            |> Result.bind tryMating
    
        member me.onBirth round (Birth (mom,_):GameEventData) =
            enm.UpdateComponent (Mating { mom with LastMatingAttempt = round + mom.Species.MaxMatingFrequency; MatingStatus = Female }) // Change Mom to Non-Pregnant Female and add some extra time to before she can mate again
            let adjustComponents (c:Component) =
                match c with
                | Mating d -> 
                    Mating { d with MatingStatus = if random.Next(2) = 0 then Male else Female }
                | Form d -> 
                    Form { d with Location = d.Location + { X = 0; Y = 0; Z = 0 }} 
                | _ -> c
            let newcts = 
                mom.EntityID
                |> EntityExt.CopyEntity enm round
                |> Array.map adjustComponents
            evm.RaiseEvent (CreateEntity newcts)
            Ok (Some (sprintf "Born:%i" (GetComponentEntityID newcts.[0]).ToUint32))
    
        override me.Initialize = 
            evm.RegisterListener me.Description Event_ActionMate (me.TrackTask me.onActionMate)
            evm.RegisterListener me.Description Event_Birth      (me.TrackTask me.onBirth)
            base.SetToInitialized
    
        override me.Update round = 
            ()
    *)


// -----------------------------------------------------------------------------------------------------------------
module Movement = 
    
    let actionsAllowed (game:Game) (eid:EntityID) =
        let mutable _allowed = Array.empty<ActionTypes>
        let move = ToMovement (get_Component game.Entities ComponentTypes.Movement.TypeID eid)
        let loc = get_Location game.Entities eid
        match move.MovesPerTurn = 0 with 
        | true -> _allowed
        | false ->
            if (isOnMap2D game.MapSize (loc + North.Location)) && (isLocationPassible game.Entities (Some eid) (loc + North.Location)) then _allowed <- Array.append _allowed [|Move_North|]
            if (isOnMap2D game.MapSize (loc +  East.Location)) && (isLocationPassible game.Entities (Some eid) (loc +  East.Location)) then _allowed <- Array.append _allowed [|Move_East |]
            if (isOnMap2D game.MapSize (loc + South.Location)) && (isLocationPassible game.Entities (Some eid) (loc + South.Location)) then _allowed <- Array.append _allowed [|Move_South|]
            if (isOnMap2D game.MapSize (loc +  West.Location)) && (isLocationPassible game.Entities (Some eid) (loc +  West.Location)) then _allowed <- Array.append _allowed [|Move_West |]
            _allowed
    
    let onMovement (game:Game) (e:AbstractEventData) =
        let ed = e :?> Action_Movement
        let f = Engine.Entities.get_Component game.Entities ComponentTypes.Form.TypeID ed.ControllerComponent.EntityID |> ToForm
        let dest = 
            f.Location +
            match ed.ControllerComponent.CurrentAction with 
            | Move_North -> North.Location
            | Move_South -> South.Location
            | Move_East  -> East.Location
            | Move_West  -> West.Location
        match (isOnMap2D game.MapSize dest) && (isLocationPassible game.Entities (Some ed.ControllerComponent.EntityID) dest) with
        | false -> Engine.Log.append (Logging.format1 "Err" "Movement System" "onMovement" ed.ControllerComponent.EntityID (Some ed.ControllerComponent.ID) (Some (dest.ToString()))) game
        | true -> 
            let forlog = Logging.format1 "Ok" "Movement System" "onMovement" ed.ControllerComponent.EntityID (Some ed.ControllerComponent.ID) (Some (dest.ToString()))
            Engine.Entities.updateComponent game (FormComponent(f.ID, f.EntityID, f.Born, f.CanSeePast, f.IsPassable, dest, f.Name, f.Symbol)).Abstract (Some forlog)
    
    
// -----------------------------------------------------------------------------------------------------------------
module PlantGrowth = 
    
    let onComponentAdded (game:Game) (e:AbstractEventData) = 
        match (e :?> EngineEvent_ComponentAdded).Component.ComponentType = PlantGrowth.TypeID with
        | false -> game
        | true -> 
            let pg = Entities.get_Component game.Entities PlantGrowth.TypeID e.EntityID |> ToPlantGrowth
            game
            |> ifBind (pg.RegrowRate > 0.0)       (Scheduler.addToSchedule { ScheduleType = RepeatIndefinitely; Frequency = PlantGrowthFrequency; Event = PlantRegrowth(e.EntityID) })
            |> ifBind (pg.ReproductionRate > 0.0) (Scheduler.addToSchedule { ScheduleType = RepeatIndefinitely; Frequency = PlantGrowthFrequency; Event = PlantReproduce(e.EntityID) })
    
    
    let onReproduce (game:Game) (e:AbstractEventData) =
        let pg = Entities.get_Component game.Entities ComponentTypes.PlantGrowth.TypeID e.EntityID :?> PlantGrowthComponent
        let createPlant (l:Location) = 
            let adjustComponents (c:AbstractComponent) =
                c
                |> ifBind 
                    (c.ComponentType = ComponentTypes.Food.TypeID) 
                    (fun ac -> 
                        let f = ToFood ac
                        FoodComponent(f.ID, f.EntityID, f.FoodType, 1, f.QuantityMax).Abstract)
                |> ifBind 
                    (c.ComponentType = ComponentTypes.Form.TypeID) 
                    (fun ac -> 
                        let f = ToForm ac
                        FormComponent(f.ID, f.EntityID, game.Round, f.CanSeePast, f.IsPassable, l, f.Name, f.Symbol).Abstract)
            e.EntityID
            |> Entities.copy game 
            |> Array.map adjustComponents
            |> Entities.create game
    
        let checkReproductionRate r = 
            match pg.ReproductionRate >= r with
            | false -> Error (sprintf "Failed: reproduction rate (%f<%f)" pg.ReproductionRate r)
            | true -> Ok None
    
        let checkOnMap _ =
            let newLocation = addOffset (Entities.get_Location game.Entities pg.EntityID) pg.ReproductionRange pg.ReproductionRange 0 false true
            match isOnMap2D game.MapSize newLocation with
            | false -> Error (sprintf "Failed: location not on map:%s" (newLocation.ToString())) 
            | true -> Ok newLocation
    
        let checkPlantAtLocation newLocation = 
            match (Engine.Entities.get_AtLocationWithComponent game.Entities PlantGrowth.TypeID None newLocation).Length with 
            | x when x > 0 -> Error (sprintf "Failed: plant exists at location:%s" (newLocation.ToString()))
            | _ -> Ok newLocation
    
        let terrainIsSuitable newLocation = 
            match pg.GrowsInTerrain |> Array.contains (ToTerrain (Engine.Entities.get_AtLocationWithComponent game.Entities Terrain.TypeID None newLocation).[0]).Terrain with
            | false -> Error "Failed: terrain is not suitable"
            | true -> Ok newLocation
    
        let checkFoodOnParent newLocation = 
            match (Engine.Entities.tryGet_Component game.Entities ComponentTypes.Food.TypeID pg.EntityID) with
            | None -> Ok (createPlant newLocation)
            | Some ac ->
                let f = ToFood ac
                let pct = float f.Quantity / float f.QuantityMax
                match pg.ReproductionRequiredFoodQuantity < pct with
                | false -> Error (sprintf "Failed: food component quantity below requirement (%f<%f)" pct pg.ReproductionRequiredFoodQuantity)
                | true -> Ok (createPlant newLocation)
    
        Ok (random.NextDouble())
        |> Result.bind checkReproductionRate
        |> Result.bind checkOnMap
        |> Result.bind checkPlantAtLocation
        |> Result.bind terrainIsSuitable
        |> Result.bind checkFoodOnParent
        |> Result.mapError (fun e -> Engine.Log.append (Logging.format1 "Err" "Plant Growth" "onReproduce" pg.EntityID (Some pg.ID) (Some e)) game)
        |> function
            | Error ge -> ge
            | Ok go -> go
        
    
// -----------------------------------------------------------------------------------------------------------------
module Vision = 

    let onLocationChanged (game:Game) (e:AbstractEventData) =
        match Engine.Entities.tryGet_Component game.Entities ComponentTypes.Vision.TypeID e.EntityID with
        | None -> game
        | Some vc ->
            let f = (e :?> LocationChanged).NewForm
            let v = ToVision vc
            Engine.Entities.updateComponent 
                game 
                (VisionComponent(v.ID, v.EntityID, locationsWithinRange2D game.MapSize f.Location v.RangeTemplate, v.Range, v.RangeTemplate, v.VisionCalculationType, v.ViewedHistory, v.VisibleLocations))
                (Some "VisionMap updated")

    let updateViewable (game:Game) = 
        let allForms = 
            Engine.Entities.get_LocationMap game.Entities
            |> Map.map (fun _ v -> ToForms v)
        
        ComponentTypes.Vision.TypeID
        |> Engine.Entities.get_Components_OfType game.Entities
        |> ToVisions
        |> Array.fold (fun (g:Game) vision ->
            let visibleLocations = 
                match vision.VisionCalculationType with
                | Basic_Cheating -> computeVisibility_Basic vision.LocationsWithinRange allForms
                | Shadowcast1 -> computeVisibility_Shadowcast1 (Engine.Entities.get_Location game.Entities vision.EntityID) vision.LocationsWithinRange allForms vision.Range
    
            let fids =
                visibleLocations
                |> Map.toArray
                |> Array.collect snd
                |> Array.map (fun f -> f.ID)
    
            let viewedHistory = 
                vision.ViewedHistory
                |> Map.fold (fun (m:Map<Location,FormComponent[]>) l fs -> 
                    let newFS =
                        match (m.ContainsKey l) with
                        | true -> m.Item l
                        | false -> fs |> Array.filter (fun f -> not (fids |> Array.contains f.ID))
                    m.Add(l,newFS)
                ) visibleLocations
            
            let newVision =
                VisionComponent(vision.ID, vision.EntityID, vision.LocationsWithinRange, vision.Range, vision.RangeTemplate, vision.VisionCalculationType, viewedHistory, visibleLocations)
    
            match (newVision = vision) with
            | true -> g
            | false ->
                Engine.Entities.updateComponent g newVision None
            ) game
    
    

    