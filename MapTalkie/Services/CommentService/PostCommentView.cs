using System;

namespace MapTalkie.Services.CommentService
{
    public class PostCommentView
    {
        public long Id { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string Sender { get; set; }
        public DateTime SentAt { get; set; }

        public int Replies { get; set; }
    }
}