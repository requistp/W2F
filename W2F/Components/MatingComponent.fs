module MatingComponent
open CommonTypes


type MatingStatus =
    | Female
    | Female_Pregnant
    | Male

    
type Species = 
    | Rabbit
    | NotRabbit
    member me.MaxMatingFrequency =
        match me with
        | Rabbit -> RoundNumber(10u)
        | NotRabbit -> RoundNumber(100u)
    member me.Gestation =
        match me with
        | Rabbit -> RoundNumber(15u)
        | NotRabbit -> RoundNumber(200u)
    

type MatingComponent = 
    { 
        ID : ComponentID
        EntityID : EntityID
        ChanceOfReproduction : float
        LastMatingAttempt : RoundNumber
        MatingStatus : MatingStatus
        Species : Species 
    }


let canMate (m:MatingComponent) (round:RoundNumber) =
    (m.MatingStatus <> MatingStatus.Female_Pregnant) && (m.LastMatingAttempt = RoundNumber(0u) || m.LastMatingAttempt + m.Species.MaxMatingFrequency <= round)


