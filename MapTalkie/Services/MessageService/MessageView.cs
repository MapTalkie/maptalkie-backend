using MapTalkie.Utils.JsonConverters;
using Newtonsoft.Json;

namespace MapTalkie.Services.MessageService
{
    public class MessageView
    {
        [JsonConverter(typeof(IdToStringConverter))]
        public long Id { get; set; }

        public string SenderId { get; set; } = default!;
        public string Sender { get; set; } = default!;
        public string Text { get; set; } = string.Empty;
        public bool Read { get; set; }
    }
}