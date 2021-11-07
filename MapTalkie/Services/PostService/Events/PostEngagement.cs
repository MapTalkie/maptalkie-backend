using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService.Events
{
    public class PostEngagement
    {
        public Point Location { get; set; }
        public long Likes { get; set; }
        public long Reposts { get; set; }
        public long PostId { get; set; }
    }
}