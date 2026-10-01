using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class SetHockeyMinDressedPlayersToZero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: Do not drop HockeyVideoReviewRules* truncated owned-type shadow columns here.
            migrationBuilder.Sql(
                """
                UPDATE hockey."HockeyCompetitions"
                SET "CompetitionRules_Roster_MinDressedPlayers" = 0
                WHERE "CompetitionRules_Roster_MinDressedPlayers" > 0;

                UPDATE hockey."HockeyCompetitionDivisions"
                SET "RulesOverride_Roster_MinDressedPlayers" = 0
                WHERE "RulesOverride_Roster_MinDressedPlayers" > 0;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Previous per-competition minimums are not recoverable.
        }
    }
}
