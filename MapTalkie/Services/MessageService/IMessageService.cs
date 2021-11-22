using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkieDB;

namespace MapTalkie.Services.MessageService
{
    public interface IMessageService
    {
        IQueryable<PrivateConversation> QueryPrivateConversations(string userId);
        IQueryable<PrivateConversationView> QueryPrivateConversationViews(string userId);
        IQueryable<PrivateMessage> QueryPrivateMessages(int conversationId);
        IQueryable<PrivateMessageView> QueryPrivateMessageViews(int conversationId);
        Task<PrivateMessage?> GetPrivateMessageOrNull(long id);
        Task<PrivateConversation?> GetPrivateConversationOrNull(long id);
        Task<PrivateMessage> SendPrivateMessage(PrivateConversation privateConversation, User sender, string text);
        Task<PrivateMessage> SendPrivateMessage(User recipient, User sender, string text);
        Task<PrivateMessage> UpdatePrivateMessage(long id, Action<PrivateMessage> updateFunction);
        Task DeleteMessage(PrivateMessage message);
    }
}