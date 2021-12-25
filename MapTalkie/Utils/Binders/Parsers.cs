using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using Pidgin;

namespace MapTalkie.Utils.Binders;

public class Parsers
{
    // POINT(X,Y)
    public static Parser<char, Point> LatLonPoint = Parser.Map(
        (x, y) => new Point(x, y) { SRID = 4326 },
        Parser.CIString("point(")
            .Then(Parser.Real)
            .Before(Parser.Char(','))
            .Before(Parser.SkipWhitespaces),
        Parser.Real
            .Before(Parser.Char(')')));

    // POLY(X0 Y0, X1 Y1, ...)

    public static Parser<char, Coordinate> Coordinate = Parser.Map(
        (x, y) => new Coordinate(x, y),
        Parser.Real,
        Parser.Char(' ').Then(Parser.Real));

    public static Parser<char, Polygon> Polygon = Parser.Map(
        coords => new Polygon(new LinearRing(FilterPolygonPoints(coords))),
        Parser.CIString("poly(")
            .Then(Coordinate.Separated(Parser.Char(',')))
            .Before(Parser.Char(')')));

    private static Coordinate[] FilterPolygonPoints(IEnumerable<Coordinate> coordinates)
    {
        var list = coordinates.ToList();
        if (list.Count > 1 && !list[0].Equals(list.Last())) list.Add(list[0].Copy());

        return list.ToArray();
    }
}