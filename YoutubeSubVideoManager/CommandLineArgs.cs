using CommandLine.Text;
using CommandLine;

namespace YoutubeSubVideoManager
{
    public class CommandLineArgs
    {
        [Option("after-video-id", Required = true, HelpText = "the video after which the videos are supposed to be opened")]
        public string AfterVideoId { get; set; } = "";

        [Option("video-count", Required = true, HelpText = "how many videos you want to open", ResourceType = typeof(int), Min = 0)]
        public int VideoCount { get; set; } = 50;

        [Option("opening-interval", Required = false, HelpText = "how many ms are between each opening of a video link", ResourceType = typeof(int), Min = 1, Default = 1000)]
        public int OpeningInterval { get; set; } = 1000;

        [Option("drop-cache", Required = false, HelpText = "if set, will only use api", SetName = "drop-cache")]
        public bool DropCache { get; set; } = false;

        [Usage(ApplicationAlias = "YoutubeSubVideoManager")]
        public static IEnumerable<Example> Examples
        {
            get
            {
                return new List<Example>() {
                    new Example("open 50 videos after video id 55555", new CommandLineArgs { AfterVideoId = "55555", VideoCount = 50})
                };
            }
        }
    }
}
