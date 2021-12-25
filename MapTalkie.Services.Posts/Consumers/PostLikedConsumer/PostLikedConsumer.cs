using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MapTalkie.DB.Context;
using MapTalkie.Domain.Messages.Posts;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MapTalkie.Services.Posts.Consumers.PostLikedConsumer;

public class PostLikedConsumer : IConsumer<Batch<PostEngagement>>
{
    private const long MaxUpdates = 100; // пока что пусть будет так
    private static readonly TimeSpan MaxRankDecayTimeout = TimeSpan.FromHours(1);
    private readonly AppDbContext _context;
    private readonly ILogger<PostLikedConsumer> _logger;
    private DateTime _lastRankDecay = DateTime.MinValue;
    private long _updatesCount;

    public PostLikedConsumer(AppDbContext context, ILogger<PostLikedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<Batch<PostEngagement>> context)
    {
        await PublishEngagement(context);
        Interlocked.Add(ref _updatesCount, context.Message.Length);

        if (Interlocked.Read(ref _updatesCount) > MaxUpdates || _lastRankDecay + MaxRankDecayTimeout < DateTime.Now)
        {
            _lastRankDecay = DateTime.Now;
            Interlocked.And(ref _updatesCount, 0);
            // тут может произойти race-condition, но я просто это проигнорирую,
            // потому что это функция вызывается редко
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"call update_exp_ranking_decay({0.0002}, {0.25});");
        }

        _logger.LogDebug("Consumed {0} with {1} messages inside", nameof(Batch<PostEngagement>),
            context.Message.Length);
    }

    private async Task PublishEngagement(ConsumeContext<Batch<PostEngagement>> context)
    {
        var postIds = context.Message.Select(c => c.Message.PostId).Distinct().ToList();

        foreach (var dbEngagement in _context.Posts
                     .Where(p => postIds.Contains(p.Id) && p.Available)
                     .Select(p => new
                         { p.Id, p.Location, p.CachedCommentsCount, p.CachedLikesCount, p.CachedSharesCount }))
            await context.Publish(new EngagementUpdate
            {
                PostId = dbEngagement.Id,
                Likes = dbEngagement.CachedLikesCount,
                Shares = dbEngagement.CachedSharesCount,
                Comments = dbEngagement.CachedCommentsCount,
                Location = dbEngagement.Location
            });
    }
}