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
    string endpointUri = documentDbEndpoint.Split(';')[0].Replace("AccountEndpoint=","");
    string primaryKey = documentDbEndpoint.Split(';')[1].Replace("AccountKey=","");

    string databaseName = "crawlOutput";
    string collectionName = "WebCrawl";
    DocumentClient client;

    HttpResponseMessage response = null;

    try
    {
        log.Info($"Creating Url for: {endpointUri}");
        var host = new Uri(endpointUri);

        
        log.Info($"Creating client for: {host}");
        client = new DocumentClient(host, primaryKey);
        
        if (client == null)
        {
            log.Info($"Client null");
        }
        else
        {
            log.Info($"Client created");
        }

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
        try
        {
            var results = query.ToList();
            if (results.Count() == 0)
            {
                log.Info("No results found");
                response = new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("No results found.  If asking for a specific request, try again in 60 seconds as it may still be running.")
                };
            }
            else
            {
                var result = results.FirstOrDefault();
                var message = JsonConvert.SerializeObject(result);
                log.Info("Returning OK");
                response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(message)
                };
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }
        }
        catch (Exception ex)
        {
            log.Info($"Failed to retrieve results - exception thrown - {ex.Message}");
            response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("An error has occurred.  Refer to the server logs.")
            };
        }
    }
    catch (Exception ex)
    {
        log.Info($"Failed to retrieve results - exception thrown - {ex.Message}");
        response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("An error has occurred.  Refer to the server logs.")
        };
    }

    return response;
}

