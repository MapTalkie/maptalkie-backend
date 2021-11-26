using System;
using MapTalkie.Utils.JsonConverters;
using Newtonsoft.Json;

namespace MapTalkie.Services.MessageService
{
    public class ConversationView
    {
        [JsonConverter(typeof(IdToStringConverter))]
        public long Id { get; set; }

        public MessageView? LastMessage { get; set; }
        public DateTime LastUpdate { get; set; }
        public UserInMessageView Recipient { get; set; }
        public bool CanSend { get; set; }
    }
}