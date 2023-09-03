using Google.Apis.Util;
using Google.Apis.YouTube.v3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YoutubeSubVideoManager
{
    internal class SubVideoManager
    {
        DelimiterVideo delimiterVideo;
        public DelimiterVideo DelimiterVideo { get { return delimiterVideo; } }
        List<Channel> subscriptionChannels = new();
        public IEnumerable<Channel> Subscriptions { get { return subscriptionChannels; } }

        public SubVideoManager()
        {
            delimiterVideo = new DelimiterVideo(Program.cmdLineArgs.AfterVideoId);
        }

        public void Load()
        {
            delimiterVideo.Load();
            LoadSubscriptionChannels();
        }

        string GetCacheFilePath()
        {
            return Path.Combine("cache", "subscriptionChannels.json");
        }

        public void Store()
        {
            Directory.CreateDirectory("cache");

            delimiterVideo.Store();

            foreach (var channel in subscriptionChannels)
            {
                channel.Store();
            }

            List<Tuple<string, string>> channels = subscriptionChannels.Select(x => new Tuple<string, string>(x.Id, x.Title)).ToList();
            File.WriteAllText(GetCacheFilePath(), JsonSerializer.Serialize(channels));
        }

        void LoadSubscriptionChannels()
        {
            if (Program.cmdLineArgs.OnlyCache)
            {
                LoadSubscriptionChannelsFromCache();
            }
            else
            {
                if (Program.cmdLineArgs.NoCache)
                {
                    LoadSubscriptionChannelsFromYoutube();
                }
                else
                {
                    if (!LoadSubscriptionChannelsFromCache())
                    {
                        LoadSubscriptionChannelsFromYoutube();
                    }
                }
            }
        }

        bool LoadSubscriptionChannelsFromCache()
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

        void LoadSubscriptionChannelsFromYoutube()
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
    }
}
