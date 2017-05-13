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



type BreathRecord = {
    name: string;
    pressure: float array;
    durationSeconds: float;
}

type BreathSummary = {
    name: string;
    count: int;
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


        let userData  = container.ListBlobs(name)

        let result = 
            { 
                name = name
                count =        
                        userData
                        |> Seq.length
            }

        return req.CreateResponse(HttpStatusCode.OK, JsonConvert.ToString(result));
        
    } |> Async.RunSynchronously
