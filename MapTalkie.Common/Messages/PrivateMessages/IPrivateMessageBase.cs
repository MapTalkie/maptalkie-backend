namespace MapTalkie.Common.Messages.PrivateMessages
{
    public interface IPrivateMessageBase
    {
        int ConversationId { get; }
        string SenderId { get; }
        string RecipientId { get; }
        long MessageId { get; }
    }
}