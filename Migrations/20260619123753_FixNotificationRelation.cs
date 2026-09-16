using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taxiiii.Migrations
{
    /// <inheritdoc />
    public partial class FixNotificationRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_RigesterUsers_RigesterUserUserId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_RigesterUserUserId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RigesterUserUserId",
                table: "Notifications");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RigesterUserUserId",
                table: "Notifications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RigesterUserUserId",
                table: "Notifications",
                column: "RigesterUserUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_RigesterUsers_RigesterUserUserId",
                table: "Notifications",
                column: "RigesterUserUserId",
                principalTable: "RigesterUsers",
                principalColumn: "UserId");
        }
    }
}
