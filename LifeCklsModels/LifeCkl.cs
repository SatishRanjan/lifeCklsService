using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCklsModels
{
    public class LifeCkl
    {
        public string ProfileId { get; set; }
        public List<string> Stories { get; set; }
        public List<string> Connections { get; set; }
        public List<string> Invites { get; set; }
        public List<string> Messages { get; set; }
    }
}
