namespace MapTalkie.Common.Messages.PrivateMessages
{
    public interface IPrivateMessageBase
    {
        string SenderId { get; }
        string RecipientId { get; }
        long MessageId { get; }
    }
}