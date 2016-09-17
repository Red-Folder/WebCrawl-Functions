using System;
using Red_Folder.WebCrawl;
using Red_Folder.Logger;

public static void Run(string crawlRequestId, out object outputDocument, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {crawlRequestId}");
    
	var azureLogger = new AzureLogger(log);
	
    var crawler = new Crawler(azureLogger);
    crawler.AddUrl("https://www.red-folder.com/sitemap.xml");
    var crawlResult = crawler.Crawl(crawlRequestId);
    
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

