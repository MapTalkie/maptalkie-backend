using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService.Events
{
    public class PopularPostEvent
    {
        public Point Location { get; set; } = default!;
        public int Index { get; set; }
        public long? DeletedPostId { get; set; }
        public int? DeletedIndex { get; set; }
        public PostPopularity Popularity { get; set; } = default!;
    }
}