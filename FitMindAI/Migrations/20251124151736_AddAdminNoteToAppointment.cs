using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMindAI.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminNoteToAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "Appointments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "Appointments");
        }
    }
}
