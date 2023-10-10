using Mastonet;
using Mastonet.Entities;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

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

        public async Task<Status?> SendToot(string botId, string content, string? replyTo, Stream? media)
        {
            _logger.LogDebug("Sending Toot");
            var client = GetServiceClient(botId);
            if (client==null)
            {
                _logger.LogWarning("Bot not found or disabled");
                return null;
            }
            string? attachmentId = null;
            if (media != null) attachmentId = await UploadMedia(client, media, "preview.png", "Vorschaubild zum Kanal");
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