//#r "Newtonsoft.Json"


using System.Net;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;

public static async Task<HttpResponseMessage> Run(HttpRequestMessage req, TraceWriter log)
{
    log.Info($"C# HTTP trigger function processed a request. RequestUri={req.RequestUri}");

    // parse query parameter
    string name = req.GetQueryNameValuePairs()
        .FirstOrDefault(q => string.Compare(q.Key, "name", true) == 0)
        .Value;

    // Get request body
    dynamic data = await req.Content.ReadAsAsync<object>();

    // Set name to query string or body data
    name = name ?? data?.name;

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

    IQueryable query = client.CreateDocumentQuery(
            UriFactory.CreateDocumentCollectionUri(databaseName, collectionName), queryOptions);
            //.Where(f => f.LastName == "Andersen");

    // The query is executed synchronously here, but can also be executed asynchronously via the IDocumentQuery<T> interface
    log.Info("Running LINQ query...");
    dynamic result = null;
    foreach (var rec in query)
    {
        log.Info("Rec found");
        log.Info(rec.ToString());
        result = rec;
    }
    
    var result2 = new {
      id = 12,
      message = "Hello World"
    };
    
    string message = JsonConvert.SerializeObject(result2);
    var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(message)
        };

    return response;
}

