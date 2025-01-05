namespace RssBot
{
    public class Config
    {
        public bool PrivateOnly { get; set; }   // Don't toot public
        public bool DisableToots { get; set; }
        public bool LoadImages { get; set; }
        public string? IgnoreImageSources { get; set; }
        public List<FeedConfig> Feeds { get; set; } 
        public List<TagReplacement> TagReplacements { get; set; } 
    }

    public class TagReplacement
    {

        public string From { get; set; }    

        public string To { get; set; }  
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
        public bool ShowTags { get; set; }
        public string? IgnoreTags { get; set; }
        public string? AdditionalTags { get; set; }
        public bool ShowImage { get; set; }
        public bool Enabled { get; set; }=true;
    }
}
