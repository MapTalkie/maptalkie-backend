using MapTalkieCommon.Utils;

namespace MapTalkieCommon.Messages
{
    public interface IAccumulatedEngagementEvent
    {
        int Likes { get; }
        int Shares { get; }
        int Comments { get; }
        string PostId { get; }
        Location Location { get; }
    }
}