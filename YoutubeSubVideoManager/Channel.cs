using Google;
using Google.Apis.Util;
using Google.Apis.YouTube.v3.Data;
using System.Text.Json;

namespace YoutubeSubVideoManager
{
    internal class Channel : LoadStoreable
    {
        public readonly Dictionary<string, Video> videos = new();
        DateTime latestVideoPublished = DateTime.MinValue;
        object lastVideoPublishedLock = new object();

        public string Id { get; }
        public string Title { get; }
        public Channel(string id, string title)
        {
            Id = id;
            Title = title;
        }

        string GetCacheFilePath()
        {
            return Path.Combine(Util.CacheDirectory, $"channel_{Id}.json");
        }

        /// <summary>
        /// loads channel from cache
        /// </summary>
        /// <returns>returns false if no cache was available</returns>
        /// <exception cref="Exception"></exception>
        protected override bool LoadFromCache()
        {
            var filePath = GetCacheFilePath();
            if (!File.Exists(filePath))
            {
                return false;
            }
            List<string>? channelVideos = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(filePath));
            if (channelVideos == null)
            {
                return false;
            }
            foreach (var videoId in channelVideos)
            {
                videos[videoId] = new Video(videoId);
            }
            Parallel.ForEach(videos, x =>
            {
                x.Value.Load();
                lock (lastVideoPublishedLock)
                {
                    latestVideoPublished = x.Value.PublishDate > latestVideoPublished ? x.Value.PublishDate : latestVideoPublished;
                }
            });
            return true;
        }

        public override void Store()
        {
            Directory.CreateDirectory(Util.CacheDirectory);
            File.WriteAllText(GetCacheFilePath(), JsonSerializer.Serialize(videos.Keys));
            Parallel.ForEach(videos, x =>
            {
                x.Value.Store();
            });
        }

        protected override void LoadFromYoutube()
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
                        var vid = new Video(videoId);
                        vid.PublishDate = videoPublishDate.Value;
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
