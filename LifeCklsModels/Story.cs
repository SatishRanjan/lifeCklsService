using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCklsModels
{
    public class Story
    {
        public string StoryId { get; set; }
        public string From { get; set;}
        public string Content { get; set;}
        public string Where { get; set; }
        public DateTime When { get; set; }
        public List<string> Participants { get; set; }
    }
}
