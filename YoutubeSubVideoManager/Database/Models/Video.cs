using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSubVideoManager.Database.Models
{
    public class Video : Model
    {
        public required DateTime PublishDate { get; set; }
        public required string Title { get; set; }
        public required Channel Channel { get; set; }
    }
}
