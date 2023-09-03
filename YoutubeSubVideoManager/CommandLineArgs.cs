using CommandLine.Text;
using CommandLine;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSubVideoManager
{
    public class CommandLineArgs
    {
        [Option("after-video-id", Required = true, HelpText = "the video after which the videos are supposed to be opened")]
        public string AfterVideoId { get; set; } = "";

        [Option("video-count", Required = true, HelpText = "how many videos you want to open")]
        public int VideoCount { get; set; } = 50;

        [Option("only-cache", Required = false, HelpText = "if set, will only use local cache to open videos", SetName = "only-cache")]
        public bool OnlyCache { get; set; } = false;

        [Option("no-cache", Required = false, HelpText = "if set, will only use api", SetName = "no-cache")]
        public bool NoCache { get; set; } = false;

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
