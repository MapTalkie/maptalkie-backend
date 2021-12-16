namespace MapTalkie.Domain.Messages.PrivateMessages
{
    public record PrivateMessage(
        string SenderId,
        string SenderUsername,
        string RecipientId,
        string RecipientUsername,
        long MessageId,
        string Text);
}