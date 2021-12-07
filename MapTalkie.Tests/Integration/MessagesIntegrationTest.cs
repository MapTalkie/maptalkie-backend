using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Hubs;
using MapTalkie.Tests.Integration.Fixtures;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using Xunit;
using Xunit.Abstractions;

namespace MapTalkie.Tests.Integration
{
    [Collection(IntegrationTestsFixtures.Server)]
    public class MessagesIntegrationTest
    {
        private readonly TestServerFixture _serverFixture;
        private readonly ITestOutputHelper _testOutputHelper;

        public MessagesIntegrationTest(TestServerFixture fixture, ITestOutputHelper testOutputHelper)
        {
            _serverFixture = fixture;
            _testOutputHelper = testOutputHelper;
        }

        [Fact]
        public async Task SendMessage()
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
            dynamic? dmUpdate = null;
            hub.On(UserHub.DirectMessage, (dynamic v) => { dmUpdate = v; });

            var user = await GetBuddy();
            var response = await client.PostAsJsonAsync("/api/messages/pm/" + user.Id, new { text = "Hello!" });
            response.EnsureSuccessStatusCode();
            response = await client.GetAsync("/api/messages/pm");
            var items = await response.Content.ReadFromJsonAsync<object[]>();
            Assert.NotNull(items);
            Assert.Single(items!);

            response = await client.GetAsync("/api/messages/pm/" + user.Id);
            items = await response.Content.ReadFromJsonAsync<object[]>();
            Assert.NotNull(items);
            Assert.Single(items!);

            await Task.Delay(1000);
            Assert.NotNull(dmUpdate);
        }

        [Fact]
        public async Task DeleteMessage()
        {
            var client = await CreateAuthorizedClient();
            var user = await GetBuddy();
            await client.PostAsJsonAsync("/api/messages/pm/" + user.Id, new { text = "Hello!" });
            var response = await client.GetAsync("/api/messages/pm/" + user.Id);
            var messages = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>[]>();
            Assert.NotNull(messages);
            Assert.Single(messages!);
            var message = messages![0];
            Assert.True(message.ContainsKey("id"));
            Assert.Equal(JsonValueKind.String, message["id"].ValueKind);
            var id = message["id"].GetString()!;
            await client.DeleteAsync("/api/messages/pm/" + user.Id + "/" + id);
            response = await client.GetAsync("/api/messages/pm/" + user.Id);
            messages = await response.Content.ReadFromJsonAsync<Dictionary<string, JsonElement>[]>();
            Assert.Empty(messages!);
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

        private async Task<User> GetBuddy()
        {
            var context = _serverFixture.Server.Services.GetRequiredService<AppDbContext>();
            var user = new User
            {
                UserName = "Buddy" + Guid.NewGuid().ToString("N") // в данном случае нам не нужен пароль
            };
            context.Add(user);
            await context.SaveChangesAsync();
            return user;
        }
    }
}