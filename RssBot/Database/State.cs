namespace RssBot
{
    public class State
    {
        public DateTime? LastFeed { get; set; }
        public string? Id { get; set; }

        public List<PostedItem> PostedItems { get; set; } = new List<PostedItem>();
    }

    public class PostedItem
    {
        public DateTime? ReadDate { get; set; }
        public string? Id { get; set; }
    }
}