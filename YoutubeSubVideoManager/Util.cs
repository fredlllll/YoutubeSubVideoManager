using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSubVideoManager
{
    public static class Util
    {
        public static string ApplicationFolder { get; private set; }

        public static string GetApplicationFilePath(string subPath)
        {
            return Path.Join(ApplicationFolder, subPath);
        }

        static Util()
        {
            ApplicationFolder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YoutubeSubVideoManager");
            Directory.CreateDirectory(ApplicationFolder);
        }
    }
}
