using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Models.Context
{
    public class AppDbContext : IdentityDbContext<User, Role, string>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<MapMessage> Messages { get; set; } = default!;
        
        public virtual DbSet<Attachment> Attachments { get; set; } = default!;
    }
}