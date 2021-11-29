using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace MapTalkie.Services.Posts
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
            var host = builder.Build();
            host.Run();
        }
    }
}