using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using CommandLine.Text;
using System.Diagnostics;
using YoutubeSubVideoManager.Database.Models;
using YoutubeSubVideoManager.Database;
using Microsoft.EntityFrameworkCore;
using Google.Apis.Util;

namespace YoutubeSubVideoManager
{
    internal class Program
    {
        public static YouTubeService? youtubeService;
        public static CommandLineArgs? cmdLineArgs;

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
            string secretsFile = Util.GetApplicationFilePath("client_secrets.json");
            if (!File.Exists(secretsFile))
            {
                throw new Exception($"{secretsFile} not found");
            }
            using (var stream = new FileStream(secretsFile, FileMode.Open, FileAccess.Read))
            {
                var googleClientSecrets = GoogleClientSecrets.FromStream(stream);
                if (googleClientSecrets == null)
                {
                    throw new Exception($"Could not load secrets from {secretsFile}");
                }
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    googleClientSecrets.Secrets,
                    // This OAuth 2.0 access scope allows for full read/write access to the
                    // authenticated user's account.
                    new[] { YouTubeService.Scope.Youtube },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(Util.ApplicationFolder)
                );
            }

            youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = typeof(Program).ToString()
            });
        }

        static void CreateCache(DatabaseContext db)
        {
            if (youtubeService == null)
            {
                throw new InvalidOperationException("youtube service is null");
            }
            Repeatable<string> part = new(["snippet"]);

            var listSubs = youtubeService.Subscriptions.List(part);
            listSubs.MaxResults = 1000;
            listSubs.Mine = true;
            while (true)
            {
                var subResponse = listSubs.Execute();
                foreach (var sub in subResponse.Items)
                {
                    db.Add(new Channel()
                    {
                        Id = sub.Snippet.ResourceId.ChannelId,
                        Created = DateTime.Now,
                        Updated = DateTime.Now,
                        Title = sub.Snippet.Title,
                        Videos = new List<Video>()
                    });
                }
                if (subResponse.NextPageToken == null)
                {
                    break;
                }
                listSubs.PageToken = subResponse.NextPageToken;
            }
            db.SaveChanges();
            foreach (var channel in db.Channels)
            {
                channel.LoadVideosFromYoutube(db);
            }
        }

        static void Run()
        {
            if (cmdLineArgs == null)
            {
                throw new InvalidOperationException("command line args is null");
            }
            if (youtubeService == null)
            {
                throw new InvalidOperationException("youtube service is null");
            }

            var db = DatabaseContext.Instance;
            db.Database.Migrate();

            if (cmdLineArgs.DropCache)
            {
                db.Videos.ExecuteDelete();
                db.Channels.ExecuteDelete();
            }

            if (db.Channels.Count() == 0)
            {
                CreateCache(db);
            }

            Video delimiterVideo = db.Videos.Where(video => video.Id == cmdLineArgs.AfterVideoId).First();

            var videosAfterDelimiter = db.Videos.Where(video => video.PublishDate > delimiterVideo.PublishDate).OrderBy(video => video.PublishDate);

            string lastOpenedVideoId = "";
            foreach (var video in videosAfterDelimiter.Take(cmdLineArgs.VideoCount))
            {
                Console.WriteLine($"Opening Video {video.Id}({video.Title})");
                var link = $"https://www.youtube.com/watch?v={video.Id}";

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = link,
                    UseShellExecute = true
                };
                Process.Start(psi);
                lastOpenedVideoId = video.Id;
                Thread.Sleep(cmdLineArgs.OpeningInterval);//dont overload browser
            }

            if (lastOpenedVideoId.Length > 0)
            {
                File.WriteAllText(Util.GetApplicationFilePath("lastOpenedVideoId.txt"), lastOpenedVideoId);
            }
        }

    }
}