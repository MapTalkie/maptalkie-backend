using MapTalkieCommon.Messages;

namespace MapTalkie.MessagesImpl
{
    public class PostEngagementEvent : IPostEngagement
    {
        public string PostId { get; set; }
        public string UserId { get; set; }
        public PostEngagementType Type { get; set; }
    }
}