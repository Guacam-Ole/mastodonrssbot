namespace RssBot.RssBot
{
    public class RssItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public string? ImageUrl { get; set; }
        public string? ImageDescription { get; set; }
        public List<string>? Tags { get; set; };
    }
}