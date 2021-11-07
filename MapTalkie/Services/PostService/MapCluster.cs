using NetTopologySuite.Geometries;

namespace MapTalkie.Services.PostService
{
    public class MapCluster
    {
        public MapCluster(Point point, int size, string? clusterId = null)
        {
            ClusterId = clusterId;
            ClusterSize = size;
            Centroid = point;
        }

        public int ClusterSize { get; set; }
        public string? ClusterId { get; set; }
        public Point Centroid { get; set; }
    }
}