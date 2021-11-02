using System.Linq;
using NetTopologySuite.Geometries;
using Pidgin;

namespace MapTalkie.Utils.Binders
{
    public class Parsers
    {
        private static Parsers? _parsers = null;

        public readonly Parser<char, Point> Point;
        public readonly Parser<char, Polygon> Polygon;

        private Parsers()
        {
            Parser<char, double> latitude = Parser
                .Char('(')
                .Then(Parser.SkipWhitespaces)
                .Then(Parser.Real)
                .Before(Parser.SkipWhitespaces)
                .Before(Parser.Char(','));

            Parser<char, double> longitude = Parser.Real
                .Before(Parser.SkipWhitespaces)
                .Before(Parser.Char(')'));

            Parser<char, Point> point = Parser.Map((lat, lon) => new Point(lon, lat), latitude, longitude);

            Point = point.Before(Parser.CIString("point"));
            Parser<char, Coordinate> coordinate =
                Parser.Map((lat, lon) => new Coordinate(lon, lat), latitude, longitude);
            Parser<char, Coordinate[]> coordinates = coordinate
                .Separated(Parser.Char(','))
                .Map(v => v.ToArray());

            Polygon = Parser
                .CIString("polygon(")
                .Then(coordinates)
                .Before(Parser.Char(')'))
                .Map(coords => new Polygon(new LinearRing(coords)));
        }

        public static Parsers GetInstance()
        {
            if (_parsers == null)
                _parsers = new Parsers();
            return _parsers;
        }
    }
}