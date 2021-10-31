using System;

namespace MapTalkie.Services.CommentService
{
    public class CommentNotFoundException : Exception
    {
        public CommentNotFoundException(long id) : base($"Comment with id={id} couldn't be found")
        {
        }
    }
}