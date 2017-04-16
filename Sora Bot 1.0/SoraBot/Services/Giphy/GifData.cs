using System;
using System.Collections.Generic;
using System.Text;

namespace Sora_Bot_1.SoraBot.Services.Giphy
{
    public class GifData
    {
        public List<Data> data { get; set; }
        public Meta meta { get; set; }
        public Pagination pagination { get; set; }
    }

    public class Data
    {
        public string type { get; set; }
        public string id { get; set; }
        public string slug { get; set; }
        public string url { get; set; }
        public string bitlyGifUrl { get; set; }
        public string bitlyUrl { get; set; }
        public string embedUrl { get; set; }
        public string username { get; set; }
        public string source { get; set; }
        public string rating { get; set; }
        public string caption { get; set; }
        public string contentUrl { get; set; }
        public string sourceTld { get; set; }
        public string sourcePostUrl { get; set; }
        public string importDateTime { get; set; }
        public string trendingDateTime { get; set; }
        public Images images { get; set; }
    }

    public class Pagination
    {
        public int totalCount { get; set; }
        public int count { get; set; }
        public int offset { get; set; }
    }

    public class Meta
    {
        public int status { get; set; }
        public string msg { get; set; }
    }

    public class Images
    {
        public Original original { get; set; }
    }

    public class Original
    {
        public string url { get; set; }
        public int width { get; set; }
        public int height { get; set; }
        public int size { get; set; }
        public int frames { get; set; }
        public string mp4 { get; set; }
        public string mp4Size { get; set; }
        public string webp { get; set; }
        public int webpSize { get; set; }
    }
}
