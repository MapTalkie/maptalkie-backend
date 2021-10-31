using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Models.Context
{
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Attachment> Attachments { get; set; } = default!;
        public virtual DbSet<FriendRequest> FriendRequests { get; set; } = default!;
        public virtual DbSet<BlacklistedUser> BlacklistedUsers { get; set; } = default!;

        public virtual DbSet<MapPost> Posts { get; set; } = default!;
        public virtual DbSet<PostComment> PostComments { get; set; } = default!;
        public virtual DbSet<CommentReaction> PostCommentReactions { get; set; } = default!;


        public virtual DbSet<PrivateMessage> PrivateMessages { get; set; } = default!;
        public virtual DbSet<PrivateConversation> PrivateConversations { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<BlacklistedUser>().HasKey(bu => new { bu.BlacklistedById, bu.UserId });
            builder.Entity<FriendRequest>().HasKey(fr => new { fr.FromId, fr.ToId });
            builder.Entity<FriendRequest>().HasKey(fr => new { fr.FromId, fr.ToId });
            builder.Entity<CommentReaction>().HasKey(r => new { r.UserId, r.CommentId });
        }
    }
}