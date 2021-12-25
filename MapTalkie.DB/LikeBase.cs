using System;
using System.Runtime.Serialization;

namespace MapTalkie.DB;

public class LikeBase
{
    public string UserId { get; set; } = string.Empty;
    [IgnoreDataMember] public User User { get; set; } = default!;
    [IgnoreDataMember] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}