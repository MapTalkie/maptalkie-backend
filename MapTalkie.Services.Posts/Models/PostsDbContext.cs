using Microsoft.EntityFrameworkCore;

namespace MapTalkie.Services.Posts.Models
{
    public class PostsDbContext : DbContext
    {
        public PostsDbContext(DbContextOptions<PostsDbContext> options) : base(options)
        {
        }

        public virtual DbSet<Post> Posts { get; set; } = default!;
        public virtual DbSet<PostLike> PostLikes { get; set; } = default!;
        public virtual DbSet<PostComment> PostComments { get; set; } = default!;
        public virtual DbSet<CommentLike> PostCommentLikes { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<PostLike>().HasKey(l => new { l.UserId, l.PostId });
            modelBuilder.Entity<PostComment>().HasIndex(c => c.PostId);
            modelBuilder.Entity<Post>().HasIndex(c => c.SharedId);
            modelBuilder.Entity<Post>().HasIndex(c => c.UserId);
        }
    }
}