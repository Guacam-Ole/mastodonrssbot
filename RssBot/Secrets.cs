namespace RssBot
{
    public class Secrets
    {
        public string Instance { get; set; } = "unknown";
        public List<BotSecret> Bots { get; set; }
    }

    public class BotSecret
    {
        public string Id { get; set; }
        public string Secret { get; set; }
    }
}