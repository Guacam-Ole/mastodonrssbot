// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RssBot;

var assembly = System.Reflection.Assembly.GetExecutingAssembly();
var attr = Attribute.GetCustomAttribute(assembly, typeof(BuildDateTimeAttribute)) as BuildDateTimeAttribute;
Console.WriteLine("Starting up RSSBot Build " + attr?.Date);

var services = new ServiceCollection();

services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
    var logFile = "rssmastodon.log";
    logging.AddFile(logFile, conf => { conf.Append = true; conf.MaxRollingFiles = 1; conf.FileSizeLimitBytes = 100000; }); 
});
services.AddScoped<Rss>();
services.AddScoped<Toot>();
services.AddScoped<BotWork>();

var provider = services.BuildServiceProvider();
var botwork = provider.GetRequiredService<BotWork>();
await botwork.RetrieveAndSendToots();