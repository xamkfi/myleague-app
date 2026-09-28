using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddSeasonStandingsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RankingCriteria",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                defaultValueSql: "'0,1,2,4,5'");

            migrationBuilder.AddColumn<int>(
                name: "TeamsAdvancing",
                schema: "floorball",
                table: "FloorballCompetitions",
                type: "integer",
                nullable: true,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RankingCriteria",
                schema: "floorball",
                table: "FloorballCompetitions");

            migrationBuilder.DropColumn(
                name: "TeamsAdvancing",
                schema: "floorball",
                table: "FloorballCompetitions");
        }
    }
}
