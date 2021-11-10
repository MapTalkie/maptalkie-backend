using System;

namespace MapTalkie.Services.MessageService
{
    public class PrivateConversationView
    {
        public int Id { get; set; }
        public string RecipientId { get; set; } = string.Empty;
        public string? LastMessage { get; set; }
        public DateTime? ActiveAt { get; set; }
    }
}