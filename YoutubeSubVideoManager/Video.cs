using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace YoutubeSubVideoManager
{
    public class Video
    {
        public Video(string id, DateTime publishDate)
        {
            Id = id;
            PublishDate = publishDate;
        }

        public static Video FromFile(string filePath)
        {
            Video? result = JsonSerializer.Deserialize<Video>(File.ReadAllText(filePath));
            if (result == null)
            {
                throw new Exception("couldnt load video " + filePath);
            }
            return result;
        }

        public void ToFile(string filePath)
        {
            File.WriteAllText(filePath, JsonSerializer.Serialize<Video>(this));
        }

        public string Id { get; }
        public DateTime PublishDate { get; }
    }
}
