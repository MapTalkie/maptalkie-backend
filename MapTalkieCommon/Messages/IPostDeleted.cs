using MapTalkieCommon.Utils;

namespace MapTalkieCommon.Messages
{
    public interface IPostDeleted
    {
        string PostId { get; }
        Location Location { get; }
    }
}