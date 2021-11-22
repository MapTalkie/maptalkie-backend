using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using NetTopologySuite.Geometries;

namespace MapTalkieCommon.Utils
{
    public struct ZoneId
    {
        public int Level { get; set; }
        public int Index { get; set; }

        [IgnoreDataMember] public string Id => $"{Level}:{Index}";

        [IgnoreDataMember]
        private int CellX
        {
            get
            {
                if (Level == 0)
                    return 0;
                var m = 2 << (Level - 1);
                return Index - Index / m * m;
            }
        }

        [IgnoreDataMember] private int CellY => Level > 0 ? Index / (2 << (Level - 1)) : 0;

        private ZoneId(int level, int index)
        {
            Level = level;
            Index = index;
        }

        public Polygon Polygon()
        {
            IList<double[]> transformedPoints;

            {
                var points = new List<double[]>(4);
                var cellsPerSize = Math.Pow(2, Level);
                var cellSize = MapConstants.MercatorAreaRadius * 2 / cellsPerSize;
                points.Add(new[]
                {
                    -MapConstants.MercatorAreaRadius + cellSize * CellX,
                    -MapConstants.MercatorAreaRadius + cellSize * CellY
                }); // bottom left
                points.Add(new[]
                {
                    -MapConstants.MercatorAreaRadius + cellSize * CellX,
                    -MapConstants.MercatorAreaRadius + cellSize * CellY + cellSize
                }); // top left
                points.Add(new[]
                {
                    -MapConstants.MercatorAreaRadius + cellSize * CellX + cellSize,
                    -MapConstants.MercatorAreaRadius + cellSize * CellY + cellSize
                }); // top right
                points.Add(new[]
                {
                    -MapConstants.MercatorAreaRadius + cellSize * CellX + cellSize,
                    -MapConstants.MercatorAreaRadius + cellSize * CellY
                }); // bottom right
                transformedPoints = MapConvert.MercatorToLatLonTransform.TransformList(points);
            }

            var coords = new Coordinate[5];

            for (var i = 0; i < 4; i++)
                coords[i] = new Coordinate(transformedPoints[i][0], transformedPoints[i][1]);
            coords[4] = coords[0];

            return new Polygon(new LinearRing(coords));
        }

        #region Статические методы

        private static IEnumerable<ZoneId> AllFromCoordinate3857(Coordinate c)
        {
            // areaSize = radius а не radius * 2 потому что в цикле мы делаем yield начиная со второго 
            // (индекс 1) уровня, т. е. изначально размер зоны равен половине всего мира, т. е. радиусу
            double areaSize = MapConstants.MercatorAreaRadius, cx = 0, cy = 0;
            int level = 0, areaId = 0, x = 0, y = 0, multiplier = 1;

            yield return new ZoneId(0, 0);

            while (areaSize >= MapConstants.MinAreaSize)
            {
                level++;
                multiplier *= 2;
                var x2 = 2 * x + 1;
                var y2 = 2 * y + 1;

                if (c.X < cx)
                {
                    x2 -= 1;
                    cx -= MapConstants.MercatorAreaRadius / multiplier;
                }
                else
                {
                    cx += MapConstants.MercatorAreaRadius / multiplier;
                }

                if (c.Y < cy)
                {
                    y2 -= 1;
                    cy -= MapConstants.MercatorAreaRadius / multiplier;
                }
                else
                {
                    cy += MapConstants.MercatorAreaRadius / multiplier;
                }

                areaId = x2 + y2 * multiplier;
                x = x2;
                y = y2;
                areaSize /= 2;

                yield return new ZoneId(level, areaId);
            }
        }

        public static IEnumerable<ZoneId> AllFromPoint(Point point, double minAreaSize)
            => AllFromCoordinate3857(To3857OrThrow(point));

        public static ZoneId FromPoint(Point point, int? level)
            => FromCoordinate3857(To3857OrThrow(point), level);

        public static ZoneId FromCoordinate3857(Coordinate coordinate, int? level = null)
        {
            if (level == null)
                return AllFromCoordinate3857(coordinate).Last();
            var lvl = (int)level;
            foreach (var zone in AllFromCoordinate3857(coordinate))
            {
                lvl--;
                if (lvl < 0)
                    return zone;
            }

            return new ZoneId(0, 0);
        }

        private static ZoneId FromEnvelope3857(Envelope envelope)
        {
            var size = Math.Max(envelope.Height, envelope.Width);
            var level = (int)Math.Ceiling(Math.Log(MapConstants.MercatorAreaRadius * 2 / size, 2));
            return FromCoordinate3857(envelope.Centre, level);
        }

        public static ZoneId FromPolygon(Polygon polygon)
            => FromEnvelope3857(GetMercatorEnvelope(polygon));

        private static Coordinate To3857OrThrow(Point point)
        {
            if (point.SRID == 3857)
                return point.Coordinate;

            if (point.SRID == 0 || point.SRID == 4326)
                return MapConvert.ToMercator(point.Coordinate);

            throw new InvalidSridException(new[] { 0, 4326, 3857 }, point.SRID);
        }

        private static Envelope GetMercatorEnvelope(Polygon polygon)
        {
            var envelope = polygon.EnvelopeInternal;
            if (polygon.SRID == 3857)
                return envelope;

            if (polygon.SRID != 4326 && polygon.SRID != 0)
                throw new InvalidSridException(new[] { 0, 4326, 3857 }, polygon.SRID);

            IList<double[]> points = new List<double[]>
            {
                new[] { envelope.MaxX, envelope.MaxY },
                new[] { envelope.MaxX, envelope.MinY },
                new[] { envelope.MinX, envelope.MinY },
                new[] { envelope.MinX, envelope.MaxY },
            };
            points = MapConvert.LatLonToMercatorTransform.TransformList(points);
            var maxX = points.Max(p => p[0]);
            var maxY = points.Max(p => p[1]);
            var minX = points.Min(p => p[0]);
            var minY = points.Min(p => p[1]);
            return new Envelope(minX, maxX, minY, maxY);
        }

        #endregion

        public static bool IsSameArea(Polygon poly1, Polygon poly2)
        {
            return FromPolygon(poly1).Id == FromPolygon(poly2).Id;
        }
    }
}