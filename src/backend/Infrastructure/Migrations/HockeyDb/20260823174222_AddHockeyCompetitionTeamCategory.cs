using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class AddHockeyCompetitionTeamCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamCategory",
                schema: "hockey",
                table: "HockeyCompetitions",
                type: "text",
                nullable: false,
                defaultValue: "Adult");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyCompetitions_TeamCategory",
                schema: "hockey",
                table: "HockeyCompetitions",
                column: "TeamCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_HockeyCompetitions_TeamCategory",
                schema: "hockey",
                table: "HockeyCompetitions");

            migrationBuilder.DropColumn(
                name: "TeamCategory",
                schema: "hockey",
                table: "HockeyCompetitions");
        }
    }
}
