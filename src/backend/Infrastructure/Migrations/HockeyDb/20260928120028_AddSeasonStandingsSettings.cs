using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class AddSeasonStandingsSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RankingCriteria",
                schema: "hockey",
                table: "HockeyCompetitions",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true,
                defaultValueSql: "'0,1,2,4,5'");

            migrationBuilder.AddColumn<int>(
                name: "TeamsAdvancing",
                schema: "hockey",
                table: "HockeyCompetitions",
                type: "integer",
                nullable: true,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RankingCriteria",
                schema: "hockey",
                table: "HockeyCompetitions");

            migrationBuilder.DropColumn(
                name: "TeamsAdvancing",
                schema: "hockey",
                table: "HockeyCompetitions");
        }
    }
}
