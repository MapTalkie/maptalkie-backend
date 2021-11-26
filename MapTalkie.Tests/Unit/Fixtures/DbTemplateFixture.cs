using System;
using MapTalkie.DB.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace MapTalkie.Tests.Unit.Fixtures
{
    public class DbTemplateFixture : IDisposable
    {
        private readonly AppDbContext _context;

        public DbTemplateFixture()
        {
            DatabaseName = $"maptalkie_testing_tmpl_{Guid.NewGuid():N}";
            ConnectionString = $"Host=localhost;Database={DatabaseName};Username=admin;Password=admin";
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(ConnectionString, x => x.UseNetTopologySuite());
            _context = new AppDbContext(optionsBuilder.Options, new NullLoggerFactory());
            _context.Database.EnsureCreated();
        }

        public string ConnectionString { get; }
        public string DatabaseName { get; }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }
    }
}