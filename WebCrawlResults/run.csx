using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Red_Folder.WebCrawl.Data;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    // parse query parameter
    string id = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "id", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set id to query string or body data
    id = id ?? data?.id;

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

    log.Info("Setting up the LINQ query...");
    IEnumerable<CrawlResults> query;
    if (id == null || id.Length == 0)
    {
        query = client.CreateDocumentQuery<CrawlResults>(UriFactory
                    .CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .OrderByDescending(x => x.Timestamp);
    }
    else
    {
        query = client.CreateDocumentQuery<CrawlResults>(UriFactory
                    .CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .Where(x => x.Id == id);
    }

    log.Info("Running LINQ query...");
    string message = "";
    try
    {
        var results = query.ToList().FirstOrDefault();
        message = JsonConvert.SerializeObject(results);
    }
    catch (Exception ex)
    {
        log.Info($"Failed to retrieve results - exception thrown - {0}", ex.Message);
    }

	if (message == null)
	{
		var response = new HttpResponseMessage(HttpStatusCode.NotFound)
		{
            Content = new StringContent("No results found.  If asking for a specific request, try again in 60 seconds as it may still be running.")
        };
	}
	else
	{
		var response = new HttpResponseMessage(HttpStatusCode.OK)
		{
            Content = new StringContent(message)
        };
	}
	
    return response;
}

