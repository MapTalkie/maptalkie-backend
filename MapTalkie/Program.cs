using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MapTalkie
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? Environments.Development;
            var configurationBuilder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{env}.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            var letsEncryptDirectory = Environment.GetEnvironmentVariable("LETSENCRYPT_CERT_DIRECTORY");

            if (letsEncryptDirectory != null)
            {
                var certPath = Path.Join(letsEncryptDirectory, "fullchain.pem");
                var keyPath = Path.Join(letsEncryptDirectory, "privkey.pem");
                if (File.Exists(keyPath) && File.Exists(certPath))
                {
                    configurationBuilder.AddInMemoryCollection(new List<KeyValuePair<string, string>>
                    {
                        new("Kestrel:Certificates:Default:Path", certPath),
                        new("Kestrel:Certificates:Default:KeyPath", keyPath),
                    });
                }
                else
                {
                    throw new FileNotFoundException(
                        "Failed to find Let's encrypt certificates, you can still use configuration variables" +
                        " Kestrel:Certificates:Default:Path and Kestrel:Certificates:Default:KeyPath to set the path " +
                        "to your certificate");
                }
            }

            var configuration = configurationBuilder.Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging => { logging.AddConsole(); })
                .ConfigureAppConfiguration((context, cfg) =>
                {
                    context.HostingEnvironment.EnvironmentName = env;
                    cfg.Sources.Clear();
                    cfg.AddConfiguration(configuration);
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseKestrel(options => { options.Configure(configuration.GetSection("Kestrel")); });
                });
        }
    }
}