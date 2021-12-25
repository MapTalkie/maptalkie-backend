using MapTalkie.Domain.Utils.JsonConverters;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace MapTalkie.Domain.Messages.Posts;

public record PostDeleted(
    long PostId,
    string UserId,
    [property: JsonConverter(typeof(PointJsonConverter))]
    Point Location);