using System;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MapTalkie.Models
{
    [Owned]
    public class Location
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
    
    public class MapMessage
    {
        public long Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string UserId { get; set; } = default!;
        public User User { get; set; } = null!; // will be initialized by EF
        public Location Loc { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool Available { get; set; } = true;
        public bool IsOriginalLocation { get; set; } = true;
    }
}