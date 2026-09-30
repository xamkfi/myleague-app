using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FootballDb
{
    /// <inheritdoc />
    public partial class AddFootballOvertimeStandingPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StandingRules_OvertimeLossPoints",
                schema: "football",
                table: "FootballCompetitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StandingRules_OvertimeWinPoints",
                schema: "football",
                table: "FootballCompetitions",
                type: "integer",
                nullable: false,
                defaultValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StandingRules_OvertimeLossPoints",
                schema: "football",
                table: "FootballCompetitions");

            migrationBuilder.DropColumn(
                name: "StandingRules_OvertimeWinPoints",
                schema: "football",
                table: "FootballCompetitions");
        }
    }
}
