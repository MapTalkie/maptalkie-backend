using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using ProjNet.CoordinateSystems.Transformations;

namespace MapTalkie.Utils.MapUtils
{
    public static class MapUtils
    {
        private static readonly ICoordinateTransformation Wgs84ToMercator = new CoordinateTransformationFactory()
            .CreateFromCoordinateSystems(
                ProjNet.CoordinateSystems.GeographicCoordinateSystem.WGS84,
                ProjNet.CoordinateSystems.ProjectedCoordinateSystem.WebMercator);

        private static readonly MathTransform Wgs84ToMercatorTransform = Wgs84ToMercator.MathTransform;
        private static readonly MathTransform MercatorToWgs86Transform = Wgs84ToMercatorTransform.Inverse();

        public static readonly double MercatorAreaRadius = 20048967;

        public static Polygon GetZonePolygon(MapZoneDescriptor descriptor)
        {
            IList<double[]> transformedPoints;

            {
                var points = new List<double[]>(4);
                var cellsPerSize = Math.Pow(2, descriptor.Level);
                var cellSize = MercatorAreaRadius * 2 / cellsPerSize;
                points.Add(new[]
                {
                    -MercatorAreaRadius + cellSize * descriptor.CellX,
                    -MercatorAreaRadius + cellSize * descriptor.CellY,
                }); // bottom left
                points.Add(new[]
                {
                    -MercatorAreaRadius + cellSize * descriptor.CellX,
                    -MercatorAreaRadius + cellSize * descriptor.CellY + cellSize,
                }); // top left
                points.Add(new[]
                {
                    -MercatorAreaRadius + cellSize * descriptor.CellX + cellSize,
                    -MercatorAreaRadius + cellSize * descriptor.CellY + cellSize,
                }); // top right
                points.Add(new[]
                {
                    -MercatorAreaRadius + cellSize * descriptor.CellX + cellSize,
                    -MercatorAreaRadius + cellSize * descriptor.CellY,
                }); // bottom right
                transformedPoints = MercatorToWgs86Transform.TransformList(points);
            }

            var coords = new Coordinate[5];

            for (var i = 0; i < 4; i++)
                coords[i] = new Coordinate(transformedPoints[i][0], transformedPoints[i][1]);
            coords[4] = coords[0];

            return new Polygon(new LinearRing(coords));
        }

        public static void ThrowIfInvalidSRID(Point point, int requiredSRID)
        {
            if (point.SRID != requiredSRID)
                throw new InvalidOperationException("Invalid SRID of point: SRID of the point must be 4326");
        }

        public static void ThrowIfNot4326(Point point) => ThrowIfInvalidSRID(point, 4326);

        public static void ThrowIfNot3857(Point point) => ThrowIfInvalidSRID(point, 3857);

        #region Конвертация проекции меркатор и WGS84 (SRID 4326)

        public static Point LatLonToMercator(Point point)
        {
            ThrowIfNot4326(point);
            return new Point(LatLonToMercator(point.Coordinate)) { SRID = 3857 };
        }

        public static Coordinate LatLonToMercator(Coordinate point)
        {
            double x = point.X;
            double y = point.Y;
            Wgs84ToMercatorTransform.Transform(ref x, ref y);
            return new Coordinate(x, y);
        }

        public static Point MercatorToLatLon(Point point)
        {
            ThrowIfNot3857(point);
            return new Point(MercatorToLatLon(point.Coordinate)) { SRID = 4326 };
        }

        public static Coordinate MercatorToLatLon(Coordinate point)
        {
            double x = point.X;
            double y = point.Y;

            MercatorToWgs86Transform.Transform(ref x, ref y);
            return new Coordinate(x, y);
        }

        #endregion

        #region Разбиение на зоны

        private const int MinAreaSize = 2000;

        private static IEnumerable<MapZoneDescriptor> EnumerateZoneIds(double px, double py)
        {
            // areaSize = radius а не radius * 2 потому что в цикле мы делаем yield начиная со второго 
            // (индекс 1) уровня, т. е. изначально размер зоны равен половине всего мира, т. е. радиусу
            double areaSize = MercatorAreaRadius, cx = 0, cy = 0;
            int level = 0, areaId = 0, x = 0, y = 0, multiplier = 1;

            yield return new MapZoneDescriptor { Level = 0, Index = 0, CellX = 0, CellY = 0 };

            while (areaSize >= MinAreaSize)
            {
                level++;
                multiplier *= 2;
                int x2 = 2 * x + 1;
                int y2 = 2 * y + 1;

                if (px < cx)
                {
                    x2 -= 1;
                    cx -= MercatorAreaRadius / multiplier;
                }
                else
                {
                    cx += MercatorAreaRadius / multiplier;
                }

                if (py < cy)
                {
                    y2 -= 1;
                    cy -= MercatorAreaRadius / multiplier;
                }
                else
                {
                    cy += MercatorAreaRadius / multiplier;
                }

                areaId = x2 + y2 * multiplier;
                x = x2;
                y = y2;
                areaSize /= 2;

                yield return new MapZoneDescriptor { Index = areaId, Level = level, CellX = x2, CellY = y2 };
            }
        }

        public static List<MapZoneDescriptor> GetZones(Point point)
        {
            ThrowIfNot3857(point);
            return EnumerateZoneIds(point.X, point.Y).ToList();
        }

        public static MapZoneDescriptor GetZone(Coordinate point, int level)
        {
            if (level < 0)
            {
                throw new ArgumentException($"{nameof(level)} cannot be less than 0");
            }

            var descriptor = EnumerateZoneIds(point.X, point.Y).Take(level + 1).Last();
            return descriptor;
        }

        public static MapZoneDescriptor GetZone(Polygon polygon)
        {
            var envelope = polygon.EnvelopeInternal;
            var size = Math.Max(envelope.Height, envelope.Width);
            var level = (int)Math.Ceiling(Math.Log(MercatorAreaRadius * 2 / size, 2));
            return GetZone(envelope.Centre, level);
        }

        public static double ZoneSize(int level)
        {
            return MercatorAreaRadius * 2 / Math.Pow(2, level);
        }

        public static IEnumerable<MapZoneDescriptor> AllZones()
        {
            var level = 0;
            var areaSize = MercatorAreaRadius * 2;
            int multiplier = 1, cellsOnLevel = 1;

            while (areaSize >= MinAreaSize)
            {
                for (var cellIndex = 0; cellIndex < cellsOnLevel; cellIndex++)
                    yield return new MapZoneDescriptor
                    {
                        Level = level,
                        Index = cellIndex,
                        CellY = cellIndex / multiplier,
                        CellX = cellIndex - (cellIndex / multiplier) * multiplier
                    };
                areaSize /= 2;
                level++;
                multiplier *= 2;
                cellsOnLevel *= 4;
            }
        }

        #endregion
    }
}