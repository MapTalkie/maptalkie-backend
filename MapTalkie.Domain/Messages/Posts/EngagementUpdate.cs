using MapTalkie.Domain.Utils.JsonConverters;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace MapTalkie.Domain.Messages.Posts
{
    public record EngagementUpdate
    {
        public int Likes { get; set; }
        public int Shares { get; set; }
        public int Comments { get; set; }
        public long PostId { get; set; }

        [JsonConverter(typeof(PointJsonConverter))]
        public Point Location { get; set; } = default!;
    }
}