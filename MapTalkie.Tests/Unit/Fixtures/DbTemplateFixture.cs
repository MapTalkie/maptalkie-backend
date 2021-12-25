using System;
using IdGen;
using MapTalkie.DB.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace MapTalkie.Tests.Unit.Fixtures;

public class DbTemplateFixture : IDisposable
{
    private readonly AppDbContext _context;

    public DbTemplateFixture()
    {
        DatabaseName = $"maptalkie_testing_tmpl_{Guid.NewGuid():N}";
        ConnectionString = $"Host=localhost;Database={DatabaseName};Username=admin;Password=admin";
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(ConnectionString, x =>
        {
            x.MigrationsAssembly(typeof(Startup).Assembly.GetName().Name);
            x.UseNetTopologySuite();
        });
        _context = new AppDbContext(optionsBuilder.Options, new NullLoggerFactory(), new IdGenerator(0));
        _context.Database.Migrate();
    }

    public string ConnectionString { get; }
    public string DatabaseName { get; }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
    }
}

[CollectionDefinition(UnitTestsFixtures.Database)]
public class DbTemplateFixtureCollectionDefinition : ICollectionFixture<DbTemplateFixture>
{
}