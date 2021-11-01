using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.MessageService
{
    public class MessageService : DbService, IMessageService
    {
        public MessageService(AppDbContext context) : base(context)
        {
        }

        public IQueryable<PrivateConversation> QueryPrivateConversations(string userId)
        {
            return DbContext.PrivateConversations.Where(pc => pc.UserHigherId == userId || pc.UserLowerId == userId);
        }

        public IQueryable<PrivateMessage> QueryPrivateMessages(int conversationId)
        {
            return DbContext.PrivateMessages.Where(pm => pm.ConversationId == conversationId && pm.Available);
        }

        public Task<PrivateMessage?> GetPrivateMessageOrNull(long id)
        {
            return DbContext.PrivateMessages.Where(pm => pm.Id == id).FirstOrDefaultAsync()!;
        }

        public async Task<PrivateMessage> SendPrivateMessage(int conversationId, int senderId, string text)
        {
            var pm = new PrivateMessage
            {
                Text = text,
                SenderId = senderId,
                ConversationId = conversationId
            };
            DbContext.Add(pm);
            await DbContext.SaveChangesAsync();
            return pm;
        }

        public async Task<PrivateMessage> UpdatePrivateMessage(long id, Action<PrivateMessage> updateFunction)
        {
            var pm = await GetPrivateMessageOrNull(id);

            if (pm != null)
            {
                updateFunction(pm);
                pm.UpdatedAt = DateTime.UtcNow;
                await DbContext.SaveChangesAsync();
                return pm;
            }

            throw new PrivateMessageNotFoundException(id);
        }
    }
}