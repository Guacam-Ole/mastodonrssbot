// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;

using RssBot;

Console.WriteLine("Hello, World!");
var services = new ServiceCollection();
services.AddLogging();
services.AddScoped<Rss>();

var provider=services.BuildServiceProvider();
var rss=provider.GetRequiredService<Rss>(); 


await rss.ReadFeed(new FeedConfig { Url = "http://www.ndr.de/nachrichten/hamburg/index-rss.xml" });