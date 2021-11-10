using MapTalkie.Models;
using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService.Events
{
    public class NewPostEvent
    {
        public NewPostEvent(Post post)
        {
            Location = post.Location;
            PostId = post.Id;
        }

        public long PostId { get; set; }
        public Point Location { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
    }
}