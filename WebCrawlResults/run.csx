#r "Red-Folder.WebCrawl.dll"

using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using Red_Folder.WebCrawl.Models;

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
    await client.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(databaseName));
    await client.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(databaseName, collectionName));

    // Set some common query options
    FeedOptions queryOptions = new FeedOptions { MaxItemCount = -1 };

    log.Info("Setting up the LINQ query...");
    IEnumerable<CrawlResults> query;
    if (id == null || id.Length == 0)
    {
        query = client.CreateDocumentQuery<CrawlResults>(UriFactory
                    .CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .OrderByDescending(f => f.Timestamp);
    }
    else
    {
        query = client.CreateDocumentQuery<CrawlResults>(UriFactory
                    .CreateDocumentCollectionUri(databaseName, collectionName), queryOptions)
                    .Where(f => f.Id == id);
    }

    log.Info("Running LINQ query...");
    var results = query.FirstOrDefault();

    if (results == null)
    {
        log.Info("Empty results");
    }

    string message = JsonConvert.SerializeObject(results);
    var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(message)
        };

    return response;
}

