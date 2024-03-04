namespace LifeCklsModels
{
    public class ConnectionRequest
    {
        public string? RequestId { get; set; }
        public string FromUserName { get; set; }
        public string ToUserName { get; set; }        
        // Possible values Accepted/Ignored/Pending
        public string? RequestStaus { get; set; } 
        public DateTime? RequestDateTimeUtc { get; set; }
    }
}
