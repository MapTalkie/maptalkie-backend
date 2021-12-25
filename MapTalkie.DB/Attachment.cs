using System;

namespace MapTalkie.DB;

public class Attachment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public User User { get; set; } = default!;
}