using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using MapTalkie.Tests.Integration.Fixtures;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace MapTalkie.Tests.Integration;

[Collection(IntegrationTestsFixtures.Server)]
public class AuthIntegrationTest
{
    private const string PASSWORD = "uga-chaka01$ABC";
    private readonly TestServerFixture _serverFixture;

    public AuthIntegrationTest(TestServerFixture server)
    {
        _serverFixture = server;
    }

    [Fact]
    public async Task TestLoggingInWithWrongCredentials()
    {
        var client = _serverFixture.Server.CreateClient();
        var response = await client.PostAsync("/api/auth/signin", JsonContent.Create(new
        {
            userName = "IDoNotExist",
            password = PASSWORD
        }));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Test_signing_up_and_signing_in()
    {
        var client = _serverFixture.Server.CreateClient();
        var userName = await CreateRandomUser(client);

        // обычный вход в систему
        var response = await client.PostAsync("/api/auth/signin", JsonContent.Create(new
        {
            userName,
            password = PASSWORD
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var data = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(data);
        Assert.Contains("token", data!.Keys);
        Assert.Contains("refreshToken", data!.Keys);
        Assert.NotNull(data["refreshToken"]);
        Assert.NotNull(data["token"]);

        // гибридный вход
        response = await client.PostAsync("/api/auth/hybrid-signin", JsonContent.Create(new
        {
            userName,
            password = PASSWORD
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        data = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(data);
        Assert.Contains("token", data!.Keys);
        Assert.Contains("refreshToken", data!.Keys);
        Assert.Null(data["refreshToken"]);
        Assert.NotNull(data["token"]);
        var cookies = response.Headers.GetValues("Set-Cookie").ToList();
        Assert.Equal(2, cookies.Count);
    }

    private static async Task<string> CreateRandomUser(HttpClient client)
    {
        var userNames = new[] { "TheGuy", "Bob", "PizzaDeliveryGuy", "JeffBezos" };
        var userName = userNames[new Random().Next() % userNames.Length] + "_" + Guid.NewGuid().ToString("N");
        var response = await client.PostAsync("/api/auth/signup", JsonContent.Create(new
        {
            userName,
            password = PASSWORD,
            email = "mail@mail.org"
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return userName;
    }

    [Fact]
    public async Task Test_refreshing_token_in_hybrid()
    {
        var client = _serverFixture.Server.CreateClient();
        var userName = await CreateRandomUser(client);
        var response = await client.PostAsync("/api/auth/hybrid-signin", JsonContent.Create(new
        {
            userName,
            password = PASSWORD
        }));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var cookies = response.Headers.GetValues("Set-Cookie")
            .Select(v => SetCookieHeaderValue.Parse(v))
            .Select(c => new CookieHeaderValue(c.Name, c.Value));
        response = await _serverFixture.Server
            .CreateRequest("/api/auth/hybrid-refresh")
            .AddHeader(HeaderNames.Cookie, string.Join(";", cookies))
            .PostAsync();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var data = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
        Assert.NotNull(data);
        Assert.Contains("token", data!.Keys);
        Assert.Contains("refreshToken", data!.Keys);
        Assert.Null(data["refreshToken"]);
        Assert.NotNull(data["token"]);
    }
}