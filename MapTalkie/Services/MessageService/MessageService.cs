using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Common.Messages.PrivateMessages;
using MapTalkie.DB;
using MapTalkie.DB.Context;
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

        public async Task<List<MessageView>> GetDirectMessages(
            string userId,
            string recipientId,
            GetDirectMessagesOptions? options = null)
        {
            var beforeTime = options?.BeforeTime;

            var messages =
                from mr in DbContext.PrivateMessageReceipts
                where
                    mr.UserIdA == userId && mr.Message.Available &&
                    (beforeTime == null || mr.Message.CreatedAt < beforeTime)
                select new MessageView
                {
                    Id = mr.MessageId,
                    Sender = mr.Message.Sender.UserName,
                    SenderId = mr.Message.SenderId,
                    Text = mr.Message.Text,
                    Read = mr.Message.Read
                };

            if (options?.Limit != null)
                messages = messages.Take((int)options.Limit);
            return await messages.ToListAsync();
        }

        public async Task<List<ConversationView>> GetUserConversations(string userId)
        {
            var conversations =
                from cp in DbContext.PrivateConversationParticipants
                where cp.IsActive && cp.SenderId == userId
                let lastMessage = DbContext.PrivateMessageReceipts
                    .LastOrDefault(r => r.UserIdA == userId && r.UserIdB == cp.RecipientId)
                select new ConversationView
                {
                    Recipient = new UserInMessageView
                    {
                        Id = cp.RecipientId,
                        UserName = cp.Recipient.UserName
                    },
                    CanSend = false,
                    LastMessage = lastMessage == null
                        ? null
                        : new MessageView
                        {
                            Id = lastMessage.Message.Id,
                            Read = lastMessage.Message.Read,
                            Sender = lastMessage.Message.Sender.UserName,
                            SenderId = lastMessage.Message.Sender.Id,
                            Text = lastMessage.Message.Text
                        },
                    LastUpdate = default
                };

            return await conversations.ToListAsync();
        }

        public async Task<PrivateMessage> CreateMessage(string userId1, string userId2, string text)
        {
            var message = new PrivateMessage
            {
                Text = text,
                SenderId = userId1,
                Receipts =
                {
                    new PrivateMessageReceipt
                    {
                        UserIdA = userId1,
                        UserIdB = userId2,
                        OutFlag = true
                    },
                    new PrivateMessageReceipt
                    {
                        UserIdA = userId2,
                        UserIdB = userId1,
                        OutFlag = false
                    }
                }
            };
            DbContext.Add(message);
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPrivateMessage>(new
            {
                Text = text,
                SenderId = userId1,
                RecipientId = userId2,
                MessageId = message.Id
            });
            return message;
        }

        public async Task<bool> DeleteMessageForUser(long messageId, string senderId, string recipientId)
        {
            var receipt = await DbContext.PrivateMessageReceipts
                .Where(m => m.UserIdA == senderId && m.UserIdB == recipientId && m.MessageId == messageId)
                .FirstOrDefaultAsync();

            if (receipt == null)
                return false;
            DbContext.Remove(receipt);
            await DbContext.SaveChangesAsync();
            await _publishEndpoint.Publish<IPrivateMessageDeleted>(new
            {
                SenderId = receipt.OutFlag ? receipt.UserIdA : receipt.UserIdB,
                RecipientId = receipt.OutFlag ? receipt.UserIdB : receipt.UserIdA,
                receipt.MessageId
            });
            return true;
        }
    }
}