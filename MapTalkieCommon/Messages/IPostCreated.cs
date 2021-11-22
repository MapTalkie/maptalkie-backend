using MapTalkieCommon.Utils;

namespace MapTalkieCommon.Messages
{
    public interface IPostCreated
    {
        string PostId { get; }
        string UserId { get; }
        Location Location { get; }
    }
}