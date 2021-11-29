namespace MapTalkie.Domain.Messages.PrivateMessages
{
    public record PrivateMessage : PrivateMessageBase
    {
        public string Text { get; set; }
    }
}