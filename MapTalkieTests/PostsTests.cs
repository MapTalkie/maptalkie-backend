using System;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Services.PostService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Geometries;
using Xunit;

namespace MaptalkieTests
{
    public class PostsTests : IdentityTestsBase
    {
        public PostsTests()
        {
            ServiceCollection.AddScoped<IPostService, PostService>();
        }

        private IPostService PostService => ServiceProvider.GetRequiredService<IPostService>();

        [Fact]
        public async Task TestNewPost()
        {
            var post = await CreateSamplePost();

            Assert.True(post.Available);
            Assert.NotEqual(0, post.Id);
            Assert.Equal(UserIds[0], post.UserId);
        }

        [Fact]
        public async Task TestPostsInArea()
        {
            var posts = new MapPost[10];

            for (var i = 0; i < 10; i++)
                posts[i] = await CreateSamplePost();

            var poly = new Polygon(
                new LinearRing(
                    new[]
                    {
                        new Coordinate(80, 58),
                        new Coordinate(80, 50),
                        new Coordinate(84, 50),
                        new Coordinate(84, 58),
                        new Coordinate(80, 58),
                    }));
            var inArea = PostService.QueryPostsInArea(poly);
            Assert.Equal(10, await inArea.CountAsync());
        }

        private async Task<MapPost> CreateSamplePost()
        {
            return await PostService.CreateTextPost(
                $"This is test {Guid.NewGuid()}",
                UserIds[0],
                new Point(82.933952, 55.018803),
                true);
        }
    }
}