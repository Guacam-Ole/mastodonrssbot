namespace RssBot
{
    public class Config
    {
        public List<FeedConfig> Feeds { get; set; }
    }

    public class FeedConfig
    {
        public string Url { get; set; }
        public List<BotConfig> Bots { get; set; }
    }

    public class BotConfig
    {
        public string Id { get; set; }
        public string UrlFilter { get; set; }
        public string? UrlExclude { get; set; }
        public string? TypeFilter { get; set; }
        public bool ShowImage { get; set; }
        public bool ShowTags { get; set; }
        public string? IgnoreTags { get; set; }
    }
}