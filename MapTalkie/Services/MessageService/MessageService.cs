using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.MessagesImpl;
using MapTalkieCommon.Messages;
using MapTalkieDB;
using MapTalkieDB.Context;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.MessageService
{
    public class MessageService : DbService, IMessageService
    {
        private readonly IPublishEndpoint _publishEndpoint;

        public MessageService(AppDbContext context, IPublishEndpoint publishEndpoint) : base(context)
        {
            _publishEndpoint = publishEndpoint;
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

        public IQueryable<MapTalkieDB.PrivateMessage> QueryPrivateMessages(int conversationId)
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

        public Task<MapTalkieDB.PrivateMessage?> GetPrivateMessageOrNull(long id)
        {
            return DbContext.PrivateMessages
                .Where(pm => pm.Id == id && pm.Available)
                .FirstOrDefaultAsync()!;
        }

        public Task<PrivateConversation?> GetPrivateConversationOrNull(long id)
        {
            return DbContext.PrivateConversations.Where(pm => pm.Id == id).FirstOrDefaultAsync()!;
        }


        public async Task<MapTalkieDB.PrivateMessage> SendPrivateMessage(PrivateConversation privateConversation,
            User sender,
            string text)
        {
            var recipientId = privateConversation.UserHigherId == sender.Id
                ? privateConversation.UserLowerId
                : privateConversation.UserHigherId;
            var pm = new MapTalkieDB.PrivateMessage
            {
                Text = text,
                Sender = sender,
                Conversation = privateConversation,
                RecipientId = recipientId
            };
            DbContext.Add(pm);
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPrivateMessage>(new PrivateMessageEvent
            {
                ConversationId = privateConversation.Id,
                MessageId = pm.Id,
                RecipientId = recipientId,
                SenderId = sender.Id
            });
            return pm;
        }

        public async Task<MapTalkieDB.PrivateMessage> SendPrivateMessage(User recipient, User sender, string text)
        {
            var privateConv = await EnsurePrivateConversation(recipient.Id, sender.Id);
            return await SendPrivateMessage(privateConv, sender, text);
        }

        public async Task<MapTalkieDB.PrivateMessage> UpdatePrivateMessage(long id,
            Action<MapTalkieDB.PrivateMessage> updateFunction)
        {
            var pm = await GetPrivateMessageOrNull(id);

            if (pm == null) throw new PrivateMessageNotFoundException(id);

            updateFunction(pm);
            var oldText = pm.Text;
            pm.UpdatedAt = DateTime.UtcNow;
            await DbContext.SaveChangesAsync();

            if (pm.Text != oldText)
                await _publishEndpoint.Publish<IPrivateMessageUpdate>(new PrivateMessageUpdateEvent
                {
                    ConversationId = pm.ConversationId,
                    MessageId = pm.Id,
                    SenderId = pm.SenderId,
                    RecipientId = pm.RecipientId,
                    NewText = pm.Text
                });

            return pm;
        }

        public async Task DeleteMessage(MapTalkieDB.PrivateMessage message)
        {
            message.Available = false;
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPrivateMessageDeleted>(new PrivateMessageDeletedEvent
            {
                MessageId = message.Id,
                SenderId = message.SenderId,
                RecipientId = message.RecipientId,
                ConversationId = message.ConversationId
            });
        }

        private async Task<PrivateConversation> EnsurePrivateConversation(string recipientId, string senderId)
        {
            string higher = recipientId, lower = senderId;

            if (string.CompareOrdinal(higher, lower) < 0)
                (higher, lower) = (lower, higher);

            var conv = await DbContext.PrivateConversations.Where(pc =>
                    pc.UserHigherId == higher && pc.UserLowerId == lower)
                .FirstOrDefaultAsync();

            if (conv == null)
            {
                conv = new PrivateConversation
                {
                    UserHigherId = higher,
                    UserLowerId = lower
                };
                DbContext.Add(conv);
                await DbContext.SaveChangesAsync();
            }

            return conv;
        }
    }
}