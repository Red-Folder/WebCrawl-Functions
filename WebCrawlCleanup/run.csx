using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Red_Folder.WebCrawl.Data;

public static async Task Run(TimerInfo timerInfo, TraceWriter log)
{
    log.Info($"Web Crawl Clean up triggered");

    // Convert from connection string to uri & key
    // Doesn't currently appear to be a vary to create a DocumentClient from a connection string
    string documentDbEndpoint = System.Environment.GetEnvironmentVariable("APPSETTING_rfcwebcrawl_DOCUMENTDB");
    string endpointUri = documentDbEndpoint.Split(';')[0].Replace("AccountEndpoint=","");
    string primaryKey = documentDbEndpoint.Split(';')[1].Replace("AccountKey=","");
	
    string databaseName = "crawlOutput";
    string collectionName = "WebCrawl";
    DocumentClient client;
    
    log.Info($"Creating client for: {endpointUri}");
    client = new DocumentClient(new Uri(endpointUri), primaryKey);

    // Set some common query options
    FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

	var docCount = client.CreateDocumentQuery<CrawlResults>(UriFactory
                    .CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .ToList()
					.Count;

	log.Info(String.Format("Total of {0} documents found", docCount));
	
	if (docCount > 1)
	{
		log.Info("Setting up the LINQ query to get all docs except latest ...");
		
        var results = client.CreateDocumentQuery<CrawlResults>(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
						.OrderBy(x => x.Timestamp)
						.Take(docCount - 1)
						.AsDocumentQuery();
		
		while (results.HasMoreResults)
		{
			foreach (CrawlResults doc in await results.ExecuteNextAsync())
			{
				log.Info(String.Format("Deleting document {0}", doc.Id.ToString()));
				
				Uri docUri = UriFactory.CreateDocumentUri(databaseName, collectionName, doc.Id);

				// Use this constructed Uri to delete the document
				await client.DeleteDocumentAsync(docUri);
			}
		}
	}
	
	log.Info("Finished");
}

