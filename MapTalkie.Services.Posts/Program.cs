using MapTalkie.DB.Context;
using MapTalkie.Services.Posts.Consumers.PostLikedConsumer;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MapTalkie.Services.Posts
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddJsonFile("appsettings.json", true);
                    config.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                    config.AddEnvironmentVariables();
                    if (args != null)
                        config.AddCommandLine(args);
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseNpgsql(context.Configuration.GetConnectionString("Postgres"),
                            x => x.UseNetTopologySuite());
                    });

                    services.AddMassTransit(x =>
                    {
                        x.AddConsumer<PostLikedConsumer>(typeof(PostLikedConsumerDefinition));

                        x.UsingRabbitMq((busContext, cfg) => { cfg.ConfigureEndpoints(busContext); });
                    });
                    services.AddHostedService<MassTransitConsoleHostedService>();
                });


            var host = builder.Build();
            host.Run();
        }
    }
}