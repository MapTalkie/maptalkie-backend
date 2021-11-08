using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public class MapCluster
    {
        public int ClusterSize { get; set; }
        public string? ClusterId { get; set; }
        public Point Centroid { get; set; }
    }
}