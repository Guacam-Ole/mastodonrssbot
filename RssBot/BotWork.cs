using Microsoft.Extensions.Logging;

namespace RssBot
{
    public class BotWork
    {
        private readonly ILogger<BotWork> _logger;
        private readonly Rss _rss;
        private readonly Toot _toot;

        public BotWork(ILogger<BotWork> logger, Rss rss, Toot toot)
        {
            _logger = logger;
            _rss = rss;
            _toot = toot;
        }

        public async Task RetrieveAndSendToots()
        {
            var newFeedItems = await _rss.ReadFeed();
            foreach (var botItems in newFeedItems)
            {
                foreach (var item in botItems.Value)
                {
                    try
                    {
                        await _toot.SendToot(botItems.Key, item);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "failed sending toot for {key}, {item}", botItems.Key, item);
                        throw;
                    }
                }
            }
        }
    }
}