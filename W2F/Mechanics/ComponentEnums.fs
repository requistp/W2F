module ComponentEnums
open EngineTypes


type ActionTypes = 
    | Eat
    | ExitGame
    | Idle
    | Mate
    | Move_East
    | Move_North
    | Move_South
    | Move_West
    static member AsArray = 
        [|
            Eat
            ExitGame
            Idle
            Mate
            Move_East
            Move_North
            Move_South
            Move_West
        |]


type ControllerTypes = 
    | AI_Random
    | Keyboard


type FoodClassifications =
    | Meat
    | Plant


type FoodTypes =
    | Food_Carrot
    | Food_Grass
    | Food_Meat_Rabbit
    member this.Calories = // This is calories per quantity (eaten)
        match this with 
        | Food_Carrot -> 2
        | Food_Grass -> 1
        | Food_Meat_Rabbit -> 3
    member this.Classification =
        match this with
        | Food_Carrot | Food_Grass -> Plant
        | Food_Meat_Rabbit -> Meat
    member this.KillOnAllEaten =
        match this with 
        | Food_Grass | Food_Meat_Rabbit -> false
        | Food_Carrot -> true
    member this.Symbol =
        match this.Classification with
        | Plant -> Some '!'
        | Meat -> None


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


type TerrainTypes = 
    | Dirt 
    | Rock
    | Sand
    member me.IsPassable = 
        match me with
        | Dirt | Sand -> true
        | Rock -> false
    member me.Symbol = 
        match me with
        | Dirt -> '.'
        | Sand -> ','
        | Rock -> '#'


type VisionCalculationTypes =
    | Basic_Cheating
    | Shadowcast1
