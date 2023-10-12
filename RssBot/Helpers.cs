using CodeHollow.FeedReader;

using RssBot.RssBot;

using System.Xml.Linq;

namespace RssBot
{
    public static class Helpers
    {
        public static RssItem? ToRssItem(this FeedItem feedItem)
        {
            var bestImage = GetBestImage(feedItem.SpecificItem.Element);
            var tags = GetTags(feedItem.SpecificItem.Element);
            var identifier = GetIdentifier(feedItem.SpecificItem.Element) ?? feedItem.Id;
            if (string.IsNullOrWhiteSpace(identifier)) return null; // No item without id allowed

            return new RssItem
            {
                Title = feedItem.Title,
                Description = feedItem.Description,
                Url = feedItem.Link,
                Image = GetBestImage(feedItem.SpecificItem.Element),
                Tags = tags,
                Identifier = identifier,
                ItemType = GetArticleType(feedItem.SpecificItem.Element)
            };
        }

        private static string? GetArticleType(XElement feedElement)
        {
            return feedElement.Descendants().FirstOrDefault(q => q.Name.LocalName == "type")?.FirstNode?.ToString();
        }

        public static string? GetIdentifier(this FeedItem feedItem)
        {
            return GetIdentifier(feedItem.SpecificItem.Element) ?? feedItem.Id;
        }

        private static string? GetIdentifier(XElement feedElement)
        {
            var id = feedElement.Descendants().FirstOrDefault(q => q.Name.LocalName == "identifier")?.FirstNode?.ToString();
            if (string.IsNullOrWhiteSpace(id)) return null;
            return id.ToString();
        }

        public static string? GetTags(XElement feedElement)
        {
            return feedElement.Descendants().Where(q => q.Name.LocalName == "keywords").FirstOrDefault()?.FirstNode?.ToString();
        }

        public static RssImage? GetBestImage(XElement feedElement)
        {
            RssImage? bestImage = null;
            var images = feedElement.Descendants().Where(q => q.Name.LocalName == "image").ToList();
            if (images.Count > 0)
            {
                int maxWidth = 0;

                foreach (var image in images)
                {
                    var url = image.Elements().FirstOrDefault(q => q.Name.LocalName == "data")?.FirstNode?.ToString();
                    var width = image.Elements().FirstOrDefault(q => q.Name.LocalName == "width")?.FirstNode?.ToString();
                    var alt = image.Elements().FirstOrDefault(q => q.Name.LocalName == "alt")?.FirstNode?.ToString();
                    var source = image.Elements().FirstOrDefault(q => q.Name.LocalName == "source")?.FirstNode?.ToString();
                    if (alt != null && url != null && width != null && int.TryParse(width, out int intWith) && intWith > maxWidth)
                    {
                        maxWidth = intWith;
                        bestImage = new RssImage { Url = url, Description = alt, Source = source };
                    }
                }
            }
            return bestImage;
        }
    }
}