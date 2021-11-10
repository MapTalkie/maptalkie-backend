using NetTopologySuite.Geometries;

namespace MapTalkie.Models
{
    public class PostCluster
    {
        public int Id { get; set; }
        public Point Location { get; set; }
        public int Level { get; set; }
    }
}