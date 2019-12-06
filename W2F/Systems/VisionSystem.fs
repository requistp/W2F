module VisionSystem

(*

let UpdateViewableForAll (enm:EntityManager) round = 
    let allForms = enm.GetLocationMap 

    VisionComponent
    |> enm.GetComponentsOfType
    |> Array.Parallel.map ToVision
    |> Array.Parallel.iter (fun vision ->
        let visibleLocations = 
            match vision.VisionCalculationType with
            | Basic_Cheating -> ComputeVisibility_Basic vision.LocationsWithinRange allForms
            | Shadowcast1 -> ComputeVisibility_Shadowcast1 (EntityExt.GetLocation enm vision.EntityID) vision.LocationsWithinRange allForms vision.Range

        let fids =
            visibleLocations
            |> Map.toArray
            |> Array.collect snd
            |> Array.map (fun f -> f.ID)

        let viewedHistory = 
            vision.ViewedHistory
            |> Map.fold (fun (m:Map<LocationDataInt,FormComponent[]>) l fs -> 
                let newFS =
                    match (m.ContainsKey l) with
                    | true -> m.Item l
                    | false -> fs |> Array.filter (fun f -> not (fids |> Array.contains f.ID))
                m.Add(l,newFS)
            ) visibleLocations
        
        let newVision = { vision with VisibleLocations = visibleLocations; ViewedHistory = viewedHistory }

        if (newVision <> vision) then
            enm.UpdateComponent (Vision newVision)
        )

type VisionSystem(description:string, isActive:bool, enm:EntityManager, evm:EventManager) =
    inherit AbstractSystem(description,isActive) 
    
    member private me.onLocationChanged round (LocationChanged form:GameEventData) =
        match EntityExt.TryGetComponent enm VisionComponent form.EntityID with
        | None -> Ok (Some "No vision Component")
        | Some (Vision vision) ->
            enm.UpdateComponent (Vision { vision with LocationsWithinRange = LocationsWithinRange2D form.Location vision.RangeTemplate })
            Ok (Some "VisionMap updated")

    override me.Initialize = 
        evm.RegisterListener me.Description Event_LocationChanged (me.TrackTask me.onLocationChanged)
        base.SetToInitialized

    override me.Update round = 
        () 


*)