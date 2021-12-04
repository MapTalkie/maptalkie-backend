using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MapTalkie.Migrations
{
    public partial class PostCommentLike : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cached_likes_count",
                table: "post_comments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<float>(
                name: "rank_decay_factor",
                table: "post_comments",
                type: "double precision",
                nullable: false,
                defaultValue: 0f);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cached_likes_count",
                table: "post_comments");

            migrationBuilder.DropColumn(
                name: "rank_decay_factor",
                table: "post_comments");
        }
    }
}
