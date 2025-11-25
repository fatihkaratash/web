using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitMindAI.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAiRecommendationEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiRecommendations_Members_MemberId",
                table: "AiRecommendations");

            migrationBuilder.RenameColumn(
                name: "ResultText",
                table: "AiRecommendations",
                newName: "OutputText");

            migrationBuilder.RenameColumn(
                name: "Model",
                table: "AiRecommendations",
                newName: "ModelName");

            migrationBuilder.RenameColumn(
                name: "InputText",
                table: "AiRecommendations",
                newName: "InputSummary");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "AiRecommendations",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Goal",
                table: "AiRecommendations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_AiRecommendations_Members_MemberId",
                table: "AiRecommendations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AiRecommendations_Members_MemberId",
                table: "AiRecommendations");

            migrationBuilder.DropColumn(
                name: "Goal",
                table: "AiRecommendations");

            migrationBuilder.RenameColumn(
                name: "OutputText",
                table: "AiRecommendations",
                newName: "ResultText");

            migrationBuilder.RenameColumn(
                name: "ModelName",
                table: "AiRecommendations",
                newName: "Model");

            migrationBuilder.RenameColumn(
                name: "InputSummary",
                table: "AiRecommendations",
                newName: "InputText");

            migrationBuilder.AlterColumn<int>(
                name: "MemberId",
                table: "AiRecommendations",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_AiRecommendations_Members_MemberId",
                table: "AiRecommendations",
                column: "MemberId",
                principalTable: "Members",
                principalColumn: "Id");
        }
    }
}
