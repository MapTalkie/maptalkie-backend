using MapTalkie.Domain.Utils.JsonConverters;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace MapTalkie.Domain.Messages.Posts
{
    public record PostMessage
    {
        public long PostId { get; set; }
        public string UserId { get; set; } = default!;

        [JsonConverter(typeof(PointJsonConverter))]
        public Point Location { get; set; } = default!;
    }
}