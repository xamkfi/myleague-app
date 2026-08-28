using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddTeamCategoryToCompetitionAndNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamCategory",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "text",
                nullable: false,
                defaultValue: "Adult");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballCompetitions_TeamCategory",
                schema: "floorball",
                table: "FloorballCompetitions",
                column: "TeamCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FloorballCompetitions_TeamCategory",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TeamCategory",
                schema: "floorball",
                table: "FloorballCompetitions");
        }
    }
}
