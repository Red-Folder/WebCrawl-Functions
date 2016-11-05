#r "Microsoft.WindowsAzure.Storage"
#r "Newtonsoft.Json"

using System;
using System.Net;
using Microsoft.Azure; // Namespace for CloudConfigurationManager 
using Microsoft.WindowsAzure.Storage; // Namespace for CloudStorageAccount
using Microsoft.WindowsAzure.Storage.Queue; // Namespace for Queue storage types
using Newtonsoft.Json;
using System.Net.Http.Headers;

using Red_Folder.WebCrawl.Data;

public static async Task Run(TimerInfo timerInfo, TraceWriter log)
{
    log.Info($"Request to start WebCrawl");

    var crawlRequest = new CrawlRequest(Guid.NewGuid().ToString(), "https://www.red-folder.com");
    
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
        CloudQueueMessage message = new CloudQueueMessage(JsonConvert.SerializeObject(crawlRequest));

        log.Info("Add the message");
        queue.AddMessage(message);
        
		log.Info($"Completed - added key {crawlRequest.Id}");
    } catch (Exception ex) {
		log.Info($"Failed to handle request - exception thrown - {ex.Message}");
    }
}