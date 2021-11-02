using System;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;

namespace MapTalkie.Utils
{
    public static class MapUtils
    {
        private static ICoordinateTransformation FromWGS84ToWebMercator = new CoordinateTransformationFactory()
            .CreateFromCoordinateSystems(
                ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84,
                ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator);

        private static double MAX_SIZE_X = 20037509;
        private static double MAX_SIZE_Y = 20048967;
        private static int MAX_SIZE_POW = 25;
        private static string GLOBAL = "GLOBAL";

        public static string AreaId(Point point, int level)
        {
            if (level == 0)
                return "AREA(0:0:0)";
            double x = Math.Max(Math.Min(point.X, 180), -180);
            double y = Math.Max(Math.Min(point.Y, 85.06), -85.06);
            FromWGS84ToWebMercator.MathTransform.Transform(ref x, ref y);

            x = Math.Max(Math.Min(x, MAX_SIZE_X), -MAX_SIZE_X);
            y = Math.Max(Math.Min(y, MAX_SIZE_Y), -MAX_SIZE_Y);

            level = (int)Math.Pow(2, level - 1);

            y = Math.Floor(y / (MAX_SIZE_Y / level));
            x = Math.Floor(x / (MAX_SIZE_X / level));

            long xLong = (long)x + level;
            long yLong = (long)y + level;

            return $"AREA({level}:{xLong}:{yLong})";
        }
    }
}