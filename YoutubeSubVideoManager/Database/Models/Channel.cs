using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSubVideoManager.Database.Models
{
    public class Channel : Model
    {
        public required string Title { get; set; }
        public required ICollection<Video> Videos { get; set; }
    }
}
