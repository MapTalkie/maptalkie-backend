using System;

namespace MapTalkie.Views
{
    public class ConversationView
    {
        public MessageView? LastMessage { get; set; }
        public DateTime LastUpdate { get; set; }
        public UserInMessageView Recipient { get; set; }
        public bool CanSend { get; set; }
    }
}