using MapTalkie.Common.Messages.Posts;
using MapTalkie.Common.Utils;

namespace MapTalkie.MessagesImpl
{
    public class PostDeletedEvent : IPostDeleted
    {
        public long PostId { get; set; }
        public LocationDescriptor LocationDescriptor { get; set; }
    }
}