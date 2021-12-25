namespace MapTalkie.Domain.Messages.PrivateMessages;

public record PrivateMessageDeleted(
    string SenderId,
    string RecipientId,
    long MessageId);