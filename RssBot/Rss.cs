using CodeHollow.FeedReader;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RssBot.RssBot;

namespace RssBot
{
    public class Rss
    {
        private readonly ILogger<Rss> _logger;
        private Config _config;

        public Rss(ILogger<Rss> logger)
        {
            _logger = logger;
            var config = File.ReadAllText("./config.json");
            _config = JsonConvert.DeserializeObject<Config>(config) ?? throw new FileNotFoundException("cannot read config");
        }

        public async Task<Dictionary<BotConfig, List<RssItem>>> ReadFeed()
        {
            var unpublishedItems = new Dictionary<BotConfig, List<RssItem>>();
            foreach (var bots in _config.Feeds.Select(q => q.Bots))
            {
                foreach (var bot in bots)
                {
                    unpublishedItems.Add(bot, new List<RssItem>());
                }
            }

            foreach (var feedConfig in _config.Feeds)
            {
                using (var db = new LiteDB.LiteDatabase("state.db"))
                {
                    var states = db.GetCollection<State>();
                    var match = states.FindById(feedConfig.Url);
                    match ??= new State { Id = feedConfig.Url, LastFeed = DateTime.Today.AddMinutes(-10) };

                    

                    var feed = await FeedReader.ReadAsync(feedConfig.Url);
                    if (feed.Type != FeedType.Rss_1_0)
                    {
                        _logger.LogError("Unexpected RSS-Type. Expecting 1.0, received '{type}'", feed.Type);
                        return unpublishedItems;
                    }

                    if (feed.LastUpdatedDate < match.LastFeed)
                    {
                        _logger.LogInformation("Nothing new on feed '{config}' since '{since}'", feedConfig, match.LastFeed);
                        return unpublishedItems;
                    }
                    var newItems = feed.Items.Where(q => q.PublishingDate > match.LastFeed);
                    _logger.LogInformation("Tooting '{count}' feeds since '{lastfeed}'" , newItems.Count(),  match.LastFeed);
                    foreach (var item in newItems)
                    {
                        try
                        {
                            var x = item.SpecificItem.Element.Descendants().ToList();
                            var rssItem = (item.ToRssItem());
                            var bot = GetBotForRssItem(feedConfig, rssItem);
                            if (bot == null) continue;
                            unpublishedItems[bot].Add(rssItem);
                        }
                        catch (Exception ex) 
                        {
                            _logger.LogError( ex, "Cannot toot item {item}", item);
                        }
                    }

                    states.Upsert(match);
                }
            }
            return unpublishedItems;
        }

        private static BotConfig? GetBotForRssItem(FeedConfig config, RssItem item)
        {
            foreach (var bot in config.Bots)
            {
                if (item.Url.Contains(bot.UrlFilter) && (bot.UrlExclude == null || !item.Url.Contains(bot.UrlExclude)))
                {
                    return bot;
                }
            }
            return null;
        }
    }
}