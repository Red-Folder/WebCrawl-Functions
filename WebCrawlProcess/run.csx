#r "Red-Folder.WebCrawl.dll"

using System;
using Red_Folder.WebCrawl;

public static void Run(string crawlRequestId, out object outputDocument, TraceWriter log)
{
    log.Info($"C# Queue trigger function processed: {crawlRequestId}");
    
    var crawler = new Crawler(log);
    crawler.AddUrl("https://www.red-folder.com/sitemap.xml");
    var crawlResult = crawler.Crawl(crawlRequestId);
    
    outputDocument = crawlResult;
}


