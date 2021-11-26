namespace MapTalkie.Services.MessageService
{
    public class NoConversationParticipationException : MessageServiceException
    {
        public NoConversationParticipationException(string user, long conversationId)
            : base($"User {user} is not a participant of conversation #{conversationId}")
        {
        }
    }
}