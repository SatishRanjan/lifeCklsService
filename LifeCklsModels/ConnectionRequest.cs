using MongoDB.Bson;

namespace LifeCklsModels
{
    public class ConnectionRequest
    {
        public ObjectId Id { get; set; }
        public string? RequestId { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }
        // Possible values Accepted/Rejected/Ignored/Pending
        public string? RequestStaus { get; set; }
        public DateTime? RequestDateTimeUtc { get; set; }
        public string? FromUserFullName { get; set; }
        public DateTime? RequestOutcomeDateTimeUtc { get; set; }
    }
}
