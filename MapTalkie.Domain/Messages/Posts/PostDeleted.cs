using NetTopologySuite.Geometries;

namespace MapTalkie.Domain.Messages.Posts
{
    public record PostDeleted(long PostId, string UserId, Point Location) : PostMessage(PostId, UserId, Location);
}