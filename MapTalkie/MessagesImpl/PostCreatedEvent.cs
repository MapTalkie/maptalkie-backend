using MapTalkieCommon.Messages;
using MapTalkieCommon.Utils;

namespace MapTalkie.MessagesImpl
{
    public class PostCreatedEvent : IPostCreated
    {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public Location Location { get; set; }
    }
}