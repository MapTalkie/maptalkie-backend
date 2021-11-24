using MapTalkie.Common.Utils;

namespace MapTalkie.Common.Messages
{
    public interface IAccumulatedEngagementEvent
    {
        int Likes { get; }
        int Shares { get; }
        int Comments { get; }
        long PostId { get; }
        LocationDescriptor LocationDescriptor { get; }
    }
}