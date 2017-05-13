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
#r "FSharp.Data"


open System
open System.Configuration
open System.Net
open System.Net.Http
open Newtonsoft.Json
open FSharp.Data

open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Host

#r "Microsoft.WindowsAzure.Storage"
open Microsoft.WindowsAzure.Storage
open Microsoft.WindowsAzure.Storage.Blob

type Named = {
    name: string
}

let Run(req: HttpRequestMessage, id: string, log: TraceWriter) =
    async {
        
        // Retrieve storage account from connection string.
        let storageAccount = 
                    CloudStorageAccount.Parse(ConfigurationManager.AppSettings.["AzureWebJobsStorage"])
        
        // Create the blob client.
        let blobClient = storageAccount.CreateCloudBlobClient();

        // Retrieve reference to a previously created container.
        let container = blobClient.GetContainerReference("games");
        
        container.CreateIfNotExists() |> ignore

        // Retrieve reference to a blob named "myblob".
        let blockBlob = container.GetBlockBlobReference(id);
        let! leaseId = blockBlob.AcquireLeaseAsync(Nullable(), null) |> Async.AwaitTask
        let! blobContents = blockBlob.DownloadTextAsync() |> Async.AwaitTask

        let releaseCondition = AccessCondition()
        releaseCondition.LeaseId <- leaseId;

        let task = blockBlob.ReleaseLeaseAsync(releaseCondition)
        task.Wait();

        return req.CreateResponse(HttpStatusCode.OK, blobContents);

    } |> Async.RunSynchronously
