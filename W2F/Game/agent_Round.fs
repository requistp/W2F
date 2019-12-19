module agent_Round
open agent_IDManager
open CommonTypes


type agent_Round() =

    let roundManager = new agent_IDManager()

    member _.get() = RoundNumber(roundManager.get())
    member _.init r = roundManager.init r
    member _.increment = roundManager.newID()

