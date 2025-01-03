namespace RssBot.Database
{
    public class TootState
    {
        public string Id { get; set; }
        public string Hash { get; set; }
        public string MastodonId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
    }
}