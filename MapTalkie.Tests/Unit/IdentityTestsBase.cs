using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.DB;
using MapTalkie.DB.Context;
using MapTalkie.Tests.Unit.Fixtures;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MapTalkie.Tests.Unit
{
    public class IdentityTestsBase : DbTestsBase
    {
        protected static readonly string PASSWORD = "Password1$_";
        protected readonly List<string> UserIds = new();

        public IdentityTestsBase(DbTemplateFixture databaseFixture) : base(databaseFixture)
        {
            ServiceCollection
                .AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
        }

        protected async Task PopulateUsers()
        {
            var userManager = ServiceProvider.GetRequiredService<UserManager<User>>();
            for (var i = 1; i <= 5; i++)
            {
                var user = new User
                {
                    UserName = $"User{i}"
                };
                await userManager.CreateAsync(user, PASSWORD);
                UserIds.Add(user.Id);
            }
        }
    }
}