using MapTalkie.Common.Messages;
using MapTalkie.Common.Utils;

namespace MapTalkie.Services.LiveEventsConsumers.MessagesImpl
{
    public class AccumulatedEngagementEvent : IAccumulatedEngagementEvent
    {
        public int Likes { get; set; }
        public int Shares { get; set; }
        public int Comments { get; set; }
        public long PostId { get; set; }
        public LocationDescriptor LocationDescriptor { get; set; }
    }
}