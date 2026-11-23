using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class AddHockeyMissedPenaltyShot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The new event type uses existing columns only. Goals against average becomes the
            // goalie's own conceded goals per game played instead of per 60 minutes.
            migrationBuilder.Sql(
                """
                UPDATE hockey."HockeyGoalieCompetitionStatistics"
                SET "GoalsAgainstAverage" = CASE WHEN "GamesPlayed" > 0
                    THEN LEAST(99.99, ROUND("GoalsAgainst"::numeric / "GamesPlayed", 2)) ELSE 0 END;

                UPDATE hockey."HockeyGoalieMatchStatistics"
                SET "GoalsAgainstAverage" = CASE WHEN "GamesPlayed" > 0
                    THEN LEAST(99.99, ROUND("GoalsAgainst"::numeric / "GamesPlayed", 2)) ELSE 0 END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE hockey."HockeyGoalieCompetitionStatistics"
                SET "GoalsAgainstAverage" = CASE WHEN "MinutesPlayed" > 0
                    THEN LEAST(99.99, ROUND("GoalsAgainst"::numeric * 60 / "MinutesPlayed", 2)) ELSE 0 END;

                UPDATE hockey."HockeyGoalieMatchStatistics"
                SET "GoalsAgainstAverage" = CASE WHEN "MinutesPlayed" > 0
                    THEN LEAST(99.99, ROUND("GoalsAgainst"::numeric * 60 / "MinutesPlayed", 2)) ELSE 0 END;
                """);
        }
    }
}
