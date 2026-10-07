using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class BackfillHockeyPeriodGoals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: Do not drop HockeyVideoReviewRules* truncated owned-type shadow columns here.
            // Period rows were created but never counted. Recount goals (EventType 1) per period and side.
            migrationBuilder.Sql(
                """
                UPDATE hockey."HockeyPeriodScores" AS ps
                SET "HomeGoals" = (
                        SELECT COUNT(*)
                        FROM hockey."HockeyMatchEvents" AS e
                        WHERE e."MatchId" = ps."MatchId"
                          AND e."EventType" = 1
                          AND e."PeriodNumber" = ps."PeriodNumber"
                          AND e."ScoringMatchTeamId" = ps."HomeMatchTeamId"),
                    "AwayGoals" = (
                        SELECT COUNT(*)
                        FROM hockey."HockeyMatchEvents" AS e
                        WHERE e."MatchId" = ps."MatchId"
                          AND e."EventType" = 1
                          AND e."PeriodNumber" = ps."PeriodNumber"
                          AND e."ScoringMatchTeamId" = ps."AwayMatchTeamId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // The counts are derived from goal events; nothing to restore.
        }
    }
}
