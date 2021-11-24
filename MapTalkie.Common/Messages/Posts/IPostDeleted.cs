using MapTalkie.Common.Utils;

namespace MapTalkie.Common.Messages.Posts
{
    public interface IPostDeleted
    {
        long PostId { get; }
        LocationDescriptor LocationDescriptor { get; }
    }
}