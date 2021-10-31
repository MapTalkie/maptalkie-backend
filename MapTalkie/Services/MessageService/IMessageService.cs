using System;
using System.Linq;
using System.Threading.Tasks;
using MapTalkie.Models;

namespace MapTalkie.Services.MessageService
{
    public interface IMessageService
    {
        IQueryable<PrivateConversation> QueryPrivateConversations(int userId);
        IQueryable<PrivateMessage> QueryPrivateMessages(int conversationId);
        Task<PrivateMessage?> GetPrivateMessageOrNull(long id);
        Task<PrivateMessage> SendPrivateMessage(int conversationId, int senderId, string text);

        Task<PrivateMessage> UpdatePrivateMessage(long id, Action<PrivateMessage> updateFunction);
    }
}