using CodeHollow.FeedReader;

using RssBot.RssBot;

using System.Xml.Linq;

namespace RssBot
{
    public static class Helpers
    {
        public static RssItem ToRssItem(this FeedItem feedItem)
        {
            var bestImage = GetBestImage(feedItem.SpecificItem.Element);
            var tags = GetTags(feedItem.SpecificItem.Element);

            return new RssItem
            {
                Title = feedItem.Title,
                Description = feedItem.Description,
                Url = feedItem.Link,
                ImageUrl = bestImage?.Key,
                ImageDescription = bestImage?.Value,
                Tags = tags
            };
        }

        public static string? GetTags(XElement feedElement)
        {
            return feedElement.Descendants().Where(q => q.Name.LocalName == "keywords").FirstOrDefault()?.FirstNode?.ToString();
        }

        public static KeyValuePair<string, string>? GetBestImage(XElement feedElement)
        {
            KeyValuePair<string, string>? bestImage = null;
            var images = feedElement.Descendants().Where(q => q.Name.LocalName == "image").ToList();
            if (images.Count > 0)
            {
                int maxWidth = 0;

                foreach (var image in images)
                {
                    var url = image.Elements().FirstOrDefault(q => q.Name.LocalName == "data")?.FirstNode?.ToString();
                    var width = image.Elements().FirstOrDefault(q => q.Name.LocalName == "width")?.FirstNode?.ToString();
                    var alt = image.Elements().FirstOrDefault(q => q.Name.LocalName == "alt")?.FirstNode?.ToString();
                    if (alt != null && url != null && width != null && int.TryParse(width, out int intWith) && intWith > maxWidth)
                    {
                        maxWidth = intWith;
                        bestImage = new KeyValuePair<string, string>(url, alt);
                    }
                }
            }
            return bestImage;
        }
    }
}