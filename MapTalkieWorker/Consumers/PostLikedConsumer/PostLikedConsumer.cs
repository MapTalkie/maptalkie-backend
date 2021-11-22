using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkieCommon.Messages;
using MapTalkieCommon.Utils;
using MapTalkieDB.Context;
using MapTalkieWorker.MessagesImpl;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MapTalkieWorker.Consumers.PostLikedConsumer
{
    public class PostLikedConsumer : IConsumer<Batch<IPostEngagement>>
    {
        private readonly AppDbContext _context;

        public PostLikedConsumer(AppDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<Batch<IPostEngagement>> context)
        {
            var diff = new Dictionary<string, Engagement>();
            foreach (var itemContext in context.Message)
            {
                var msg = itemContext.Message;
                if (!diff.ContainsKey(msg.PostId))
                {
                    diff[msg.PostId] = new Engagement();
                }

                switch (msg.Type)
                {
                    case PostEngagementType.Favorite:
                        diff[msg.PostId].Likes++;
                        break;
                    case PostEngagementType.FavoriteRemoved:
                        diff[msg.PostId].Likes--;
                        break;
                    case PostEngagementType.Comment:
                        diff[msg.PostId].Comments++;
                        break;
                    case PostEngagementType.CommentRemoved:
                        diff[msg.PostId].Comments--;
                        break;
                    case PostEngagementType.Share:
                        diff[msg.PostId].Shares++;
                        break;
                    case PostEngagementType.ShareRemoved:
                        diff[msg.PostId].Shares--;
                        break;
                }
            }

            var ids = diff.Keys.ToList();
            var dbEngagements = await _context.Posts
                .Where(p => ids.Contains(p.Id))
                .Select(p => new
                {
                    p.Id, p.CreatedAt, p.Location,
                    Likes = p.CachedLikesCount + p.Likes.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                    Shares = p.CachedSharesCount + p.Shares.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                    Comments = p.CachedCommentsCount + p.Comments.Count(l => l.CreatedAt > p.CacheUpdatedAt),
                })
                .ToListAsync();

            var messages = new List<AccumulatedEngagementEvent>(dbEngagements.Count);
            foreach (var dbEngagement in dbEngagements)
            {
                var engagement = diff[dbEngagement.Id];
                messages.Add(new AccumulatedEngagementEvent
                {
                    PostId = dbEngagement.Id,
                    Likes = dbEngagement.Likes + engagement.Likes,
                    Shares = dbEngagement.Shares + engagement.Shares,
                    Comments = dbEngagement.Comments + engagement.Comments,
                    Location = (Location)dbEngagement.Location
                });
            }

            await context.PublishBatch(messages);
        }

        private class Engagement
        {
            public int Likes { get; set; }
            public int Comments { get; set; }
            public int Shares { get; set; }
        }
    }
}