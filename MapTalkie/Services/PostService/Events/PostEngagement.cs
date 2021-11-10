using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService.Events
{
    public class PostEngagement
    {
        public Point Location { get; set; }
        public PostPopularity Popularity { get; set; } = new();
    }
}