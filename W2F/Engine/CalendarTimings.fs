module CalendarTimings
open CommonFunctions
open EngineTypes
open System


type FrequencyTypes = 
    | Minute
    | Hour
    | Day
    | Month
    | Year


let secondsPerMinute = 60
let minutesPerHour = 60
let hoursPerDay = 24
let daysPerMonth = 30
let monthsPerYear = 12


let roundInSeconds = 6
let roundsPerMinute = RoundNumber(uint32 (secondsPerMinute / roundInSeconds))           //        10 rounds
let roundsPerHour = RoundNumber(uint32 (int roundsPerMinute.ToUint32 * minutesPerHour)) //       600 rounds
let roundsPerDay = RoundNumber(uint32 (int roundsPerHour.ToUint32 * hoursPerDay))       //    14,400 rounds
let roundsPerMonth = RoundNumber(uint32 (int roundsPerDay.ToUint32 * daysPerMonth))     //   432,000 rounds
let roundsPerYear = RoundNumber(uint32 (int roundsPerMonth.ToUint32 * monthsPerYear))   // 5,184,000 rounds


let convertRounds (r:RoundNumber) (toFreq:FrequencyTypes) = 
    match toFreq with
    | Minute -> (float r.ToUint32) / (float roundsPerMinute.ToUint32)
    | Hour -> (float r.ToUint32) / (float roundsPerHour.ToUint32)
    | Day -> (float r.ToUint32) / (float roundsPerDay.ToUint32)
    | Month -> (float r.ToUint32) / (float roundsPerMonth.ToUint32)
    | Year -> (float r.ToUint32) / (float roundsPerYear.ToUint32)


let convertAmountByFrequency (amountPerFreq:int) (frequencyOfAmount:FrequencyTypes) (perIntervals:RoundNumber) = 
    int (Math.Round((float amountPerFreq) * convertRounds perIntervals frequencyOfAmount, 0))
   

let ExecuteTiming (frequency:RoundNumber) (offset:RoundNumber) (round:RoundNumber) = 
    match frequency.ToUint32 with
    | 0u -> false
    | _ -> round % frequency = offset


let TimingOffset (max:RoundNumber) = RoundNumber(uint32 (random.Next(1,int max.ToUint32))) // Add 1 because the frequency timing is being done off the current round, but that round is really over. So a zero offset should be done next round


let AllTimingAccelerator = RoundNumber(1u)
let MetabolismFrequency = RoundNumber(10u) // roundsPerHour * 2u / AllTimingAccelerator
let PlantGrowthFrequency = RoundNumber(10u) // roundsPerDay / AllTimingAccelerator // If I change this, I need to change the regrowRate because 100% of that is applied per this update
let PlantReproductionFrequency = RoundNumber(10u) // roundsPerMonth / AllTimingAccelerator // If I change this, I need to change the reproductionRate because 100% of that is applied per this update



