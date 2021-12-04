using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MapTalkie.Migrations
{
    public partial class BlacklistedUsersFunctions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blacklisted_users_users_blacklisted_by_id",
                table: "blacklisted_users");

            migrationBuilder.RenameColumn(
                name: "blacklisted_by_id",
                table: "blacklisted_users",
                newName: "blocked_by_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_blacklisted_users_users_blocked_by_user_id",
                table: "blacklisted_users",
                column: "blocked_by_user_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_blacklisted_users_users_blocked_by_user_id",
                table: "blacklisted_users");

            migrationBuilder.RenameColumn(
                name: "blocked_by_user_id",
                table: "blacklisted_users",
                newName: "blacklisted_by_id");

            migrationBuilder.AddForeignKey(
                name: "fk_blacklisted_users_users_blacklisted_by_id",
                table: "blacklisted_users",
                column: "blacklisted_by_id",
                principalTable: "asp_net_users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
