using System;
using NetTopologySuite.Geometries;

namespace MapTalkie.Utils.RTEFC
{
    public class Cluster
    {
        public Cluster(Coordinate centroid, int value)
        {
            Centroid = centroid;
            Value = value;
        }

        public int Value { get; set; }
        public Guid Id { get; set; } = Guid.NewGuid();

        public Coordinate Centroid { get; set; }
    }
}