namespace MapTalkie.Services.MessageService
{
    public class PrivateMessageView
    {
        public long Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string RecipientId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string RecipientName { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public bool Read { get; set; }
    }
}