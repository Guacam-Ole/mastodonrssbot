namespace RssBot.RssBot
{
    public class RssItem
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string Url { get; set; }
        public string? Tags { get; set; }
        public string? ItemType { get; set; }
        public RssImage? Image { get; set; }
        public string Identifier { get; set; }  
    }

    public class RssImage
    {
        public string Url { get; set; }
        public string? Source { get; set; }
        public string? Description { get; set; }
    }
}