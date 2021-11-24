using MapTalkie.Common.Utils;

namespace MapTalkie.Common.Messages.Posts
{
    public interface IPostCreated
    {
        long PostId { get; }
        string UserId { get; }
        LocationDescriptor Location { get; }
    }
}