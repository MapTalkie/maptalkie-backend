using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MapTalkieDB.Context
{
    public class AppDbContext : IdentityDbContext<User, Role, string>
    {
        private readonly ILoggerFactory _loggerFactory;

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ILoggerFactory loggerFactory) : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        public virtual DbSet<Attachment> Attachments { get; set; } = default!;
        public virtual DbSet<FriendRequest> FriendRequests { get; set; } = default!;
        public virtual DbSet<BlacklistedUser> BlacklistedUsers { get; set; } = default!;
        public virtual DbSet<Post> Posts { get; set; } = default!;
        public virtual DbSet<PostLike> PostLikes { get; set; } = default!;
        public virtual DbSet<PostComment> PostComments { get; set; } = default!;
        public virtual DbSet<CommentLike> PostCommentLikes { get; set; } = default!;
        public virtual DbSet<PostCluster> PostClusters { get; set; } = default!;


        public virtual DbSet<PrivateMessage> PrivateMessages { get; set; } = default!;
        public virtual DbSet<PrivateConversation> PrivateConversations { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.HasPostgresExtension("postgis");

            builder.Entity<Post>()
                .Property(p => p.Location)
                .HasColumnType("geometry(point)");

            builder.Entity<Post>()
                .HasMany(p => p.Shares)
                .WithOne(p => p.Shared!);

            builder.Entity<BlacklistedUser>().HasKey(bu => new { bu.BlacklistedById, bu.UserId });
            builder.Entity<FriendRequest>().HasKey(fr => new { fr.FromId, fr.ToId });
            builder.Entity<FriendRequest>().HasKey(fr => new { fr.FromId, fr.ToId });
            builder.Entity<CommentLike>().HasKey(r => new { r.UserId, r.CommentId });
            builder.Entity<PostLike>().HasKey(r => new { r.UserId, r.PostId });

            builder.Entity<PostComment>()
                .HasMany(c => c.Comments)
                .WithOne(c => c.ReplyTo)
                .HasForeignKey(c => c.ReplyToId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(_loggerFactory);
        }
    }
}