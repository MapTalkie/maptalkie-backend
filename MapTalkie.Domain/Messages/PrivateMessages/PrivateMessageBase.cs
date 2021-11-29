namespace MapTalkie.Domain.Messages.PrivateMessages
{
    public record PrivateMessageBase
    {
        public string SenderId { get; set; }
        public string RecipientId { get; set; }
        public long MessageId { get; set; }
    }
}