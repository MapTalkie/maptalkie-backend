using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.Domain.Popularity;
using MapTalkie.Domain.Utils;
using MapTalkie.Tests.Unit.Fixtures;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Xunit;

namespace MapTalkie.Tests.Unit;

[Collection(UnitTestsFixtures.Database)]
public class PostsTests : IdentityTestsBase
{
    public PostsTests(DbTemplateFixture databaseFixture) : base(databaseFixture)
    {
    }

    [Fact]
    public async Task TestNewPost()
    {
        await PopulateUsers();
        var post = await CreateSamplePost();

        Assert.True(post.Available);
        Assert.Equal(UserIds[0], post.UserId);
    }

    [Fact]
    public async Task TestPostsInArea()
    {
        await PopulateUsers();
        var posts = new Post[10];

        for (var i = 0; i < 10; i++)
            posts[i] = await CreateSamplePost();

        var poly = new Polygon(
            new LinearRing(
                new[]
                {
                    new Coordinate(80.933952, 50.018803),
                    new Coordinate(80.933952, 56.018803),
                    new Coordinate(83.933952, 56.018803),
                    new Coordinate(83.933952, 50.018803),
                    new Coordinate(80.933952, 50.018803)
                })) { SRID = 4326 };
        poly = MapConvert.ToMercator(poly);
        var inArea = Context.Posts.Where(p => poly.Contains(p.Location));
        var allPosts = await Context.Posts.ToListAsync();
        Assert.Equal(10, await inArea.CountAsync());
    }

    private async Task<Post> CreateSamplePost()
    {
        var post = new Post
        {
            Location = MapConvert.ToMercator(new Point(82.933952, 55.018803) { SRID = 4326 }),
            Text = $"This is test {Guid.NewGuid()}",
            UserId = UserIds[0]
        };
        Context.Add(post);
        await Context.SaveChangesAsync();
        return post;
    }

    [Fact]
    public async Task TestPopularPostsTimeDecay()
    {
        await PopulateUsers();
        var random = new Random();
        var newestPosts = new List<Post>();
        {
            var old = DateTime.UtcNow - TimeSpan.FromDays(3);

            for (var i = 0; i < 20; i++)
            {
                var postTime = i >= 10 ? DateTime.UtcNow : old + TimeSpan.FromDays(random.NextDouble());
                var comments = new PostComment[10];

                for (var j = 0; j < 10; j++)
                    comments[j] = new PostComment
                    {
                        CreatedAt = postTime,
                        SenderId = UserIds[1]
                    };

                var post = new Post
                {
                    UserId = UserIds[0],
                    Text = "Hello!",
                    CreatedAt = postTime,
                    Comments = comments,
                    Location = new Point(0, 0) { SRID = 3857 }
                };
                if (i >= 10)
                    newestPosts.Add(post);

                Context.Add(post);
            }

            await Context.SaveChangesAsync();
        }

        await Context.Database.ExecuteSqlInterpolatedAsync(
            $"call update_exp_ranking_decay({Popularity.MinDecay}, {Popularity.DecayCoefficient});");

        var newestIds = newestPosts.Select(p => p.Id);
        var popularPosts = await Context.Posts.OrderByDescending(Popularity.PopularityRankProjection).Take(10)
            .ToListAsync();
        Assert.DoesNotContain(popularPosts, p => !newestIds.Contains(p.Id));
    }
}