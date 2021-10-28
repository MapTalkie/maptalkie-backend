using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Models
{
    [Owned]
    public class PrivacySettings
    {
        public bool PrivateLocation { get; set; } = false;

        public bool NonFriendMessages { get; set; } = false;
    }
    
    [Owned]
    public class UserSettings
    {
        public PrivacySettings Privacy { get; set; } = new PrivacySettings();
    }
    
    public class User : IdentityUser
    {
        public UserSettings Settings { get; set; } = new();
    }
}