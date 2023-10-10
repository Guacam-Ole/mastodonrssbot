using CodeHollow.FeedReader;

using Microsoft.Extensions.Logging;

using RssBot.RssBot;

namespace RssBot
{
    public class Rss
    {
        private readonly ILogger<Rss> _logger;

        public Rss(ILogger<Rss> logger)
        {
            _logger = logger;
        }

        public async Task<Dictionary<string, List<RssItem>>> ReadFeed(FeedConfig feedConfig)
        {
            var unpublishedItems = new Dictionary<string, List<RssItem>>();
            foreach (var botId in feedConfig.Bots.Select(q => q.Id))
            {
                unpublishedItems.Add(botId, new List<RssItem>());
            }

            using (var db = new LiteDB.LiteDatabase("state.db"))
            {
                var states = db.GetCollection<State>();
                var match = states.FindById(feedConfig.Url);
                match ??= new State { Id = feedConfig.Url, LastFeed = DateTime.Today.AddDays(-30) };

                match.LastFeed = DateTime.Now.AddDays(-1);      // testing only

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

                foreach (var item in feed.Items.Where(q => q.PublishingDate > match.LastFeed))
                {
                    var x = item.SpecificItem.Element.Descendants().ToList();
                    var rssItem = (item.ToRssItem());
                    var bot = GetBotForRssItem(feedConfig, rssItem);
                    if (bot == null) continue;
                    unpublishedItems[bot.Id].Add(rssItem);
                }

                states.Upsert(match);
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