using System;
using IdGen;
using MapTalkie.DB.Context;
using MapTalkie.Tests.Unit.Fixtures;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Npgsql;

namespace MapTalkie.Tests.Unit;

public class DbTestsBase : TestsBase, IDisposable
{
    private readonly string _databaseName = "maptalkie_testing_" + Guid.NewGuid().ToString("n");

    public DbTestsBase(DbTemplateFixture databaseFixture)
    {
        var env = new Mock<IWebHostEnvironment>();

        env
            .Setup(e => e.EnvironmentName)
            .Returns(Environments.Development);

        ServiceCollection.AddSingleton(env.Object);
        ServiceCollection.AddSingleton(new IdGenerator(0));

        using (var tmplConnection = new NpgsqlConnection(databaseFixture.ConnectionString))
        {
            tmplConnection.Open();

            using (var cmd =
                   new NpgsqlCommand($"CREATE DATABASE {_databaseName} WITH TEMPLATE {databaseFixture.DatabaseName}",
                       tmplConnection))
            {
                cmd.ExecuteNonQuery();
            }
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                $"Host=localhost;Database={_databaseName};Username=admin;Password=admin",
                o => o.UseNetTopologySuite())
            .Options;
        Context = new AppDbContext(options, new NullLoggerFactory(), new IdGenerator(0));
        ServiceCollection.AddSingleton(Context);
    }

    public AppDbContext Context { get; }

    public void Dispose()
    {
        Context.Database.EnsureDeleted();
    }
}