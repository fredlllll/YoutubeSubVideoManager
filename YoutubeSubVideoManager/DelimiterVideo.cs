using Google.Apis.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YoutubeSubVideoManager
{
    internal class DelimiterVideo
    {
        public Video Video { get; private set; }
        public string Id { get; }

        public DelimiterVideo(string id)
        {
            Id = id;
        }

        public void Load()
        {
            if (Program.cmdLineArgs.OnlyCache)
            {
                LoadFromCache();
            }
            else
            {
                if (Program.cmdLineArgs.NoCache)
                {
                    LoadFromYoutube();
                }
                else
                {
                    if (!LoadFromCache())
                    {
                        LoadFromYoutube();
                    }
                }
            }
        }

        string GetCacheFilePath()
        {
            return Path.Combine("cache", $"delimiter_video_{Id}.json");
        }

        bool LoadFromCache()
        {
            var filePath = GetCacheFilePath();
            if (!File.Exists(filePath))
            {
                return false;
            }
            Video = JsonSerializer.Deserialize<Video>(File.ReadAllText(filePath));
            return true;
        }

        void LoadFromYoutube()
        {
            var part = new Repeatable<string>(new string[] { "snippet" });
            var listDelimiter = Program.youtubeService.Videos.List(part);
            listDelimiter.Id = Program.cmdLineArgs.AfterVideoId;
            var listDelimiterResponse = listDelimiter.Execute();
            var videoItem = listDelimiterResponse.Items.First();
            Video = new(videoItem.Id, videoItem.Snippet.PublishedAtDateTimeOffset.Value.UtcDateTime);
        }

        public void Store()
        {
            Video.ToFile(GetCacheFilePath());
        }
    }
}
