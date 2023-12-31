using Google.Apis.Util;
using System.Text.Json;

namespace YoutubeSubVideoManager
{
    public class Video : LoadStoreable
    {
        private class VideoDTO
        {
            public string Id { get; set; } = string.Empty;
            public DateTime PublishDate { get; set; }
        }

        public string Id { get { return dto.Id; } }
        public DateTime PublishDate { get { return dto.PublishDate; } set { dto.PublishDate = value; } }

        private VideoDTO dto;

        public Video(string id)
        {
            dto = new VideoDTO
            {
                Id = id
            };
        }

        string GetCacheFilePath()
        {
            return Path.Combine(Util.CacheDirectory, $"video_{Id}.json");
        }

        protected override bool LoadFromCache()
        {
            var filePath = GetCacheFilePath();
            if (!File.Exists(filePath))
            {
                return false;
            }
            var tmp = JsonSerializer.Deserialize<VideoDTO>(File.ReadAllText(filePath));
            if (tmp == null)
            {
                return false;
            }
            dto = tmp;
            return true;
        }

        protected override void LoadFromYoutube()
        {
            var part = new Repeatable<string>(new string[] { "snippet" });
            var listDelimiter = Program.youtubeService.Videos.List(part);
            listDelimiter.Id = Program.cmdLineArgs.AfterVideoId;
            var listDelimiterResponse = listDelimiter.Execute();
            var videoItem = listDelimiterResponse.Items.First();
            dto = new VideoDTO() { Id = videoItem.Id, PublishDate = videoItem.Snippet.PublishedAtDateTimeOffset.Value.UtcDateTime };
        }

        public override void Store()
        {
            Directory.CreateDirectory(Util.CacheDirectory);
            string text = JsonSerializer.Serialize<VideoDTO>(dto);
            File.WriteAllText(GetCacheFilePath(), text);
        }
    }
}
