using System;
using System.Runtime.Serialization;
using NetTopologySuite.Geometries;

namespace MapTalkie.Common.Utils
{
    public class LocationDescriptor
    {
        private AreaId? _areaId;

        private Coordinate? _latLon;
        public double X { get; set; }
        public double Y { get; set; }

        [IgnoreDataMember]
        public AreaId AreaId => _areaId ?? (AreaId)(_areaId = AreaId.FromCoordinate3857(new Coordinate(X, Y)));

        [IgnoreDataMember] public double Latitude => GetLatLon().Y;
        [IgnoreDataMember] public double Longitude => GetLatLon().X;

        private Coordinate GetLatLon()
        {
            if (_latLon == null)
                _latLon = MapConvert.ToLatLon(new Coordinate(X, Y));
            return _latLon;
        }

        public static explicit operator Coordinate(LocationDescriptor locationDescriptor)
        {
            return new Coordinate(locationDescriptor.X, locationDescriptor.Y);
        }

        public static explicit operator Point(LocationDescriptor locationDescriptor)
        {
            return new Point(locationDescriptor.X, locationDescriptor.Y) { SRID = 3857 };
        }

        public static explicit operator LocationDescriptor(Point point)
        {
            try
            {
                var coordinate = MapConvert.ToMercator(point.Coordinate);
                return new LocationDescriptor { X = coordinate.X, Y = coordinate.Y };
            }
            catch (Exception e)
            {
                throw new InvalidCastException(
                    "Failed to cast Point as LocationDescriptor, see inner exception for detail", e);
            }
        }

        public static implicit operator LocationDescriptor(Coordinate point)
        {
            return new LocationDescriptor { X = point.X, Y = point.Y };
        }
    }
}