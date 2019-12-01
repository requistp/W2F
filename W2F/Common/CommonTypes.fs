module CommonTypes
open CommonFunctions
open System


type ComponentID = 
    | ComponentID of uint32
    member me.ToUint32 = 
        let (ComponentID v) = me
        v


type DistanceType = int16


type EntityID = 
    | EntityID of uint32
    static member (+) (EntityID m1, EntityID m2) = EntityID(m1 + m2)
    static member (+) (EntityID m1, m2:uint32) = EntityID(m1 + m2)
    static member (+) (m1:uint32, EntityID m2) = EntityID(m1 + m2)
    member me.ToUint32 = 
        let (EntityID v) = me
        v


type Location = 
    {
        X : DistanceType
        Y : DistanceType
        Z : DistanceType
    } 
    static member (+) (l1:Location,l2:Location) = { X = l1.X + l2.X; Y = l1.Y + l2.Y; Z = l1.Z + l2.Z }
    static member (-) (l1:Location,l2:Location) = { X = l1.X - l2.X; Y = l1.Y - l2.Y; Z = l1.Z - l2.Z }
    static member empty = { X = 0s; Y = 0s; Z = 0s }
    static member Is000 l = (l = Location.empty)
    static member Offset (rangeX:int) (rangeY:int) (rangeZ:int) (allow000:bool) (doubleRandom:bool) =
        let getNewLocation rnd = 
            {
                X = int16 (random.Next(-rangeX,rangeX+1))
                Y = int16 (random.Next(-rangeY,rangeY+1))
                Z = int16 (random.Next(-rangeZ,rangeZ+1))
            }
        let getNewLocation_double rnd = 
            {
                X = int16 (Math.Round((float (random.Next(-rangeX,rangeX+1)) + float (random.Next(-rangeX,rangeX+1))) / 2.0, 0))
                Y = int16 (Math.Round((float (random.Next(-rangeY,rangeY+1)) + float (random.Next(-rangeY,rangeY+1))) / 2.0, 0))
                Z = int16 (Math.Round((float (random.Next(-rangeZ,rangeZ+1)) + float (random.Next(-rangeZ,rangeZ+1))) / 2.0, 0))
            }
        let newLocation rnd =
            if doubleRandom then getNewLocation_double rnd else getNewLocation rnd

        let mutable l = newLocation random.Next
        while (not allow000 && Location.Is000 l) do
            l <- newLocation random.Next
        l
    static member random (size:Location) = 
        {
            X = int16 (random.Next(0,int size.X))
            Y = int16 (random.Next(0,int size.Y))
            Z = 0s
        }
    override me.ToString() = sprintf "{X=%i, Y=%i, Z=%i}" me.X me.Y me.Z


type RenderTypes =
    | Entity
    | Skip
    | World
    

type RoundNumber =  
    | RoundNumber of uint32
    member me.ToUint32 = 
        let (RoundNumber v) = me
        v
    static member (+) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 + m2)
    static member (+) (RoundNumber m1, m2:uint32) = RoundNumber (m1 + m2)
    static member (+) (m1:uint32, RoundNumber m2) = RoundNumber (m1 + m2)
    static member (-) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 - m2)
    static member (-) (RoundNumber m1, m2:uint32) = RoundNumber (m1 - m2)
    static member (-) (m1:uint32, RoundNumber m2) = RoundNumber (m1 - m2)
    static member (*) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 * m2)
    static member (*) (RoundNumber m1, m2:uint32) = RoundNumber (m1 * m2)
    static member (*) (m1:uint32, RoundNumber m2) = RoundNumber (m1 * m2)
    static member (/) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 / m2)
    static member (/) (RoundNumber m1, m2:uint32) = RoundNumber (m1 / m2)
    static member (/) (m1:uint32, RoundNumber m2) = RoundNumber (m1 / m2)
    static member (%) (RoundNumber m1, RoundNumber m2) = RoundNumber (m1 % m2)
    static member (%) (RoundNumber m1, m2:uint32) = RoundNumber (m1 % m2)
    static member (%) (m1:uint32, RoundNumber m2) = RoundNumber (m1 % m2)




