using NetTopologySuite.Geometries;

namespace MapTalkie.Utils
{
    public class Location
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }

        public static implicit operator Location(Point point) =>
            new Location { Longitude = point.X, Latitude = point.Y };
    }
}