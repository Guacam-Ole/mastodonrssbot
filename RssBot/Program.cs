﻿// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RssBot;

var assembly = System.Reflection.Assembly.GetExecutingAssembly();
var attr = Attribute.GetCustomAttribute(assembly, typeof(BuildDateTimeAttribute)) as BuildDateTimeAttribute;
int maxTries = 5;
Console.WriteLine("Starting up RSSBot Build " + attr?.Date);

var services = new ServiceCollection();

services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
    var logFile = "rssmastodon.log";
    logging.AddFile(logFile, conf =>
    {
        conf.Append = true;
        conf.MaxRollingFiles = 1;
        conf.FileSizeLimitBytes = 100000;
    });
});
services.AddScoped<Rss>();
services.AddScoped<Toot>();
services.AddScoped<BotWork>();
services.AddSingleton<Config>(JsonConvert.DeserializeObject<Config>(File.ReadAllText("./config.json")));
services.AddSingleton<Secrets>(JsonConvert.DeserializeObject<Secrets>(File.ReadAllText("./secrets.json")));

var provider = services.BuildServiceProvider();
var botwork = provider.GetRequiredService<BotWork>();

var retries = maxTries;
while (true)
{
    try
    {
        Thread.Sleep(1000*60*5);
        await botwork.RetrieveAndSendToots();
        retries = maxTries;
    }
    catch (Exception e)
    {
        retries--;
        Console.WriteLine($"'{retries}' retries left: {e.Message}");
        if (retries == 0) return;
    }
}