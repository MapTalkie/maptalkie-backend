using System.Threading.Tasks;
using MapTalkie.Models.Context;
using MapTalkie.Services.FriendshipService;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace MaptalkieTests
{
    public class FriendshipTests : IdentityTestsBase
    {
        public FriendshipTests()
        {
            ServiceCollection.AddSingleton<IFriendshipService, FriendshipService>();
        }

        private IFriendshipService FriendshipService => ServiceProvider.GetRequiredService<IFriendshipService>();

        [Fact]
        public async Task UsersAreNotFriendsByDefault()
        {
            var id1 = UserIds[0];
            var id2 = UserIds[1];
            Assert.False(await FriendshipService.AreFriends(id1, id2));
        }

        [Fact]
        public async Task UsersAreNotFriendsIfOneRequestIsNotAccepted()
        {
            var id1 = UserIds[0];
            var id2 = UserIds[1];
            await FriendshipService.EnsureFriendshipRequest(id1, id2);
            Assert.False(await FriendshipService.AreFriends(id1, id2));
            await FriendshipService.EnsureFriendshipRequest(id2, id1);
            await FriendshipService.RevokeFriendship(id2, id1);
            Assert.False(await FriendshipService.AreFriends(id1, id2));
        }

        [Fact]
        public async Task UsersAreFriendsIfBothRequestsAccepted()
        {
            var id1 = UserIds[0];
            var id2 = UserIds[1];
            await FriendshipService.EnsureFriendshipRequest(id1, id2);
            await FriendshipService.EnsureFriendshipRequest(id2, id1);
            Assert.True(await FriendshipService.AreFriends(id1, id2));
        }

        [Fact]
        public async Task UserIsListedInFriendsListIfThereIsFriendship()
        {
            var id1 = UserIds[0];
            var id2 = UserIds[1];
            await FriendshipService.EnsureFriendshipRequest(id1, id2);
            await FriendshipService.EnsureFriendshipRequest(id2, id1);
            var requests = await ServiceProvider.GetRequiredService<AppDbContext>().FriendRequests.ToListAsync();
            Assert.Equal(2, requests.Count);
            Assert.Contains(
                await FriendshipService.GetFriends(id1),
                user => user.Id == id2);
            Assert.Contains(
                await FriendshipService.GetFriends(id2),
                user => user.Id == id1);

            await FriendshipService.RevokeFriendship(id2, id1);

            Assert.Empty(await FriendshipService.GetFriends(id1));
            Assert.Empty(await FriendshipService.GetFriends(id2));
        }
    }
}