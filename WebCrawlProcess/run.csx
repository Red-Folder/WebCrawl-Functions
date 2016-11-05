#r "Newtonsoft.Json"

using System;
using Red_Folder.WebCrawl;
using Red_Folder.WebCrawl.Data;
using Red_Folder.Logging;
using Newtonsoft.Json;

public static void Run(string request, out object outputDocument, TraceWriter log)
{
	var crawlRequest = JsonConvert.DeserializeObject<CrawlRequest>(request);
    log.Info($"C# Queue trigger function processed: {crawlRequest.Id}");
    
	var azureLogger = new AzureLogger(log);

    var crawler = new Crawler(crawlRequest, azureLogger);
    crawler.AddUrl($"{crawlRequest.Host}/sitemap.xml");
    var crawlResult = crawler.Crawl();
    
    outputDocument = crawlResult;
}

public class AzureLogger : ILogger
{
	private TraceWriter _log;
	
	public AzureLogger(TraceWriter log)
	{
		_log = log;
	}
	
	public void Info(string message)
	{
		_log.Info(message);
	}
}

