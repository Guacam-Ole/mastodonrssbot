using Mastonet;
using Mastonet.Entities;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RssBot.RssBot;

namespace RssBot
{
    public class Toot
    {
        private readonly Secrets _secrets;
        private readonly ILogger<Toot> _logger;

        public Toot(ILogger<Toot> logger)
        {
            var secrets = File.ReadAllText("./secrets.json");
            _secrets = JsonConvert.DeserializeObject<Secrets>(secrets)!;
            _logger = logger;
        }

        private async Task<string?> UploadMedia(MastodonClient client, Stream fileStream, string filename, string description)
        {
            _logger.LogDebug("Uploading Image");
            if (fileStream == null) return null;
            var attachment = await client.UploadMedia(fileStream, filename, description);
            return attachment.Id;
        }

        public async Task<Status?> SendToot(BotConfig botConfig, RssItem rssItem)
        {
            var allTags = string.Empty;
            if (botConfig.ShowTags)
            {
                var ignoreTags = new List<string>();
                if (botConfig.IgnoreTags != null) ignoreTags = botConfig.IgnoreTags.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                allTags = string.Join(" ", rssItem.Tags.Where(q => !ignoreTags.Contains(q)).Select(q => "#" + q));
            }

            string content = $"{rssItem.Title}\n\n{rssItem.Description}\n{rssItem.Url}\n\n{allTags}";
            Stream? imageStream = null;
            if (rssItem.ImageUrl != null)
            {
                imageStream = await DownloadImage(rssItem.ImageUrl);
            }
            return await SendToot(botConfig.Id, content, null, imageStream, rssItem.ImageDescription ?? "Vorschaubild");
        }

        private async Task<Stream?> DownloadImage(string url)
        {
            HttpClient client = new();
            var response = await client.GetAsync(new Uri(url));
            if (!response.IsSuccessStatusCode) return null; // Dont throw error if just image is missing
            return await response.Content.ReadAsStreamAsync();
        }

        public async Task<Status?> SendToot(string botId, string content, string? replyTo, Stream? media, string altTag)
        {
            _logger.LogDebug("Sending Toot");
            var client = GetServiceClient(botId);
            if (client == null)
            {
                _logger.LogWarning("Bot not found or disabled");
                return null;
            }
            string? attachmentId = null;
            if (media != null) attachmentId = await UploadMedia(client, media, "preview.png", altTag);
            if (attachmentId != null)
            {
                return await client.PublishStatus(content, Visibility.Public, replyTo, mediaIds: new List<string> { attachmentId });
            }
            else
            {
                return await client.PublishStatus(content, Visibility.Public, replyTo);
            }
        }

        private MastodonClient? GetServiceClient(string botId)
        {
            var bot = _secrets.Bots.FirstOrDefault(q => q.Id == botId && !q.Disabled);
            if (bot == null) return null;

            return new MastodonClient(_secrets.Instance, bot.Secret);
        }
    }
}