using MapTalkie.DB.Utils;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace MapTalkie.DB.Context
{
    public class AppDbContext : IdentityDbContext<User, IdentityRole, string>
    {
        private readonly ILoggerFactory _loggerFactory;

        public AppDbContext()
        {
            _loggerFactory = new NullLoggerFactory();
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            _loggerFactory = new NullLoggerFactory();
        }

        public AppDbContext(
            DbContextOptions<AppDbContext> options,
            ILoggerFactory loggerFactory) : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        public virtual DbSet<Attachment> Attachments { get; set; } = default!;
        public virtual DbSet<BlacklistedUser> BlacklistedUsers { get; set; }
        public virtual DbSet<FriendRequest> FriendRequests { get; set; } = default!;
        public virtual DbSet<Post> Posts { get; set; } = default!;
        public virtual DbSet<PostLike> PostLikes { get; set; } = default!;
        public virtual DbSet<PostComment> PostComments { get; set; } = default!;
        public virtual DbSet<PostCommentLike> PostCommentLikes { get; set; } = default!;
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; } = default!;
        public virtual DbSet<PrivateMessage> PrivateMessages { get; set; } = default!;
        public virtual DbSet<PrivateMessageReceipt> PrivateMessageReceipts { get; set; } = default!;
        public virtual DbSet<PrivateConversationParticipant> PrivateConversationParticipants { get; set; } = default!;


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<User>().ToTable("asp_net_users");
            builder.Entity<IdentityUserToken<string>>().ToTable("asp_net_user_tokens");
            builder.Entity<IdentityUserLogin<string>>().ToTable("asp_net_user_logins");
            builder.Entity<IdentityUserClaim<string>>().ToTable("asp_net_user_claims");
            builder.Entity<IdentityRole>().ToTable("asp_net_roles");
            builder.Entity<IdentityUserRole<string>>().ToTable("asp_net_user_roles");
            builder.Entity<IdentityRoleClaim<string>>().ToTable("asp_net_role_claims");

            builder.HasPostgresExtension("postgis");

            {
                var post = builder.Entity<Post>();

                post.Property(p => p.Id).HasValueGenerator<IdGenValueGenerator>();

                post.HasMany(p => p.Shares)
                    .WithOne(p => p.Shared!);
                post.Property(p => p.Location)
                    .HasColumnType("geometry(point)");
            }

            // idgen
            builder.Entity<PrivateMessage>().Property(m => m.Id).HasValueGenerator<IdGenValueGenerator>();
            builder.Entity<PostComment>().Property(c => c.Id).HasValueGenerator<IdGenValueGenerator>();

            // первичные ключи для таблиц
            builder.Entity<BlacklistedUser>().HasKey(fr => new { fr.BlockedByUserId, fr.UserId });
            builder.Entity<FriendRequest>().HasKey(fr => new { fr.FromId, fr.ToId });
            builder.Entity<PostCommentLike>().HasKey(r => new { r.UserId, r.CommentId });
            builder.Entity<PostLike>().HasKey(r => new { r.UserId, r.PostId });
            builder.Entity<PrivateMessageReceipt>().HasKey(r => new { r.UserIdA, r.UserIdB, r.MessageId });
            builder.Entity<PrivateConversationParticipant>().HasKey(r => new { r.SenderId, r.RecipientId });

            builder.Entity<PostComment>()
                .HasMany(c => c.Comments)
                .WithOne(nameof(PostComment.ReplyTo))
                .HasForeignKey(c => c.ReplyToId);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(_loggerFactory);
            optionsBuilder.UseSnakeCaseNamingConvention();
        }
    }
}