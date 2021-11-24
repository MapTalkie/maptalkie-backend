using System.Collections.Generic;
using MapTalkie.Common.Messages.Posts;

namespace MapTalkie.Services.LiveEventsConsumers.MessagesImpl
{
    public class GeoUpdateEvent : IGeoUpdate
    {
        public IList<GeoAreaUpdate> Updates { get; set; }
    }
}