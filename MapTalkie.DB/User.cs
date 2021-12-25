using System;
using Microsoft.AspNetCore.Identity;

namespace MapTalkie.DB;

public class User : IdentityUser<string>
{
    public User()
    {
        Id = Nanoid.Nanoid.Generate();
        SecurityStamp = Guid.NewGuid().ToString();
    }

    public bool UsesPrivateLocation { get; set; }
    public bool AllowsNonFriendMessages { get; set; }
}