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
        public static string GetApplicationFilePath(string subPath)
        {
            var folder = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "YoutubeSubVideoManager");
            Directory.CreateDirectory(folder);
            return Path.Join(folder, subPath);
        }
    }
}
