using System;
using System.Runtime.Serialization;
using NetTopologySuite.Geometries;
using Location = MapTalkie.Utils.Location;

namespace MapTalkie.Models
{
    public class MapPost
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public User User { get; set; } = null!; // will be initialized by EF
        [IgnoreDataMember] public Point Location { get; set; } = default!;
        public Location LatLon => Location;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = null;
        public bool Available { get; set; } = true;
        public bool IsOriginalLocation { get; set; } = true;
    }
}