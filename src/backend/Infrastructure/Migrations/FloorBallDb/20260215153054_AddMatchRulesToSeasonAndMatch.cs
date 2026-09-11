using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddMatchRulesToSeasonAndMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_AllowOvertime",
                schema: "floorball",
                table: "FloorballSeasons",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_AllowShootout",
                schema: "floorball",
                table: "FloorballSeasons",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_NumberOfPeriods",
                schema: "floorball",
                table: "FloorballSeasons",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_OvertimeDurationMinutes",
                schema: "floorball",
                table: "FloorballSeasons",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_PeriodDurationMinutes",
                schema: "floorball",
                table: "FloorballSeasons",
                type: "integer",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_AllowOvertime",
                schema: "floorball",
                table: "FloorballMatches",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_AllowShootout",
                schema: "floorball",
                table: "FloorballMatches",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_NumberOfPeriods",
                schema: "floorball",
                table: "FloorballMatches",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_OvertimeDurationMinutes",
                schema: "floorball",
                table: "FloorballMatches",
                type: "integer",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_PeriodDurationMinutes",
                schema: "floorball",
                table: "FloorballMatches",
                type: "integer",
                nullable: false,
                defaultValue: 15);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchRules_AllowOvertime",
                schema: "floorball",
                table: "FloorballSeasons");

            migrationBuilder.DropColumn(
                name: "MatchRules_AllowShootout",
                schema: "floorball",
                table: "FloorballSeasons");

            migrationBuilder.DropColumn(
                name: "MatchRules_NumberOfPeriods",
                schema: "floorball",
                table: "FloorballSeasons");

            migrationBuilder.DropColumn(
                name: "MatchRules_OvertimeDurationMinutes",
                schema: "floorball",
                table: "FloorballSeasons");

            migrationBuilder.DropColumn(
                name: "MatchRules_PeriodDurationMinutes",
                schema: "floorball",
                table: "FloorballSeasons");

            migrationBuilder.DropColumn(
                name: "MatchRules_AllowOvertime",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_AllowShootout",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_NumberOfPeriods",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_OvertimeDurationMinutes",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_PeriodDurationMinutes",
                schema: "floorball",
                table: "FloorballMatches");
        }
    }
}
