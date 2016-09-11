#r "Microsoft.WindowsAzure.Storage"

using System;
using System.Net;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"Request to start WebCrawl");

    var requestId = Guid.NewGuid();
    
    try
    {
        string storageConnectionString = System.Environment.GetEnvironmentVariable("APPSETTING_rfcwebcrawl_STORAGE");
    
        // Parse the connection string and return a reference to the storage account.
        log.Info("Login to the storage account");
        CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

        // Create the queue client.
        log.Info("Create the queueClient");
        CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

        // Retrieve a reference to a container.
        log.Info("Create reference to the toWebCrawl queue");
        CloudQueue queue = queueClient.GetQueueReference("towebcrawl");

        // Create the queue if it doesn't already exist
        log.Info("Create the queue if needed");
        queue.CreateIfNotExists();
    
        // Create a message and add it to the queue.
        log.Info("Create the message");
        CloudQueueMessage message = new CloudQueueMessage(requestId.ToString());

        log.Info("Add the message");
        queue.AddMessage(message);
        
        return req.CreateResponse(HttpStatusCode.OK, requestId); 
    } catch (Exception ex) {
        log.Error(ex.Message);
        return req.CreateResponse(HttpStatusCode.InternalServerError, ex.Message); 
    }
    
}