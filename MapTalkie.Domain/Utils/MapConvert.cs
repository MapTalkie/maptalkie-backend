using System;
using System.Linq;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;

namespace MapTalkie.Domain.Utils
{
    public static class MapConvert
    {
        #region Проверка SRID

        private static void ThrowIfInvalidSrid(Geometry point, int requiredSrid)
        {
            if (point.SRID != requiredSrid)
                throw new InvalidOperationException($"Invalid SRID of point: SRID of the point must be {requiredSrid}");
        }

        public static void ThrowIfNot4326(Geometry point)
        {
            ThrowIfInvalidSrid(point, 4326);
        }

        public static void ThrowIfNot3857(Geometry point)
        {
            ThrowIfInvalidSrid(point, 3857);
        }

        #endregion

        #region Конвертация проекции меркатор (3857) и WGS84 (SRID 4326)

        private static readonly ICoordinateTransformation LatLonToMercator = new CoordinateTransformationFactory()
            .CreateFromCoordinateSystems(
                GeographicCoordinateSystem.WGS84,
                ProjectedCoordinateSystem.WebMercator);

        public static readonly MathTransform LatLonToMercatorTransform = LatLonToMercator.MathTransform;
        public static readonly MathTransform MercatorToLatLonTransform = LatLonToMercatorTransform.Inverse();

        public static Point ToMercator(Point point)
        {
            if (point.SRID == 3857)
                return point;
            if (point.SRID != 0 && point.SRID != 4326)
                throw new InvalidSridException(new[] { 0, 4326, 3857 }, point.SRID);
            return new Point(ToMercator(point.Coordinate)) { SRID = 3857 };
        }

        public static Coordinate ToMercator(Coordinate point)
        {
            var x = point.X;
            var y = point.Y;
            LatLonToMercatorTransform.Transform(ref x, ref y);
            return new Coordinate(x, y);
        }

        public static Polygon ToMercator(Polygon poly)
        {
            if (poly.SRID == 3857)
                return poly;
            if (poly.SRID != 0 && poly.SRID != 4326)
                throw new InvalidSridException(new[] { 0, 4326, 3857 }, poly.SRID);

            var coords = poly.Shell.Coordinates
                .Select(c => new double[] { c.X, c.Y })
                .ToList();
            var newCoords = LatLonToMercatorTransform.TransformList(coords)
                .Select(l => new Coordinate(l[0], l[1]))
                .ToArray();
            return new Polygon(new LinearRing(newCoords)) { SRID = 4326 };
        }

        public static Point ToLatLon(Point point)
        {
            if (point.SRID == 4326)
                return point;
            if (point.SRID != 0 && point.SRID != 3857)
                throw new InvalidSridException(new[] { 0, 4326, 3857 }, point.SRID);
            return new Point(ToLatLon(point.Coordinate)) { SRID = 4326 };
        }

        public static Coordinate ToLatLon(Coordinate point)
        {
            var x = point.X;
            var y = point.Y;

            MercatorToLatLonTransform.Transform(ref x, ref y);
            return new Coordinate(x, y);
        }

        #endregion
    }
}