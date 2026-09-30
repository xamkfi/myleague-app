using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddFloorballSeasonStandingRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StandingRules_DrawPoints",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "StandingRules_OvertimeLossPoints",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "StandingRules_OvertimeWinPoints",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "StandingRules_WinPoints",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true,
                defaultValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StandingRules_DrawPoints",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "StandingRules_OvertimeLossPoints",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "StandingRules_OvertimeWinPoints",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "StandingRules_WinPoints",
                schema: "floorball",
                table: "FloorballCompetitions");
        }
    }
}
