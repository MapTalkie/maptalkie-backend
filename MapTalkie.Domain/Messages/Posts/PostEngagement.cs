using MapTalkie.Domain.Utils.JsonConverters;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace MapTalkie.Domain.Messages.Posts;

public record PostEngagement(
    long PostId,
    string UserId,
    [property: JsonConverter(typeof(PointJsonConverter))]
    Point Location,
    PostEngagementType Type);

public enum PostEngagementType
{
    Favorite,
    FavoriteRemoved,
    Share,
    ShareRemoved,
    Comment,
    CommentRemoved
}