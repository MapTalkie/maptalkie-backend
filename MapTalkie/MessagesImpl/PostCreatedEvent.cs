using MapTalkie.Common.Messages.Posts;
using MapTalkie.Common.Utils;

namespace MapTalkie.MessagesImpl
{
    public class PostCreatedEvent : IPostCreated
    {
        public long PostId { get; set; }
        public string UserId { get; set; }
        public LocationDescriptor Location { get; set; }
    }
}