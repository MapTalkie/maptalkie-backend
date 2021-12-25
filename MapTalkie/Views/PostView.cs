using System;
using System.Linq.Expressions;
using MapTalkie.DB;
using MapTalkie.Domain.Utils;
using NetTopologySuite.Geometries;

namespace MapTalkie.Views;

public record PostView(
    long Id,
    string UserId,
    string UserName,
    DateTime CreatedAt,
    int Likes,
    int Comments,
    int Shares,
    string Text,
    bool IsOriginalLocation,
    Point Location)
{
    public static Expression<Func<Post, PostView>> Projection => post => new PostView(
        post.Id, post.UserId, post.User.UserName, post.CreatedAt, post.CachedLikesCount, post.CachedCommentsCount,
        post.CachedSharesCount, post.Text, post.IsOriginalLocation, MapConvert.ToLatLon(post.Location));
}