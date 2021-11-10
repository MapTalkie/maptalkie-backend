using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;
using MapTalkie.Models.Context;
using MapTalkie.Services.EventBus;
using MapTalkie.Services.MessageService.Events;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.MessageService
{
    public class MessageService : DbService, IMessageService
    {
        private IEventBus _eventBus;

        public MessageService(AppDbContext context, IEventBus eventBus) : base(context)
        {
            _eventBus = eventBus;
        }

        public IQueryable<PrivateConversation> QueryPrivateConversations(string userId)
        {
            return DbContext.PrivateConversations.Where(pc => pc.UserHigherId == userId || pc.UserLowerId == userId);
        }

        public IQueryable<PrivateConversationView> QueryPrivateConversationViews(string userId)
        {
            return
                from c in QueryPrivateConversations(userId)
                let lastMessage = c.PrivateMessages.OrderByDescending(m => m.CreatedAt).FirstOrDefault()
                select new PrivateConversationView
                {
                    Id = c.Id,
                    LastMessage = lastMessage.Text,
                    ActiveAt = lastMessage.CreatedAt,
                    RecipientId = c.UserHigherId == userId ? c.UserLowerId : c.UserHigherId
                };
        }

        public IQueryable<PrivateMessage> QueryPrivateMessages(int conversationId)
        {
            return DbContext.PrivateMessages.Where(pm => pm.ConversationId == conversationId && pm.Available);
        }

        public IQueryable<PrivateMessageView> QueryPrivateMessageViews(int conversationId)
        {
            return QueryPrivateMessages(conversationId).Select(m => new PrivateMessageView
            {
                Id = m.Id,
                Read = m.Read,
                SenderId = m.SenderId,
                SenderName = m.Sender.UserName,
                RecipientId = m.RecipientId,
                RecipientName = m.Recipient.UserName,
                Text = m.Text
            });
        }

        public Task<PrivateMessage?> GetPrivateMessageOrNull(long id)
        {
            return DbContext.PrivateMessages
                .Where(pm => pm.Id == id && pm.Available)
                .FirstOrDefaultAsync()!;
        }

        public Task<PrivateConversation?> GetPrivateConversationOrNull(long id)
        {
            return DbContext.PrivateConversations.Where(pm => pm.Id == id).FirstOrDefaultAsync()!;
        }


        public async Task<PrivateMessage> SendPrivateMessage(PrivateConversation privateConversation, User sender,
            string text)
        {
            var pm = new PrivateMessage
            {
                Text = text,
                Sender = sender,
                Conversation = privateConversation,
                RecipientId = privateConversation.UserHigherId == sender.Id
                    ? privateConversation.UserLowerId
                    : privateConversation.UserHigherId
            };
            DbContext.Add(pm);
            await DbContext.SaveChangesAsync();
            await _eventBus.Trigger(
                new[]
                {
                    MessageServiceDefaults.MessageEventPrefix + privateConversation.UserHigherId,
                    MessageServiceDefaults.MessageEventPrefix + privateConversation.UserLowerId
                },
                new MessageEvent
                {
                    SenderId = sender.Id,
                    ConversationId = privateConversation.Id,
                    MessageId = pm.Id,
                    MessageShort = text.Length > 40 ? text.Substring(0, 37) + "..." : text
                });
            return pm;
        }

        public async Task<PrivateMessage> SendPrivateMessage(User recipient, User sender, string text)
        {
            var privateConv = await EnsurePrivateConversation(recipient.Id, sender.Id);
            return await SendPrivateMessage(privateConv, sender, text);
        }

        public async Task<PrivateMessage> UpdatePrivateMessage(long id, Action<PrivateMessage> updateFunction)
        {
            var pm = await GetPrivateMessageOrNull(id);

            if (pm != null)
            {
                updateFunction(pm);
                var oldText = pm.Text;
                pm.UpdatedAt = DateTime.UtcNow;
                await DbContext.SaveChangesAsync();
                if (pm.Text != oldText)
                {
                    await _eventBus.Trigger(pm.ConversationId, new ConversationUpdate
                    {
                        ConversationId = pm.ConversationId,
                        MessageId = pm.Id,
                        NewText = pm.Text
                    });
                }

                return pm;
            }

            throw new PrivateMessageNotFoundException(id);
        }

        public async Task DeleteMessage(PrivateMessage message)
        {
            message.Available = false;
            await Task.WhenAll(
                DbContext.SaveChangesAsync(),
                _eventBus.Trigger(message.ConversationId, new ConversationUpdate
                {
                    MessageId = message.Id,
                    ConversationId = message.ConversationId,
                    IsDeleted = true
                }));
        }

        private async Task<PrivateConversation> EnsurePrivateConversation(string recipientId, string senderId)
        {
            string higher = recipientId, lower = senderId;

            if (String.CompareOrdinal(higher, lower) < 0)
                (higher, lower) = (lower, higher);

            var conv = await DbContext.PrivateConversations.Where(pc =>
                    pc.UserHigherId == higher && pc.UserLowerId == lower)
                .FirstOrDefaultAsync();

            if (conv == null)
            {
                conv = new PrivateConversation
                {
                    UserHigherId = higher,
                    UserLowerId = lower,
                };
                DbContext.Add(conv);
                await DbContext.SaveChangesAsync();
            }

            return conv;
        }
    }
}