using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService.Events
{
    public class PostUpdate
    {
        public long PostId { get; set; }

        public bool Expired { get; set; }

        public Point Point { get; set; }
    }
}