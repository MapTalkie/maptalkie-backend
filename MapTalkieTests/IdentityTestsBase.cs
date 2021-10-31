using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace MaptalkieTests
{
    public class IdentityTestsBase : DbTestsBase
    {
        protected static readonly string PASSWORD = "Password1$_";
        protected readonly UserManager<User> userManager;

        public IdentityTestsBase()
        {
            ServiceCollection.AddLogging();
            ServiceCollection
                .AddIdentity<User, Role>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            userManager = ServiceProvider.GetService<UserManager<User>>();

            PopulateUsers();
        }

        private void PopulateUsers()
        {
            var tasks = new List<Task>();
            for (var i = 1; i <= 5; i++)
                tasks.Add(
                    userManager.CreateAsync(new User
                    {
                        UserName = $"User{i}"
                    }, PASSWORD));

            Task.WaitAll(tasks.ToArray());
        }
    }
}