using Google;
using Google.Apis.Util;
using Google.Apis.YouTube.v3.Data;
using System.Text.Json;

namespace YoutubeSubVideoManager
{
    internal class Channel
    {
        public readonly Dictionary<string, Video> videos = new();
        DateTime latestVideoPublished = DateTime.MinValue;

        public string Id { get; }
        public string Title { get; }
        public Channel(string id, string title)
        {
            Id = id;
            Title = title;
        }

        string GetCacheDirectory()
        {
            return Path.Combine("cache", $"channel_{Id}");
        }

        string GetCacheFilePath()
        {
            return Path.Combine(GetCacheDirectory(), $"channel_{Id}.json");
        }

        string GetVideoCacheFilePath(string videoId)
        {
            return Path.Combine(GetCacheDirectory(), $"video_{videoId}.json");
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

        /// <summary>
        /// loads channel from cache
        /// </summary>
        /// <returns>returns false if no cache was available</returns>
        /// <exception cref="Exception"></exception>
        bool LoadFromCache()
        {
            var filePath = GetCacheFilePath();
            if (!File.Exists(filePath))
            {
                return false;
            }
            List<string>? channelVideos = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(filePath));
            if (channelVideos == null)
            {
                throw new Exception("cant load channel from " + filePath);
            }
            foreach (var videoId in channelVideos)
            {
                filePath = GetVideoCacheFilePath(videoId);
                var video = Video.FromFile(filePath);
                videos[videoId] = video;
                latestVideoPublished = video.PublishDate > latestVideoPublished ? video.PublishDate : latestVideoPublished;
            }
            return true;
        }

        public void Store()
        {
            Directory.CreateDirectory(GetCacheDirectory());
            File.WriteAllText(GetCacheFilePath(), JsonSerializer.Serialize(videos.Keys));
            foreach (var video in videos)
            {
                video.Value.ToFile(GetVideoCacheFilePath(video.Value.Id));
            }
        }

        void LoadFromYoutube()
        {
            var part = new Repeatable<string>(new string[] { "contentDetails" });

            var listChannel = Program.youtubeService.Channels.List(part);
            listChannel.Id = Id;
            var channelResponse = listChannel.Execute();
            var uploadPlaylistId = channelResponse.Items.First().ContentDetails.RelatedPlaylists.Uploads;


            var listRequest = Program.youtubeService.PlaylistItems.List(part);
            listRequest.PlaylistId = uploadPlaylistId;
            listRequest.MaxResults = 1000;
            while (true)
            {
                PlaylistItemListResponse response;
                try
                {
                    response = listRequest.Execute();
                }
                catch (GoogleApiException e)
                {
                    if (e.Error.Code == 404)
                    {
                        Console.WriteLine("channel with id " + Id + " (" + Title + ") has no videos");
                        break;
                    }
                    throw;
                }
                foreach (var video in response.Items)
                {
                    var videoId = video.ContentDetails.VideoId;
                    var videoPublishDate = video.ContentDetails.VideoPublishedAtDateTimeOffset?.UtcDateTime;
                    if (!videos.ContainsKey(videoId) && videoPublishDate != null)
                    {
                        var vid = new Video(videoId, videoPublishDate.Value);
                        videos.Add(vid.Id, vid);
                        latestVideoPublished = vid.PublishDate > latestVideoPublished ? vid.PublishDate : latestVideoPublished;
                    }
                    //playlists seem to be randomly ordered, so there is no way to know if there arent any more new videos in the list
                }
                if (response.NextPageToken == null)
                {
                    break;
                }
                listRequest.PageToken = response.NextPageToken;
            }
        }
    }
}
