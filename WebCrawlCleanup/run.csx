using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Red_Folder.WebCrawl.Data;

public static async Task<HttpResponseMessage> Run(TimerInfo timerInfo, TraceWriter log)
{
    log.Info($"Web Crawl Clean up triggered");

    // Convert from connection string to uri & key
    // Doesn't currently appear to be a vary to create a DocumentClient from a connection string
    string documentDbEndpoint = System.Environment.GetEnvironmentVariable("APPSETTING_rfcwebcrawl_DOCUMENTDB");
    string endpointUri = documentDbEndpoint.Split(';')[0].Split('=')[1];
    string primaryKey = documentDbEndpoint.Split(';')[1].Split('=')[1];

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
					.Count();

	log.Info($"Total of {0} documents found");
	
	if (docCount > 1)
	{
		log.Info("Setting up the LINQ query to get all docs except latest ...");
		var sqlQuery = String.Format("select top {0} from c order by c.timestamp", docCount - 13);

		//Get a reference to the collection
		DocumentCollection coll = client.CreateDocumentCollectionQuery(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
			.ToArray()
			.FirstOrDefault();

		//First execution of the query
		var results = client.CreateDocumentQuery<Document>(coll.DocumentsLink, sqlQuery).AsDocumentQuery();
		
        //var query = client.CreateDocumentQuery<CrawlResults>(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
        //            .OrderBy(x => x.Timestamp)
		//				.Take(docCount - 13)
		//			.AsDocumentQuery();
		
		while (results.HasMoreResults)
		{
			foreach (Document doc in results.ExecuteNextAsync())
			{
				log.Info($"Deleting document {0}", doc.id);
				//await client.DeleteDocumentAsync(doc.SelfLink);
			}
		}
	}
	
	log.Info("Finished");
}

