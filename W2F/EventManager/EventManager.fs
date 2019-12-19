module EventManager
open agent_EventListeners
open agent_EventSchedule


type EventManager() = 
    let agent_Listeners = new agent_EventListeners()
    let agent_Schedule = new agent_EventSchedule()
    
    member _.register listener = agent_Listeners.register listener
    member _.registers listeners = agent_Listeners.registers listeners