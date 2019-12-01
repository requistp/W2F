module PlantGrowthComponent
open CommonTypes
open TerrainComponent

type PlantGrowthComponent = 
    { 
        //ID : ComponentID
        EntityID : EntityID
        GrowsInTerrain : TerrainTypes[]
        RegrowRate : float
        ReproductionRate : float
        ReproductionRange : int
        ReproductionRequiredFoodQuantity : float 
    } 