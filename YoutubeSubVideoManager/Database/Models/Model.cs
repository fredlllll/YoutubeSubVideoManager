using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeSubVideoManager.Database.Models
{
    public class Model
    {
        public required string Id { get; set; }
        public required DateTime Created { get; set; }
        public required DateTime Updated { get; set; }
    }
}
