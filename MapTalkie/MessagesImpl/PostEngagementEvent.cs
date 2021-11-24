using MapTalkie.Common.Messages.Posts;

namespace MapTalkie.MessagesImpl
{
    public class PostEngagementEvent : IPostEngagement
    {
        public long PostId { get; set; }
        public string UserId { get; set; }
        public PostEngagementType Type { get; set; }
    }
}