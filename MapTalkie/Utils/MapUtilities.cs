using System;
using MapTalkie.Utils.MapUtils;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;

namespace MapTalkie.Utils
{
    public static class MapUtilities
    {
        private static ICoordinateTransformation _wgs84ToMercator = new CoordinateTransformationFactory()
            .CreateFromCoordinateSystems(
                ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84,
                ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator);

        private static MathTransform _wgs84ToMercatorTransform = _wgs84ToMercator.MathTransform;
        private static MathTransform _mercatorToWgs86Transform = _wgs84ToMercatorTransform.Inverse();

        private static double MAX_SIZE_X = 20037509;
        private static double MAX_SIZE_Y = 20048967;
        private static int MAX_SIZE_POW = 25;

        public static AreaId GetAreaId(Point point, int level)
        {
            if (level == 0)
                return AreaId.Global;
            double x = Math.Max(Math.Min(point.X, 180), -180);
            double y = Math.Max(Math.Min(point.Y, 85.06), -85.06);
            _wgs84ToMercator.MathTransform.Transform(ref x, ref y);

            x = Math.Max(Math.Min(x, MAX_SIZE_X), -MAX_SIZE_X);
            y = Math.Max(Math.Min(y, MAX_SIZE_Y), -MAX_SIZE_Y);

            level = (int)Math.Pow(2, level - 1);

            y = Math.Floor(y / (MAX_SIZE_Y / level));
            x = Math.Floor(x / (MAX_SIZE_X / level));

            int xInt = (int)x + level;
            int yInt = (int)y + level;

            return new AreaId(level, xInt, yInt);
        }


        public static Point LatLonToMercator(Point point)
        {
            double x = point.X;
            double y = point.Y;
            _wgs84ToMercatorTransform.Transform(ref x, ref y);
            return new Point(x, y) { SRID = 3857 };
        }

        public static Point MercatorToLatLon(Point point)
        {
            ThrowIfNot3857(point);
            double x = point.X;
            double y = point.Y;
            _mercatorToWgs86Transform.Transform(ref x, ref y);
            return new Point(x, y) { SRID = 4326 };
        }

        public static void ThrowIfInvalidSRID(Point point, int requiredSRID)
        {
            if (point.SRID != requiredSRID)
                throw new InvalidOperationException("Invalid SRID of point: SRID of the point must be 4326");
        }

        public static void ThrowIfNot4326(Point point) => ThrowIfInvalidSRID(point, 4326);
        public static void ThrowIfNot3857(Point point) => ThrowIfInvalidSRID(point, 3857);
    }
}