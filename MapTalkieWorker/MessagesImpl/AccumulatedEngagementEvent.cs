using MapTalkieCommon.Messages;
using MapTalkieCommon.Utils;

namespace MapTalkieWorker.MessagesImpl
{
    public class AccumulatedEngagementEvent : IAccumulatedEngagementEvent
    {
        public int Likes { get; set; }
        public int Shares { get; set; }
        public int Comments { get; set; }
        public string PostId { get; set; }
        public Location Location { get; set; }
    }
}