﻿// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using RssBot;

Console.WriteLine("Hello, World!");
var services = new ServiceCollection();

services.AddLogging(logging =>
{
    logging.ClearProviders();
    logging.AddConsole();
    logging.SetMinimumLevel(LogLevel.Debug);
    var logFile = "rssmastodon.log";
    logging.AddFile(logFile, append: true);
});
services.AddScoped<Rss>();
services.AddScoped<Toot>();
services.AddScoped<BotWork>();

var provider = services.BuildServiceProvider();
var botwork = provider.GetRequiredService<BotWork>();
await botwork.RetrieveAndSendToots();




