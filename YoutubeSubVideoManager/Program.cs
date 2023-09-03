using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.YouTube.v3;
using Google.Apis.Util;
using CommandLine.Text;
using System.Diagnostics;
using System;

namespace YoutubeSubVideoManager
{
    internal class Program
    {
        public static YouTubeService youtubeService;
        public static CommandLineArgs cmdLineArgs;

        [STAThread]
        static void Main(string[] args)
        {
            var result = CommandLine.Parser.Default.ParseArguments<CommandLineArgs>(args);

            if (args.Length == 0)
            {
                var helpText = HelpText.AutoBuild(result, h => h, e => e);
                Console.WriteLine(helpText);
                return;
            }

            if (result.Errors.Any())
            {
                foreach (var err in result.Errors)
                {
                    Console.WriteLine(err);
                }
                return;
            }
            cmdLineArgs = result.Value;

            Task.WaitAll(Init());
            Run();
        }

        static async Task Init()
        {
            UserCredential credential;
            using (var stream = new FileStream("client_secret_674135367122-i1rt797i21937mroh8554rbrfeebjtnt.apps.googleusercontent.com.json", FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "iunno",
                    CancellationToken.None,
                    new FileDataStore(typeof(Program).ToString())
                );
            }

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = typeof(Program).ToString()
            });
        }

        static void Run()
        {
            SubVideoManager svm = new();

            svm.Load();
            svm.Store();

            //subscriptions and all their videos are now loaded
            //open x videos after certain video now

            //get videos after delimiter video
            IEnumerable<Video> subscriptionVideos = Enumerable.Empty<Video>();
            List<Video> videosAfterDelimiter = new();
            foreach (var channel in svm.Subscriptions)
            {
                foreach (var idAndVideo in channel.videos)
                {
                    if (idAndVideo.Value.PublishDate > svm.DelimiterVideo.Video.PublishDate)
                    {
                        videosAfterDelimiter.Add(idAndVideo.Value);
                    }
                }
            }

            //sort by date
            videosAfterDelimiter.Sort((x, y) =>
            {
                return x.PublishDate.CompareTo(y.PublishDate);
            });

            //oldest video should be first now
            int videosToOpen = Math.Min(cmdLineArgs.VideoCount, videosAfterDelimiter.Count);
            
            for (int i = 0; i < videosToOpen; i++)
            {
                var link = $"https://www.youtube.com/watch?v={videosAfterDelimiter[i].Id}";
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                };
                Process.Start(psi);
                Thread.Sleep(100); //to not completely overwhelm the browser
            }

            string lastOpenedVideoId = videosAfterDelimiter[videosToOpen - 1].Id;
            File.WriteAllText("lastOpenedVideoId.txt", lastOpenedVideoId);
        }

    }
}