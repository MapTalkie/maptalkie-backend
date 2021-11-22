using NetTopologySuite.Geometries;

namespace MapTalkieCommon.Utils
{
    public class Location
    {
        public double X { get; set; }
        public double Y { get; set; }

        public static explicit operator Coordinate(Location location)
        {
            return new Coordinate(location.X, location.Y);
        }

        public static explicit operator Point(Location location)
        {
            return new Point(location.X, location.Y) { SRID = 3857 };
        }

        public static explicit operator Location(Point point)
        {
            var coordinate = MapConvert.ToMercator(point.Coordinate);
            return new Location { X = coordinate.X, Y = coordinate.Y };
        }

        public static implicit operator Location(Coordinate point)
        {
            return new Location { X = point.X, Y = point.Y };
        }
    }
}