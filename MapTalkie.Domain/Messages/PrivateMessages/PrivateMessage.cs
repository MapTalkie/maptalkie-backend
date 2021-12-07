namespace MapTalkie.Domain.Messages.PrivateMessages
{
    public record PrivateMessage(
        string SenderId,
        string RecipientId,
        long MessageId,
        string Text);
}