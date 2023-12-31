using Google.Apis.Util;
using System.Text.Json;

namespace YoutubeSubVideoManager
{
    internal class SubVideoManager : LoadStoreable
    {
        Video delimiterVideo;
        public Video DelimiterVideo { get { return delimiterVideo; } }
        readonly List<Channel> subscriptionChannels = new();
        public IEnumerable<Channel> Subscriptions { get { return subscriptionChannels; } }

        public SubVideoManager()
        {
            delimiterVideo = new Video(Program.cmdLineArgs.AfterVideoId);
        }

        public override void Load()
        {
            delimiterVideo.Load();
            base.Load();
        }

        string GetCacheFilePath()
        {
            return Path.Combine(Util.CacheDirectory, "subscriptionChannels.json");
        }

        protected override bool LoadFromCache()
        {
            var filePath = GetCacheFilePath();
            if (!File.Exists(filePath))
            {
                return false;
            }
            var channels = JsonSerializer.Deserialize<List<Tuple<string, string>>>(File.ReadAllText(GetCacheFilePath()));
            foreach (var channelInfo in channels)
            {
                var channel = new Channel(channelInfo.Item1, channelInfo.Item2);
                subscriptionChannels.Add(channel);
            }
            Parallel.ForEach(subscriptionChannels, (x) =>
            {
                x.Load();
            });
            return true;
        }

        protected override void LoadFromYoutube()
        {
            var part = new Repeatable<string>(new string[] { "snippet" });

            var listSubs = Program.youtubeService.Subscriptions.List(part);
            listSubs.MaxResults = 1000;
            listSubs.Mine = true;
            while (true)
            {
                var subResponse = listSubs.Execute();
                foreach (var sub in subResponse.Items)
                {
                    subscriptionChannels.Add(new Channel(sub.Snippet.ResourceId.ChannelId, sub.Snippet.Title));
                }
                if (subResponse.NextPageToken == null)
                {
                    break;
                }
                listSubs.PageToken = subResponse.NextPageToken;
            }
            Parallel.ForEach(subscriptionChannels, (x) =>
            {
                x.Load();
            });
        }

        public override void Store()
        {
            Directory.CreateDirectory(Util.CacheDirectory);

            delimiterVideo.Store();

            Parallel.ForEach(subscriptionChannels, (x) =>
            {
                x.Store();
            });

            //store tuple of id and title
            List<Tuple<string, string>> channels = subscriptionChannels.Select(x => new Tuple<string, string>(x.Id, x.Title)).ToList();
            File.WriteAllText(GetCacheFilePath(), JsonSerializer.Serialize(channels));
        }
    }
}
