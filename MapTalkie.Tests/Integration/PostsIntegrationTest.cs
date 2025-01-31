using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MapTalkie.Hubs;
using MapTalkie.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Net.Http.Headers;
using Xunit;
using Xunit.Abstractions;

namespace MapTalkie.Tests.Integration;

[Collection(IntegrationTestsFixtures.Server)]
public class PostsIntegrationTest
{
    private readonly TestServerFixture _serverFixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public PostsIntegrationTest(TestServerFixture fixture, ITestOutputHelper testOutputHelper)
    {
        _serverFixture = fixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task CreatePost()
    {
        var client = await CreateAuthorizedClient();
        var token = client.DefaultRequestHeaders.Authorization!.Parameter!;
        var hub = new HubConnectionBuilder()
            .WithUrl(new Uri(_serverFixture.Server.BaseAddress, "_signalr/user"), o =>
            {
                o.AccessTokenProvider = () => Task.FromResult(token)!;
                o.HttpMessageHandlerFactory = _ => _serverFixture.Server.CreateHandler();
            })
            .WithAutomaticReconnect()
            .Build();

        await hub.StartAsync();
        dynamic? postsUpdate = null;
        hub.On(UserHub.PostsUpdate, (dynamic v) => { postsUpdate = v; });

        await hub.SendAsync(nameof(UserHub.ConfigureSubscription),
            new { lat = 56.018803, lon = 83.933952 },
            new { lat = 54.018803, lon = 81.933952 });
        var response = await client.PostAsJsonAsync("/api/posts", new
        {
            text = "yoooooo this is a test yoooo",
            location = new { lat = 55.018803, lon = 82.933952 }
        });
        if (response.StatusCode != HttpStatusCode.Created)
            _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());

        response.EnsureSuccessStatusCode();
        await Task.Delay(3000);
        Assert.NotNull(postsUpdate);

        // изменим на другую область 
        postsUpdate = null;
        await hub.SendAsync(nameof(UserHub.ConfigureSubscription),
            new { lat = 26.018803, lon = 23.933952 },
            new { lat = 24.018803, lon = 21.933952 });
        await Task.Delay(2000);
        response = await client.PostAsJsonAsync("/api/posts", new
        {
            text = "this is not supposed to be here",
            location = new { lat = 55.018803, lon = 81.933952 }
        });
        if (response.StatusCode != HttpStatusCode.Created)
            _testOutputHelper.WriteLine(await response.Content.ReadAsStringAsync());

        response.EnsureSuccessStatusCode();
        await Task.Delay(2000);
        Assert.Null(postsUpdate);
    }

    [Fact]
    public async Task LikePost()
    {
        var client = await CreateAuthorizedClient();
        var response = await client.PostAsJsonAsync("/api/posts", new
        {
            text = "yoooooo this is a test yoooo",
            location = new { lat = 55.018803, lon = 82.933952 }
        });
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<IdResponse>();
        response = await client.PutAsync("/api/posts/" + data!.Id + "/like", null);
        response.EnsureSuccessStatusCode();
        response = await client.GetAsync("/api/posts/" + data.Id);
        var postView = await response.Content.ReadFromJsonAsync<PostViewLike>();
        Assert.Equal(data.Id, postView!.Id);
        Assert.Equal(1, postView.Likes);

        response = await client.DeleteAsync("/api/posts/" + data.Id + "/like");
        response = await client.GetAsync("/api/posts/" + data.Id);
        postView = await response.Content.ReadFromJsonAsync<PostViewLike>();
        Assert.Equal(0, postView!.Likes);
    }

    private async Task<HttpClient> CreateAuthorizedClient()
    {
        var client = _serverFixture.Server.CreateClient();
        var response = await client.PostAsync("/api/auth/signin", JsonContent.Create(new
        {
            userName = _serverFixture.TestUsername,
            password = _serverFixture.TestPassword
        }));
        response.EnsureSuccessStatusCode();
        var data = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(data);
        Assert.NotNull(data!["token"]);
        client.DefaultRequestHeaders.Add(HeaderNames.Authorization, $"Bearer {data["token"]}");
        return client;
    }

    private class IdResponse
    {
        public string Id { get; set; }
    }

    private class PostViewLike
    {
        public string Id { get; set; }
        public int Likes { get; set; }
    }

    private class PostsUpdate
    {
        public List<PostUpdate> Posts { get; set; }

        public class LatLon
        {
            public double Lat { get; set; }
            public double Lon { get; set; }
        }

        public class PostUpdate
        {
            public long Id { get; set; }
            public string UserId { get; set; }
            public LatLon Location { get; set; }
        }
    }
}