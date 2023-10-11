using Mastonet;
using Mastonet.Entities;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using RssBot.RssBot;

using System.Net.Mail;
using System.Text.RegularExpressions;

namespace RssBot
{
    public class Toot
    {
        private readonly Secrets _secrets;
        private Config _config;
        private readonly ILogger<Toot> _logger;

        public Toot(ILogger<Toot> logger)
        {
            var secrets = File.ReadAllText("./secrets.json");
            _secrets = JsonConvert.DeserializeObject<Secrets>(secrets)!;
            var config = File.ReadAllText("./config.json");
            _config = JsonConvert.DeserializeObject<Config>(config)!;
            _logger = logger;
        }

        private async Task<string?> UploadMedia(MastodonClient client, Stream fileStream, string filename, string description)
        {
            string attachmentId = null;
            try
            {
                _logger.LogDebug("Uploading Image {filename}", filename);
                if (fileStream == null) return null;
                var attachment = await client.UploadMedia(fileStream, filename, description);
                attachmentId = attachment.Id;
            }
            catch (Exception ex)
            {
                return null;
            }
            return attachmentId;
        }

        public async Task<Status?> SendToot(BotConfig botConfig, RssItem rssItem)
        {
            var allTags = string.Empty;
            if (botConfig.ShowTags)
            {
                var ignoreTags = new List<string>();
                if (botConfig.IgnoreTags != null) ignoreTags = botConfig.IgnoreTags.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                allTags = GetTagString(botConfig, rssItem);
            }

            string content = $"{rssItem.Title}\n\n{rssItem.Description}\n\n{rssItem.Url}\n\n{allTags}";
            Stream? imageStream = null;
            if (rssItem.ImageUrl != null)
            {
                imageStream = await DownloadImage(rssItem.ImageUrl);
            }
            return await SendToot(botConfig.Id, content, null, imageStream, rssItem.ImageDescription ?? "Vorschaubild");
        }

        private string GetTagString(BotConfig botConfig, RssItem rssItem)
        {
            var allTags = rssItem.Tags ?? string.Empty;
            foreach (var replacement in _config.TagReplacements) allTags = allTags.Replace(replacement.From, replacement.To);
            List<string> tagList;
            tagList = allTags.Split(allTags.Contains(",") ? "," : " ", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).ToList();

            if (botConfig.IgnoreTags != null)
            {
                var ignoreTags = botConfig.IgnoreTags.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                tagList = tagList.Where(q => !ignoreTags.Contains(q, StringComparer.InvariantCultureIgnoreCase)).ToList();
            }
            if (botConfig.AdditionalTags != null)
            {
                var additionalTags = botConfig.AdditionalTags.Split(" ", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                tagList.AddRange(additionalTags);
            }

            tagList.ForEach(q => q = Regex.Replace(q, "[^A-Za-z0-9]", ""));

            return string.Join(" ", tagList.Distinct().Select(q => "#" + q));
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
                return await client.PublishStatus(content, _config.PrivateOnly ? Visibility.Private : Visibility.Public, replyTo, mediaIds: new List<string> { attachmentId });
            }
            else
            {
                return await client.PublishStatus(content, _config.PrivateOnly ? Visibility.Private : Visibility.Public, replyTo);
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