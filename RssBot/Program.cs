// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.DependencyInjection;

using RssBot;

Console.WriteLine("Hello, World!");
var services = new ServiceCollection();
services.AddLogging();
services.AddScoped<Rss>();
services.AddScoped<Toot>();
services.AddScoped<BotWork>();

var provider = services.BuildServiceProvider();
var botwork = provider.GetRequiredService<BotWork>();
await botwork.RetrieveAndSendToots();


