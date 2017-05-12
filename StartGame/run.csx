//#if !COMPILED
//#I @"C:\Users\aidan.treasure\AppData\Roaming\npm\node_modules\azure-functions-cli\bin"
// #r "Microsoft.Azure.WebJobs.Host.dll"
// #r "System.Web.Http.dll"
// #r "System.Net.Http.dll"
// #r "Newtonsoft.Json.dll"
// #r "System.Net.Http.Formatting.dll"
// #r "System.Net.Http.Extensions.dll"
#r "Microsoft.WindowsAzure.Storage"
#r "System.Configuration"
//#endif

using System;
using System.Net;
using System.Configuration;

using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;



public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, string name, TraceWriter log)
{    
    log.Info("C# HTTP trigger function processed a request.");
    
    // Retrieve storage account from connection string.
    CloudStorageAccount storageAccount =
            CloudStorageAccount.Parse(ConfigurationManager.AppSettings["AzureWebJobsStorage"]);

    // Create the blob client.
    CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

    // Retrieve reference to a previously created container.
    CloudBlobContainer container = blobClient.GetContainerReference("games");
    container.CreateIfNotExists();
    
    var gameGuid = Guid.NewGuid();
    
    CloudBlockBlob blockBlob = container.GetBlockBlobReference(gameGuid.ToString());
    blockBlob.UploadText("{ state: \"WaitingForPlayers\" }");

    // Fetching the name from the path parameter in the request URL
    return req.CreateResponse(HttpStatusCode.OK, "Hello " + name + gameGuid);
}