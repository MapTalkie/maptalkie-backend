using System.Threading.Tasks;
using MapTalkie.DB.Context;
using MapTalkie.Services.FriendshipService;
using MapTalkie.Tests.Unit.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MapTalkie.Tests.Unit;

[Collection(UnitTestsFixtures.Database)]
public class FriendshipTests : IdentityTestsBase
{
    public FriendshipTests(DbTemplateFixture dbTemplateFixture) : base(dbTemplateFixture)
    {
        ServiceCollection.AddSingleton<IFriendshipService, FriendshipService>();
    }

    private IFriendshipService FriendshipService => ServiceProvider.GetRequiredService<IFriendshipService>();

    [Fact]
    public async Task UsersAreNotFriendsByDefault()
    {
        await PopulateUsers();
        var id1 = UserIds[0];
        var id2 = UserIds[1];
        Assert.Equal(FriendshipState.None, await FriendshipService.GetFriendshipState(id1, id2));
    }

    [Fact]
    public async Task UsersAreNotFriendsIfOneRequestIsNotAccepted()
    {
        await PopulateUsers();
        var id1 = UserIds[0];
        var id2 = UserIds[1];
        await FriendshipService.EnsureFriendshipRequest(id1, id2);
        Assert.Equal(FriendshipState.RequestPending, await FriendshipService.GetFriendshipState(id1, id2));
        await FriendshipService.EnsureFriendshipRequest(id2, id1);
        await FriendshipService.RevokeFriendship(id2, id1);
        Assert.Equal(FriendshipState.RequestPending, await FriendshipService.GetFriendshipState(id1, id2));
    }

    [Fact]
    public async Task UsersAreFriendsIfBothRequestsAccepted()
    {
        await PopulateUsers();
        var id1 = UserIds[0];
        var id2 = UserIds[1];
        await FriendshipService.EnsureFriendshipRequest(id1, id2);
        await FriendshipService.EnsureFriendshipRequest(id2, id1);
        Assert.Equal(FriendshipState.Mutual, await FriendshipService.GetFriendshipState(id1, id2));
    }

    [Fact]
    public async Task UserIsListedInFriendsListIfThereIsFriendship()
    {
        await PopulateUsers();
        var id1 = UserIds[0];
        var id2 = UserIds[1];
        await FriendshipService.EnsureFriendshipRequest(id1, id2);
        await FriendshipService.EnsureFriendshipRequest(id2, id1);
        var requests = await ServiceProvider.GetRequiredService<AppDbContext>().FriendRequests.ToListAsync();
        Assert.Equal(2, requests.Count);
        Assert.Contains(
            (await FriendshipService.FindFriendships(id1)).Friends,
            user => user.UserId == id2);
        Assert.Contains(
            (await FriendshipService.FindFriendships(id2)).Friends,
            user => user.UserId == id1);

        await FriendshipService.RevokeFriendship(id2, id1);

        Assert.Empty((await FriendshipService.FindFriendships(id1)).Friends);
        Assert.Empty((await FriendshipService.FindFriendships(id2)).Friends);
    }
}