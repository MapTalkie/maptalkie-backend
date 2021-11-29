using System;

namespace MapTalkie.Domain.Messages.Posts
{
    public record PostCreated : PostMessage
    {
        public DateTime CreatedAt { get; set; }
    }
}