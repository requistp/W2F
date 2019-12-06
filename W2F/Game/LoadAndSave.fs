module LoadAndSave
open CommonTypes
open EntityAndGameTypes
open MBrace.FsPickler
open System
open System.IO


type SaveGameFormats =
    | Binary
    | XML
    member me.Ext =
        match me with   
        | Binary -> ".bin"
        | XML -> ".xml"


let private savePath = "./saves"
let private binarySerializer = FsPickler.CreateBinarySerializer()
let private xmlSerializer = FsPickler.CreateXmlSerializer(indent = true)


let private inputStream format filename = 
    let str = 
        match format with
        | Binary -> new StreamReader(File.OpenRead(savePath + "/" + filename + format.Ext))
        | XML -> new StreamReader(File.OpenRead(savePath + "/" + filename + format.Ext))
    str.BaseStream


let private outputStream (format:SaveGameFormats) (round:RoundNumber) = 
    if Directory.Exists(savePath) |> not then Directory.CreateDirectory(savePath) |> ignore
    let str = 
        let filename = savePath + "/" + "Save_" +  (DateTime.Now.ToString("yyyyMMddHHmm")) + "_r" + round.ToUint32.ToString() + format.Ext
        match format with
        | Binary -> new StreamWriter(File.OpenWrite(filename))
        | XML -> new StreamWriter(File.OpenWrite(filename))
    str.AutoFlush <- true
    str.BaseStream
    

let load (format:SaveGameFormats) (filename:string) =
    match format with
    | Binary -> binarySerializer.Deserialize<Game> (inputStream format filename)
    | XML -> xmlSerializer.Deserialize<Game> (inputStream format filename)


let save (format:SaveGameFormats) (game:Game) = 
    let g = { game with Log = Array.empty }
    match format with
    | Binary -> binarySerializer.Serialize(outputStream format g.Round, g)
    | XML -> xmlSerializer.Serialize(outputStream format g.Round, g)


