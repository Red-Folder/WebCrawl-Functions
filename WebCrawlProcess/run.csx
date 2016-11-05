using System;
using Red_Folder.WebCrawl;
using Red_Folder.WebCrawl.Data;
using Red_Folder.Logging;

public static void Run(CrawlRequest crawlRequest, out object outputDocument, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {crawlRequestId}");
    
	var azureLogger = new AzureLogger(log);
	
    var crawler = new Crawler(crawlRequest, azureLogger);
    crawler.AddUrl("https://www.red-folder.com/sitemap.xml");
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

