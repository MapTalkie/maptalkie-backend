// See https://aka.ms/new-console-template for more information

using MapTalkie.Services.Posts.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var optionsBuilder = new DbContextOptionsBuilder<PostsDbContext>();
var configuration = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();
optionsBuilder.UseNpgsql(configuration.GetConnectionString("Database"), options => { options.UseNetTopologySuite(); });
var context = new PostsDbContext(optionsBuilder.Options);
context.Database.Migrate();