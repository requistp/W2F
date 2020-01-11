module FileIO
open System
open System.IO

(*
type FileDataAgentMsg = 
    | Close of AsyncReplyChannel<unit> 
    | Write of string 

type FileData = 
    {
        File : Ref<StreamWriter option>
        FileName : string
        Inbox : MailboxProcessor<FileDataAgentMsg>
        FilePath : string
    }
    member me.FullFilePath = me.FilePath + "/" + me.FileName

let private processor (fd:FileData)  =
    async {
        while true do 
        //try
            let! msg = fd.Inbox.Receive()
            match msg with
            | Write s -> ()// logToFile file s
            | Close rc -> 
                //closeLog file
                rc.Reply()
        //with
            //ex -> Console.WriteLine ex.Message
        }
        

type FileAgent(fn:string, fp:string) = 
    let fd = 
        {
            File ref None
            FileName : string
            Inbox : MailboxProcessor<FileDataAgentMsg>
            FilePath : string
        }
    MailboxProcessor.Start logProcessor
*)


(*
type FileDataAgentMsg = 
    | Close of AsyncReplyChannel<unit> 
    | Write of string 


type FileData = 
    {
        File : Ref<StreamWriter option>
        FileName : string
        Inbox : MailboxProcessor<FileDataAgentMsg>
        FilePath : string
    }
    member me.FullFilePath = me.FilePath + "/" + me.FileName


let private processor (fd:FileData) =
    let close  =
        match !fd.File with
        | None -> ()
        | Some (str:StreamWriter) -> 
            str.Flush()
            str.Close()
        fd.File := None
    let newFile = 
        if not (Directory.Exists(fd.FilePath)) then 
            Directory.CreateDirectory(fd.FilePath) |> ignore
        let sw = new StreamWriter(File.OpenWrite(fd.FullFilePath))
        sw.AutoFlush <- true
        sw
    let write s =
        let str =
            match !fd.File with
            | Some sw -> sw
            | None -> 
                let sw = newFile
                fd.File := Some sw
                sw
        str.WriteLine(s:string)

    async {
        while true do 
            try
                let! msg = fd.Inbox.Receive()
                match msg with
                | Close replychannel -> 
                    close
                    replychannel.Reply()
                | Write s -> write s
            with 
                ex -> Console.WriteLine ex.Message
            }


let write (afd:FileData) (s:string) =
    s

type fileWriter(fd:FileData) =
    let mb = MailboxProcessor.Start (processor fd)
*)