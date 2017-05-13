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

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        log.Info(sprintf 
            "F# HTTP trigger function processed a request.")

        let! data = req.Content.ReadAsStringAsync() |> Async.AwaitTask
        
        let breathRecord = JsonConvert.DeserializeObject<BreathRecord>(data)

        log.Info(ConfigurationManager.AppSettings.["AzureWebJobsStorage"])

        // Retrieve storage account from connection string.
        let storageAccount = 
                    CloudStorageAccount.Parse(ConfigurationManager.AppSettings.["AzureWebJobsStorage"])
        
        // Create the blob client.
        let blobClient = storageAccount.CreateCloudBlobClient();

        // Retrieve reference to a previously created container.
        let container = blobClient.GetContainerReference("breaths");
        
        container.CreateIfNotExists() |> ignore
        let dt = DateTime.UtcNow;
        // Retrieve reference to a blob named "myblob".
        let blockBlob = container.GetBlockBlobReference(breathRecord.name + "/" + dt.ToFileTime().ToString());
//        let! leaseId = blockBlob.AcquireLeaseAsync(Nullable(), null) |> Async.AwaitTask
        let! blobContents = blockBlob.DownloadTextAsync() |> Async.AwaitTask

//        let releaseCondition = AccessCondition()
//        releaseCondition.LeaseId <- leaseId;

//        let task = blockBlob.ReleaseLeaseAsync(releaseCondition)
//        task.Wait();



        if not (String.IsNullOrEmpty(data)) then
            
            return req.CreateResponse(HttpStatusCode.OK, "Thanks " + breathRecord.name + ", well done");
        else
            return req.CreateResponse(HttpStatusCode.BadRequest, "Specify a Name value");
    } |> Async.RunSynchronously
