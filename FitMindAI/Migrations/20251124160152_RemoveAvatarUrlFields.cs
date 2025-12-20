using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMindAI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAvatarUrlFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Trainers");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Members");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Trainers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Members",
                type: "text",
                nullable: true);
        }
    }
}
