using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FootballDb
{
    /// <inheritdoc />
    public partial class AddSeasonStandingsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RankingCriteria",
                schema: "football",
                table: "FootballCompetitions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                defaultValueSql: "'0,1,2,4,5'");

            migrationBuilder.AddColumn<int>(
                name: "TeamsAdvancing",
                schema: "football",
                table: "FootballCompetitions",
                type: "integer",
                nullable: true,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RankingCriteria",
                schema: "football",
                table: "FootballCompetitions");

            migrationBuilder.DropColumn(
                name: "TeamsAdvancing",
                schema: "football",
                table: "FootballCompetitions");
        }
    }
}
