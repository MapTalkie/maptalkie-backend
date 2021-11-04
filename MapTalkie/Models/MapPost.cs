using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MapTalkie.Utils;
using NetTopologySuite.Geometries;

namespace MapTalkie.Models
{
    public class MapPost
    {
        private Point? _mercatorLocation;

        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!; // will be initialized by EF
        public Point Location { get; set; } = default!;

        [IgnoreDataMember]
        public Point MercatorLocation
        {
            get
            {
                if (_mercatorLocation == null)
                    _mercatorLocation = MapUtilities.LatLonToMercator(Location);
                return _mercatorLocation;
            }
            set
            {
                // TODO проверить, не выходит ли точка за пределы
                Location = MapUtilities.LatLonToMercator(value);
                _mercatorLocation = value;
            }
        }


        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public bool Available { get; set; } = true;
        public bool IsOriginalLocation { get; set; } = true;

        public ICollection<PostComment> Comments { get; set; } = default!;
        public ICollection<PostHeart> Hearts { get; set; } = default!;
    }
}