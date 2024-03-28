using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LifeCklsModels
{
    public class ConnectionRequestResult
    {
        public string? RequestId { get; set; }
        // Possible values Accepted/Rejected/Ignored
        public string? ConnectionRequestOutcome { get; set; }
    }
}
