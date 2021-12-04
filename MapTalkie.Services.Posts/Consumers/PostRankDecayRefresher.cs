using System.Threading.Tasks;
using MapTalkie.DB.Context;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.Posts.Consumers
{
    public class PostRankDecayRefresher : IConsumer<PostRankDecayRefresher.RefreshRankDecay>
    {
        private readonly AppDbContext _context;

        public PostRankDecayRefresher(AppDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<RefreshRankDecay> context)
        {
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"call update_exp_ranking_decay({context.Message.MinDecayAllowed}, {context.Message.ExpCoefficient});");
        }

        public record RefreshRankDecay(double MinDecayAllowed, double ExpCoefficient);
    }
}