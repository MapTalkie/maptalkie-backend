namespace MapTalkie.Services.MessageService.Events
{
    public class ConversationUpdate
    {
        public int ConversationId { get; set; }
        public long MessageId { get; set; }
        public string? NewText { get; set; } = null;
        public bool IsDeleted { get; set; } = false;
    }
}