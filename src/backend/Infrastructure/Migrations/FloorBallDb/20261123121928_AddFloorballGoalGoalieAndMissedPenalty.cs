using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddFloorballGoalGoalieAndMissedPenalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ShooterPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WasDelayedPenalty",
                schema: "floorball",
                table: "FloorballMatchEvents",
                type: "boolean",
                nullable: true,
                defaultValue: false);

            // Charge existing goals to the goalie on record for the match. Own goals (5), empty-net
            // goals (3) and shootout goals (7, or scored in the shootout period) are charged to no one.
            migrationBuilder.Sql(
                """
                UPDATE floorball."FloorballMatchEvents" AS e
                SET "GoalieId" = CASE WHEN e."TeamId" = m."HomeTeamId"
                        THEN m."AwayActiveGoalieId" ELSE m."HomeActiveGoalieId" END
                FROM floorball."FloorballMatches" AS m
                WHERE m."Id" = e."MatchId"
                  AND e."EventType" = 'Goal'
                  AND COALESCE(e."GoalType", 0) NOT IN (3, 5, 7)
                  AND e."PeriodNumber" <> m."MatchRules_NumberOfPeriods" + 2;
                """);

            // Empty-net and shootout goals were counted against the goalie. Take them back.
            migrationBuilder.Sql(
                """
                UPDATE floorball."FloorballGoalieSeasonStatistics" AS s
                SET "GoalsAgainst" = GREATEST(0, s."GoalsAgainst" - x.goals),
                    "ShotsAgainst" = GREATEST(0, s."ShotsAgainst" - x.goals)
                FROM (
                    SELECT CASE WHEN e."TeamId" = m."HomeTeamId"
                               THEN m."AwayActiveGoalieId" ELSE m."HomeActiveGoalieId" END AS goalie_id,
                           CASE WHEN e."TeamId" = m."HomeTeamId"
                               THEN m."AwayTeamId" ELSE m."HomeTeamId" END AS team_id,
                           m."CompetitionId" AS competition_id,
                           COUNT(*)::int AS goals
                    FROM floorball."FloorballMatchEvents" AS e
                    JOIN floorball."FloorballMatches" AS m ON m."Id" = e."MatchId"
                    WHERE e."EventType" = 'Goal'
                      AND COALESCE(e."GoalType", 0) <> 5
                      AND (COALESCE(e."GoalType", 0) IN (3, 7)
                           OR e."PeriodNumber" = m."MatchRules_NumberOfPeriods" + 2)
                    GROUP BY 1, 2, 3
                ) AS x
                WHERE s."PlayerId" = x.goalie_id
                  AND s."TeamId" = x.team_id
                  AND s."CompetitionId" = x.competition_id;
                """);

            // A goalie who shared a match now also has the match in games played.
            migrationBuilder.Sql(
                """
                UPDATE floorball."FloorballGoalieSeasonStatistics" AS s
                SET "GamesPlayed" = GREATEST(s."GamesPlayed", x.games)
                FROM (
                    SELECT g.goalie_id, g.team_id, g.competition_id, COUNT(DISTINCT g.match_id)::int AS games
                    FROM (
                        SELECT m."HomeActiveGoalieId" AS goalie_id, m."HomeTeamId" AS team_id,
                               m."CompetitionId" AS competition_id, m."Id" AS match_id
                        FROM floorball."FloorballMatches" AS m WHERE m."Status" = 'Completed'
                        UNION
                        SELECT m."AwayActiveGoalieId", m."AwayTeamId", m."CompetitionId", m."Id"
                        FROM floorball."FloorballMatches" AS m WHERE m."Status" = 'Completed'
                        UNION
                        SELECT e."GoalieId", e."TeamId", m."CompetitionId", m."Id"
                        FROM floorball."FloorballMatchEvents" AS e
                        JOIN floorball."FloorballMatches" AS m ON m."Id" = e."MatchId"
                        WHERE m."Status" = 'Completed' AND e."EventType" = 'Save'
                    ) AS g
                    WHERE g.goalie_id IS NOT NULL
                    GROUP BY g.goalie_id, g.team_id, g.competition_id
                ) AS x
                WHERE s."PlayerId" = x.goalie_id
                  AND s."TeamId" = x.team_id
                  AND s."CompetitionId" = x.competition_id;
                """);

            // Goals against average is the goalie's own conceded goals per game played.
            migrationBuilder.Sql(
                """
                UPDATE floorball."FloorballGoalieSeasonStatistics"
                SET "GoalsAgainstAverage" = CASE WHEN "GamesPlayed" > 0
                        THEN LEAST(99.99, ROUND("GoalsAgainst"::numeric / "GamesPlayed", 2)) ELSE 0 END,
                    "SavePercentage" = CASE WHEN "ShotsAgainst" > 0
                        THEN ROUND("Saves"::numeric / "ShotsAgainst" * 100, 2) ELSE 0 END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShooterPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents");

            migrationBuilder.DropColumn(
                name: "WasDelayedPenalty",
                schema: "floorball",
                table: "FloorballMatchEvents");
        }
    }
}
