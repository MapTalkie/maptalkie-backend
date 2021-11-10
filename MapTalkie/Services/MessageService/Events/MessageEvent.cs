namespace MapTalkie.Services.MessageService.Events
{
    public class MessageEvent
    {
        public string SenderId { get; set; } = default!;
        public int ConversationId { get; set; }
        public long MessageId { get; set; }
        public string MessageShort { get; set; } = string.Empty;
    }
}