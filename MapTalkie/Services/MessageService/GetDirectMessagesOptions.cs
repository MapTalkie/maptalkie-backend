using System;

namespace MapTalkie.Services.MessageService
{
    public class GetDirectMessagesOptions
    {
        public DateTime? BeforeTime { get; set; }
        public int? Limit { get; set; } = 50;
    }
}