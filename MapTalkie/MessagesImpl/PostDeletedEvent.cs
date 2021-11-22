using MapTalkieCommon.Messages;
using MapTalkieCommon.Utils;

namespace MapTalkie.MessagesImpl
{
    public class PostDeletedEvent : IPostDeleted
    {
        public string PostId { get; set; }
        public Location Location { get; set; }
    }
}