namespace RssBot.Database
{
    public class TootState
    {
        public string Id { get; set; } = string.Empty;
        public string Hash { get; set; } = string.Empty;
        public string MastodonId { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}