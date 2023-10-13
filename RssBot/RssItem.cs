using System.Security.Cryptography;
using System.Text;

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

        public string GetHash()
        {
            var source = $"{Title}{Description}{Url}{Tags}";

            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(source));
                var sBuilder = new StringBuilder();
                for (int i = 0; i < data.Length; i++) sBuilder.Append(data[i].ToString("x2"));
                return sBuilder.ToString();
            }
        }
    }

    public class RssImage
    {
        public string Url { get; set; }
        public string? Source { get; set; }
        public string? Description { get; set; }
    }
}