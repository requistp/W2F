module rec MatingSystem
open CommonFunctions
open ComponentEnums
open Components
open CommonTypes

//let private eligibleFemales (ent:Entities) (m:MatingComponent) round = 
//    m.EntityID 
//    |> Engine.Entities.getLocation ent
//    |> Engine.Entities.getAtLocationWithComponent ent Mating.TypeID (Some m.EntityID)
//    |> ToMatings
//    |> Array.filter (fun m -> m.Species = m.Species && m.MatingStatus = Female && canMate m round) // Same Species & Non-Pregnant Females & Can Retry


//let canMate (m:MatingComponent) (round:RoundNumber) =
//    (m.MatingStatus <> MatingStatus.Female_Pregnant) && (m.LastMatingAttempt = RoundNumber(0u) || m.LastMatingAttempt + m.Species.MaxMatingFrequency <= round)


//let mateActionEnabled (ent:Entities) (eid:EntityID) (round:RoundNumber) =
//    let m = Engine.Entities.getComponent ent ComponentTypes.Mating.TypeID eid |> ToMating
//    match m.MatingStatus with
//    | Male when canMate m round -> 
//        (eligibleFemales ent m round).Length > 0
//    | _ -> false

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
