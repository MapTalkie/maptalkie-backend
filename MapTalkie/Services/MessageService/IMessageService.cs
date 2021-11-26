using System.Collections.Generic;
using System.Threading.Tasks;
using MapTalkie.DB;

namespace MapTalkie.Services.MessageService
{
    public interface IMessageService
    {
        Task<List<MessageView>> GetDirectMessages(
            string userId,
            string recipientId,
            GetDirectMessagesOptions? options = null);

        Task<List<ConversationView>> GetUserConversations(string userId);

        Task<PrivateMessage> CreateMessage(
            string userId1,
            string userId2,
            string text);

        Task<bool> DeleteMessageForUser(long messageId, string senderId, string recipientId);
    }
}