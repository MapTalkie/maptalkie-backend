namespace MapTalkieCommon.Messages
{
    public interface IPrivateMessageBase
    {
        int ConversationId { get; }
        string SenderId { get; }
        string RecipientId { get; }
        long MessageId { get; }
    }
}