using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.Services.PostService;
using MapTalkieDB;
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
            Assert.Equal(UserIds[0], post.UserId);
        }

        [Fact]
        public async Task TestPostsInArea()
        {
            var posts = new Post[10];

            for (var i = 0; i < 10; i++)
                posts[i] = await CreateSamplePost();

            var poly = new Polygon(
                new LinearRing(
                    new[]
                    {
                        new Coordinate(8905559.263461877, 7967317.535015895),
                        new Coordinate(7967317.535015895, 6446275.841017148),
                        new Coordinate(9350837.22663497, 6446275.841017148),
                        new Coordinate(9350837.22663497, 7967317.535015895),
                        new Coordinate(8905559.263461877, 7967317.535015895)
                    })) { SRID = 3857 };
            var inArea = PostService.QueryPosts(poly);
            Assert.Equal(10, await inArea.CountAsync());
        }

        private async Task<Post> CreateSamplePost()
        {
            return await PostService.CreateTextPost(
                $"This is test {Guid.NewGuid()}",
                UserIds[0],
                new Point(82.933952, 55.018803) { SRID = 4326 },
                true);
        }

        [Fact]
        public async Task TestPopularPostsTimeDecay()
        {
            var random = new Random();
            var newestPosts = new List<string>();
            {
                var old = DateTime.UtcNow - TimeSpan.FromDays(3);

                for (var i = 0; i < 20; i++)
                {
                    var postTime = i >= 10 ? DateTime.Now : old + TimeSpan.FromDays(random.NextDouble());
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
                        Comments = comments
                    };
                    if (i >= 10)
                        newestPosts.Add(post.Id);

                    Context.Add(post);
                }

                await Context.SaveChangesAsync();
            }

            var popularPosts = await PostService.QueryPopularPosts().ToListAsync();
            Assert.DoesNotContain(popularPosts, p => !newestPosts.Contains(p.Id));
        }

        [Fact]
        public async Task TestPopularPostsLikePriority()
        {
            var popularIds = new List<string>();
            {
                for (var i = 0; i < 20; i++)
                {
                    var post = new Post
                    {
                        UserId = UserIds[0],
                        Text = "Hello!"
                    };

                    Context.Add(post);
                }

                for (var i = 0; i < 20; i++)
                {
                    PostComment[] comments;
                    PostLike[] likes;

                    if (i < 10)
                    {
                        comments = new PostComment[10];

                        for (var j = 0; j < 10; j++)
                            comments[j] = new PostComment
                            {
                                SenderId = UserIds[1]
                            };
                        likes = Array.Empty<PostLike>();
                    }
                    else
                    {
                        likes = new PostLike[10];

                        for (var j = 0; j < 10; j++)
                            likes[j] = new PostLike
                            {
                                UserId = UserIds[1],
                                PostId = j
                            };
                        comments = Array.Empty<PostComment>();
                    }

                    await Context.AddRangeAsync(likes);
                    await Context.AddRangeAsync(comments);
                }

                await Context.SaveChangesAsync();
            }

            var popularPosts = await PostService.QueryPopularPosts().ToListAsync();
            Assert.DoesNotContain(popularPosts, p => p.Id < 10);
        }
    }
}