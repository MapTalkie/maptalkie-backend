using System.Threading.Tasks;
using MapTalkie.Services.FriendshipService;
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
            Assert.False(await FriendshipService.AreFriends(1, 2));
        }

        [Fact]
        public async Task UsersAreNotFriendsIfOneRequestIsNotAccepted()
        {
            await FriendshipService.EnsureFriendshipRequest(1, 2);
            Assert.False(await FriendshipService.AreFriends(1, 2));
            await FriendshipService.EnsureFriendshipRequest(2, 1);
            await FriendshipService.RevokeFriendship(2, 1);
            Assert.False(await FriendshipService.AreFriends(1, 2));
        }

        [Fact]
        public async Task UsersAreFriendsIfBothRequestsAccepted()
        {
            await FriendshipService.EnsureFriendshipRequest(1, 2);
            await FriendshipService.EnsureFriendshipRequest(2, 1);
            Assert.True(await FriendshipService.AreFriends(1, 2));
        }

        [Fact]
        public async Task UserIsListedInFriendsListIfThereIsFriendship()
        {
            await FriendshipService.EnsureFriendshipRequest(1, 2);
            await FriendshipService.EnsureFriendshipRequest(2, 1);
            Assert.Contains(
                await FriendshipService.GetFriends(1),
                user => user.Id == 2);
            Assert.Contains(
                await FriendshipService.GetFriends(2),
                user => user.Id == 1);

            await FriendshipService.RevokeFriendship(2, 1);

            Assert.Empty(await FriendshipService.GetFriends(1));
            Assert.Empty(await FriendshipService.GetFriends(2));
        }
    }
}