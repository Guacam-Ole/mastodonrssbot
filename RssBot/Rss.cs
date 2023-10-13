using CodeHollow.FeedReader;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RssBot.Database;
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
                    var tootStates = db.GetCollection<TootState>();
                    var match = states.FindById(feedConfig.Url);

                    var feed = await FeedReader.ReadAsync(feedConfig.Url);
                    if (feed.Type != FeedType.Rss_1_0)
                    {
                        _logger.LogError("Unexpected RSS-Type. Expecting 1.0, received '{type}'", feed.Type);
                        return unpublishedItems;
                    }

                    if (match == null)
                    {
                        // first start, mark all as already sent
                        match = new State { Id = feedConfig.Url, LastFeed = DateTime.Now };

                        foreach (var item in feed.Items)
                        {
                            var id = item.GetIdentifier();
                            if (id != null) match.PostedItems.Add(new PostedItem { Id = id, ReadDate = DateTime.Now });
                        }
                    }
                    else
                    {
                        // cleanup old stuff
                        match.PostedItems.RemoveAll(q => q.ReadDate < DateTime.Now.AddDays(-120));
                    }
                  
                    int newItemCount = 0;
                    foreach (var item in feed.Items)
                    {
                        try
                        {
                            var x = item.SpecificItem.Element.Descendants().ToList();
                            var rssItem = item.ToRssItem();
                            if (rssItem == null) continue;
                            if (match.PostedItems.Any(q => q.Id == rssItem.Identifier))
                            {
                                // Already posted, check updates
                                var tootState = tootStates.FindById(rssItem.Identifier);
                                if (tootState == null) 
                                    continue; // Posted but don't know what Id
                                if (tootState.Hash == rssItem.GetHash())
                                    continue; // Posted with the same content
                            }
                            var bot = GetBotForRssItem(feedConfig, rssItem);
                            if (bot == null) continue;
                            newItemCount++;
                            match.PostedItems.Add(new PostedItem { Id = rssItem.Identifier, ReadDate = DateTime.Now });
                            unpublishedItems[bot].Add(rssItem);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Cannot toot item {item}", item);
                        }

                    }
                    match.LastFeed = DateTime.Now;
                    _logger.LogInformation("Tooting '{count}' feed items since '{lastfeed}'", newItemCount, match.LastFeed);

                    if (!_config.DisableToots) states.Upsert(match);
                }
            }
            return unpublishedItems;
        }

        private static bool UrlHasExcludes(string url, string? excludes)
        {
            if (excludes == null) return false;
            var excludeList = excludes.Split(" ");
            foreach (var exclude in excludeList)
            {
                if (url.Contains(exclude, StringComparison.CurrentCultureIgnoreCase)) return true;
            }
            return false;
        }

        private static BotConfig? GetBotForRssItem(FeedConfig config, RssItem item)
        {
            foreach (var bot in config.Bots)
            {
                if (item.Url.Contains(bot.UrlFilter, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (UrlHasExcludes(item.Url, bot.UrlExclude)) continue;
                    return bot;
                }
            }
            return null;
        }
    }
}