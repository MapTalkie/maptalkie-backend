using System;
using System.Runtime.Serialization;

namespace MapTalkie.DB;

public class RefreshToken
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    public DateTime ExpiresAt { get; set; }
    public bool IsBlocked { get; set; }
    [IgnoreDataMember] public User User { get; set; } = default!;
    public string UserId { get; set; } = default!;
}