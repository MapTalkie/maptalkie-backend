using System;
using NetTopologySuite.Geometries;

namespace MapTalkie.Domain.Messages.Posts
{
    public record PostCreated(
        DateTime CreatedAt,
        long PostId,
        string UserId,
        Point Location) : PostMessage(PostId, UserId, Location);
}