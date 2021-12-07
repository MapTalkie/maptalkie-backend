using System;
using MapTalkie.Domain.Utils.JsonConverters;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace MapTalkie.Domain.Messages.Posts
{
    public record PostCreated(
        DateTime CreatedAt,
        long PostId,
        string UserId,
        [property: JsonConverter(typeof(PointJsonConverter))]
        Point Location);
}