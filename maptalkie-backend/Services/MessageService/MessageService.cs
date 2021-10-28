using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.MessageService
{
    internal class MessageService : IMessageService
    {
        private AppDbContext _context;
        
        public MessageService(AppDbContext context)
        {
            _context = context;
        }
        
        public async Task<MapMessage> CreateTextMessage(string text, User user, Location location, bool isOriginalLocation)
        {
            var message = new MapMessage
            {
                Text = text,
                Loc = location,
                UserId = user.Id,
                IsOriginalLocation = isOriginalLocation
            };
            await _context.Messages.AddAsync(message);
            return message;
        }

        public Task<MapMessage?> GetMessageOrNull(long id)
        {
            return _context.Messages.Where(m => m.Id == id).FirstOrDefaultAsync()!;
        }

        public Task<List<MapMessage>> FindMessagesInArea(LatLngBounds bounds, DateTime? before = null)
        {
            var query = _context.Messages
                .Where(m => m.Available)
                .Where(m => (
                    m.Loc.Latitude <= bounds.NorthEast.Latitude &&
                    m.Loc.Latitude >= bounds.SouthWest.Latitude &&
                    m.Loc.Longitude <= bounds.NorthEast.Longitude &&
                    m.Loc.Longitude >= bounds.SouthWest.Longitude));
            if (before != null)
                query = query.Where(m => m.CreatedAt <= before);
            return query.ToListAsync();
        }
    }
}