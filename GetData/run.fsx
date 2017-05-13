#if !COMPILED
#I @"C:\Users\aidan.treasure\AppData\Roaming\npm\node_modules\azure-functions-cli\bin"
#r "Microsoft.Azure.WebJobs.Host.dll"
#r "System.Web.Http.dll"
#r "System.Net.Http.dll"
#r "Newtonsoft.Json.dll"
#r "System.Net.Http.Formatting.dll"
#r "System.Net.Http.Extensions.dll"
#endif


#r "System.Configuration"
#r "System.Net.Http"
#r "Newtonsoft.Json"
//#r "FSharp.Data"


open System
open System.Configuration
open System.Net
open System.Net.Http
open Newtonsoft.Json
//open FSharp.Data

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host

#r "Microsoft.WindowsAzure.Storage"
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob

#r "System.Runtime.Serialization.dll"
open System.Runtime.Serialization

type BreathRecord = {
    name: string;
    pressure: float array;
    durationSeconds: float;
}

[<DataContract>]
type BreathSummary = {
    [<field: DataMember(Name="name")>]
    name: string;
    [<field: DataMember(Name="count")>]
    count: int;

    [<field: DataMember(Name="averageDuration")>]
    averageDuration: float;

    [<field: DataMember(Name="minPressure")>]
    minPressure: float;

    [<field: DataMember(Name="maxPressure")>]
    maxPressure: float;
}

let Run(req: HttpRequestMessage, name: string, log: TraceWriter) =
    async {

        let! data = req.Content.ReadAsStringAsync() |> Async.AwaitTask
        
        //let breathRecord = JsonConvert.DeserializeObject<BreathRecord>(data)

        // Retrieve storage account from connection string.
        let storageAccount = 
                    CloudStorageAccount.Parse(ConfigurationManager.AppSettings.["AzureWebJobsStorage"])
        
        log.Info(sprintf 
            "F# HTTP trigger function processed a request.")

        // Create the blob client.
        let blobClient = storageAccount.CreateCloudBlobClient();

        // Retrieve reference to a previously created container.
        let container = blobClient.GetContainerReference("breaths");
        container.CreateIfNotExists() |> ignore

        log.Info(sprintf 
            "F# HTTP trigger function processed a request.")

        //let blob = container.GetBlockBlobReference(sprintf "%s" name)
        //blob.DownloadTextAsync();
        let userData  = 
                    container.ListBlobs(name)
                    |> Seq.map (fun x -> 
                                    x.Container.GetBlockBlobReference(x.StorageUri.ToString())
                                    |> fun y -> JsonConvert.DeserializeObject<BreathRecord>(y.DownloadText()))

        let result = 
            { 
                name = name
                count =        
                        userData
                        |> Seq.length
                averageDuration = 
                        userData
                        |> Seq.averageBy (fun x -> x.durationSeconds) 
                minPressure =
                        userData
                        |> Seq.map (fun x -> x.pressure |> Array.min) 
                        |> Seq.min
                maxPressure =
                        userData
                        |> Seq.map (fun x -> x.pressure |> Array.max) 
                        |> Seq.max
            }
 

        log.Info(sprintf 
            "F# HTTP trigger function processed a request.")

        let json =  JsonConvert.SerializeObject(result)

        return req.CreateResponse(HttpStatusCode.OK, result);
        
    } |> Async.RunSynchronously
