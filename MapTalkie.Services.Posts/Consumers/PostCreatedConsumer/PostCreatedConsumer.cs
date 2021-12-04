using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Domain.Messages.Posts;
using MapTalkie.Domain.Utils;
using MassTransit;
using Microsoft.Extensions.Caching.Memory;

namespace MapTalkie.Services.Posts.Consumers.PostCreatedConsumer
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class PostCreatedConsumer : IConsumer<Batch<PostCreated>>
    {
        private readonly IMemoryCache _cache;

        public PostCreatedConsumer(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task Consume(ConsumeContext<Batch<PostCreated>> context)
        {
            await SendGeoUpdate(context);
        }

        private async Task SendGeoUpdate(ConsumeContext<Batch<PostCreated>> context)
        {
            var perArea = new Dictionary<AreaId, SortedList<DateTime, PostCreated>>();

            foreach (var ctx in context.Message)
            {
                var areas = AreaId.AllFromPoint(ctx.Message.Location);
                foreach (var areaId in areas)
                {
                    if (!perArea.ContainsKey(areaId))
                    {
                        perArea[areaId] = new SortedList<DateTime, PostCreated>(DescendedDateComparer.Instance);
                    }

                    perArea[areaId].Add(ctx.Message.CreatedAt, ctx.Message);
                }
            }

            var updates = perArea.Select(kv =>
            {
                // TODO динамическая конфигурация, вместо хардкода значения 30
                var newPosts = kv.Value.Count > 30 ? kv.Value.Values.Take(30).ToList() : kv.Value.Values.ToList();
                return new GeoUpdate(kv.Key, newPosts);
            }).ToList();

            var geoUpdates = new GeoUpdates(updates);
            await context.Publish(geoUpdates);
        }

        // https://stackoverflow.com/questions/7815930/sortedlist-desc-order/7816014
        private class DescendedDateComparer : IComparer<DateTime>
        {
            public static readonly DescendedDateComparer Instance = new();

            public int Compare(DateTime x, DateTime y)
            {
                // use the default comparer to do the original comparison for datetimes
                int ascendingResult = Comparer<DateTime>.Default.Compare(x, y);

                // turn the result around
                return 0 - ascendingResult;
            }
        }
    }
}