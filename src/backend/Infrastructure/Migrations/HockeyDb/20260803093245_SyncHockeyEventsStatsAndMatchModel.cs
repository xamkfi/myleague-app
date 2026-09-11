using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class SyncHockeyEventsStatsAndMatchModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // NOTE: Do not drop HockeyVideoReviewRules* truncated owned-type shadow columns here.
            // They remain in the model snapshot (nested CoachChallengeRules ownership); dropping them
            // caused EF PendingModelChangesWarning oscillation at Migrate().

            // Home/Away competition team FKs were replaced by MatchTeams; do not rename into
            // TournamentGroupId / PlayoffSeriesId (different semantics — EF false-positive).
            migrationBuilder.DropColumn(
                name: "HomeCompetitionTeamId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "AwayCompetitionTeamId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "hockey",
                table: "HockeyMatches",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "TournamentGroupId",
                schema: "hockey",
                table: "HockeyMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualEndTime",
                schema: "hockey",
                table: "HockeyMatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActualStartTime",
                schema: "hockey",
                table: "HockeyMatches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CompetitionDivisionId",
                schema: "hockey",
                table: "HockeyMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CountsTowardGoalieStatistics",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CountsTowardPlayerStatistics",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CountsTowardStandings",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CountsTowardTeamStatistics",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CurrentPeriodNumber",
                schema: "hockey",
                table: "HockeyMatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_DelayedOffsideEnabled",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_GoaliePullAllowed",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MatchRules_IcingRule",
                schema: "hockey",
                table: "HockeyMatches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_OffsideEnabled",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_OvertimeEnabled",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_OvertimeLengthMinutes",
                schema: "hockey",
                table: "HockeyMatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_PenaltyShotEnabled",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_RegularPeriodCount",
                schema: "hockey",
                table: "HockeyMatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MatchRules_RegularPeriodLengthMinutes",
                schema: "hockey",
                table: "HockeyMatches",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_ShootoutEnabled",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "MatchRules_StopClock",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MatchType",
                schema: "hockey",
                table: "HockeyMatches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ResultType",
                schema: "hockey",
                table: "HockeyMatches",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledStartTime",
                schema: "hockey",
                table: "HockeyMatches",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Status",
                schema: "hockey",
                table: "HockeyMatches",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "UsesLineManagement",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Venue",
                schema: "hockey",
                table: "HockeyMatches",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WentToOvertime",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WentToShootout",
                schema: "hockey",
                table: "HockeyMatches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "HockeyGoalieCompetitionStatistics",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    CompetitionDivisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TournamentGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayoffSeriesId = table.Column<Guid>(type: "uuid", nullable: true),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GamesStarted = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OvertimeLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShootoutLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NoDecisions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Saves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SavePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalsAgainstAverage = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false, defaultValue: 0m),
                    Shutouts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MinutesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyGoalieCompetitionStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieCompetitionStatistics_HockeyCompetitionDivision~",
                        column: x => x.CompetitionDivisionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieCompetitionStatistics_HockeyCompetitions_Compet~",
                        column: x => x.CompetitionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieCompetitionStatistics_HockeyPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieCompetitionStatistics_HockeyPlayoffSeries_Playo~",
                        column: x => x.PlayoffSeriesId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayoffSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieCompetitionStatistics_HockeyTeamPlayers_TeamPla~",
                        column: x => x.TeamPlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeamPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieCompetitionStatistics_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieCompetitionStatistics_HockeyTournamentGroups_To~",
                        column: x => x.TournamentGroupId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchOfficials",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfficialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    IsMainOfficial = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchOfficials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyMatchOfficials_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyPlayerCompetitionStatistics",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    CompetitionDivisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TournamentGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayoffSeriesId = table.Column<Guid>(type: "uuid", nullable: true),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Goals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Assists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PlusMinusRating = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsOnGoal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    Hits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BlockedShots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Takeaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Giveaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TimeOnIceSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Shifts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyPlayerCompetitionStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyPlayerCompetitionStatistics_HockeyCompetitionDivision~",
                        column: x => x.CompetitionDivisionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyPlayerCompetitionStatistics_HockeyCompetitions_Compet~",
                        column: x => x.CompetitionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyPlayerCompetitionStatistics_HockeyPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyPlayerCompetitionStatistics_HockeyPlayoffSeries_Playo~",
                        column: x => x.PlayoffSeriesId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayoffSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyPlayerCompetitionStatistics_HockeyTeamPlayers_TeamPla~",
                        column: x => x.TeamPlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeamPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyPlayerCompetitionStatistics_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyPlayerCompetitionStatistics_HockeyTournamentGroups_To~",
                        column: x => x.TournamentGroupId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyStatisticsCache",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    CacheKey = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Scope = table.Column<string>(type: "text", nullable: true),
                    CompetitionDivisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TournamentGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayoffSeriesId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    JsonData = table.Column<string>(type: "text", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyStatisticsCache", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyStatisticsCache_HockeyCompetitionDivisions_Competitio~",
                        column: x => x.CompetitionDivisionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyStatisticsCache_HockeyCompetitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyStatisticsCache_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyStatisticsCache_HockeyPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyStatisticsCache_HockeyPlayoffSeries_PlayoffSeriesId",
                        column: x => x.PlayoffSeriesId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayoffSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyStatisticsCache_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyStatisticsCache_HockeyTournamentGroups_TournamentGrou~",
                        column: x => x.TournamentGroupId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyTeamCompetitionStatistics",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Scope = table.Column<string>(type: "text", nullable: false),
                    CompetitionDivisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    TournamentGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayoffSeriesId = table.Column<Guid>(type: "uuid", nullable: true),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RegulationWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OvertimeWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShootoutWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RegulationLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OvertimeLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShootoutLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Ties = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalsFor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalDifference = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsFor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    PowerPlayGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PowerPlayOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PowerPlayPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    PenaltyKillOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PenaltyKillSuccesses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PenaltyKillPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    HomeWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HomeLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AwayWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AwayLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    StandingRank = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyTeamCompetitionStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyTeamCompetitionStatistics_HockeyCompetitionDivisions_~",
                        column: x => x.CompetitionDivisionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyTeamCompetitionStatistics_HockeyCompetitions_Competit~",
                        column: x => x.CompetitionId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyTeamCompetitionStatistics_HockeyPlayoffSeries_Playoff~",
                        column: x => x.PlayoffSeriesId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayoffSeries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyTeamCompetitionStatistics_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyTeamCompetitionStatistics_HockeyTournamentGroups_Tour~",
                        column: x => x.TournamentGroupId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyGoalieMatchStatistics",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchActivePlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    WasStarter = table.Column<bool>(type: "boolean", nullable: false),
                    Decision = table.Column<string>(type: "text", nullable: false),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GamesStarted = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OvertimeLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShootoutLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    NoDecisions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Saves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SavePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalsAgainstAverage = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false, defaultValue: 0m),
                    Shutouts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MinutesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyGoalieMatchStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieMatchStatistics_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieMatchStatistics_HockeyPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieMatchStatistics_HockeyTeamPlayers_TeamPlayerId",
                        column: x => x.TeamPlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeamPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoalieMatchStatistics_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyGoaliePeriodStatistics",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    GoalieMatchStatisticsId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchActivePlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    PeriodType = table.Column<string>(type: "text", nullable: false),
                    TimeOnIceSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Saves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SavePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyGoaliePeriodStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyGoaliePeriodStatistics_HockeyGoalieMatchStatistics_Go~",
                        column: x => x.GoalieMatchStatisticsId,
                        principalSchema: "hockey",
                        principalTable: "HockeyGoalieMatchStatistics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HockeyGoaliePeriodStatistics_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoaliePeriodStatistics_HockeyPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoaliePeriodStatistics_HockeyTeamPlayers_TeamPlayerId",
                        column: x => x.TeamPlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeamPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyGoaliePeriodStatistics_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchActivePlayers",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchPlayerSelectionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    JerseyNumber = table.Column<int>(type: "integer", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    CaptainRole = table.Column<string>(type: "text", nullable: false),
                    IsStartingPlayer = table.Column<bool>(type: "boolean", nullable: false),
                    IsGoalie = table.Column<bool>(type: "boolean", nullable: false),
                    IsEmergencyGoalie = table.Column<bool>(type: "boolean", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchActivePlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchTeams",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    TeamSlot = table.Column<string>(type: "text", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    IsGoaliePulled = table.Column<bool>(type: "boolean", nullable: false),
                    ActiveGoalieMatchPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TracksOnIcePlayers = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyMatchTeams_HockeyCompetitionTeams_CompetitionTeamId",
                        column: x => x.CompetitionTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyCompetitionTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyMatchTeams_HockeyMatchActivePlayers_ActiveGoalieMatch~",
                        column: x => x.ActiveGoalieMatchPlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyMatchTeams_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchEvents",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventType = table.Column<int>(type: "integer", nullable: false),
                    MatchTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    MatchActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    GameTime = table.Column<TimeSpan>(type: "interval", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    WinningMatchTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    LosingMatchTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    WinningActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    LosingActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Zone = table.Column<string>(type: "text", nullable: true),
                    Spot = table.Column<string>(type: "text", nullable: true),
                    ScoringMatchTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScorerActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    PrimaryAssistActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondaryAssistActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoalieActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    RelatedShotId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoalStrength = table.Column<string>(type: "text", nullable: true),
                    IsGameWinningGoal = table.Column<bool>(type: "boolean", nullable: true),
                    WasEmptyNet = table.Column<bool>(type: "boolean", nullable: true),
                    WasDelayedPenalty = table.Column<bool>(type: "boolean", nullable: true),
                    WasPenaltyShotGoal = table.Column<bool>(type: "boolean", nullable: true),
                    OutgoingGoalieActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    IncomingGoalieActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Reason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PenaltyMatchTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    PenalizedActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServedByActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    Severity = table.Column<string>(type: "text", nullable: true),
                    Offence = table.Column<string>(type: "text", nullable: true),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: true),
                    PenaltyStartTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    PenaltyEndTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    IsBenchPenalty = table.Column<bool>(type: "boolean", nullable: true),
                    IsDelayedPenalty = table.Column<bool>(type: "boolean", nullable: true),
                    WasServed = table.Column<bool>(type: "boolean", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: true),
                    ShooterActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    HockeyShootoutAttempt_GoalieActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShotOrder = table.Column<int>(type: "integer", nullable: true),
                    Result = table.Column<string>(type: "text", nullable: true),
                    ShootingMatchTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    HockeyShot_ShooterActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    HockeyShot_GoalieActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShotResult = table.Column<string>(type: "text", nullable: true),
                    IsPowerPlayShot = table.Column<bool>(type: "boolean", nullable: true),
                    IsShortHandedShot = table.Column<bool>(type: "boolean", nullable: true),
                    IsShootoutShot = table.Column<bool>(type: "boolean", nullable: true),
                    CountsAsShotOnGoal = table.Column<bool>(type: "boolean", nullable: true),
                    HockeyStoppage_Reason = table.Column<string>(type: "text", nullable: true),
                    ResponsibleMatchTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponsibleActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    NextFaceoffZone = table.Column<string>(type: "text", nullable: true),
                    NextFaceoffSpot = table.Column<string>(type: "text", nullable: true),
                    RuleReference = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReviewType = table.Column<string>(type: "text", nullable: true),
                    OriginalDecision = table.Column<string>(type: "text", nullable: true),
                    FinalDecision = table.Column<string>(type: "text", nullable: true),
                    RequestedByMatchTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsCoachChallenge = table.Column<bool>(type: "boolean", nullable: true),
                    WasSuccessful = table.Column<bool>(type: "boolean", nullable: true),
                    ResultingPenaltyId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchEvents", x => x.Id);
                    table.CheckConstraint("CK_HockeyMatchEvent_PeriodNumber", "\"PeriodNumber\" >= 1");
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_GoalieActivePlay~",
                        column: x => x.GoalieActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_HockeyShootoutAt~",
                        column: x => x.HockeyShootoutAttempt_GoalieActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_HockeyShot_Goali~",
                        column: x => x.HockeyShot_GoalieActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_HockeyShot_Shoot~",
                        column: x => x.HockeyShot_ShooterActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_IncomingGoalieAc~",
                        column: x => x.IncomingGoalieActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_LosingActivePlay~",
                        column: x => x.LosingActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_MatchActivePlaye~",
                        column: x => x.MatchActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_OutgoingGoalieAc~",
                        column: x => x.OutgoingGoalieActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_PenalizedActiveP~",
                        column: x => x.PenalizedActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_PrimaryAssistAct~",
                        column: x => x.PrimaryAssistActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_ResponsibleActiv~",
                        column: x => x.ResponsibleActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_ScorerActivePlay~",
                        column: x => x.ScorerActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_SecondaryAssistA~",
                        column: x => x.SecondaryAssistActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_ServedByActivePl~",
                        column: x => x.ServedByActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_ShooterActivePla~",
                        column: x => x.ShooterActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchActivePlayers_WinningActivePla~",
                        column: x => x.WinningActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchEvents_RelatedShotId",
                        column: x => x.RelatedShotId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchEvents_ResultingPenaltyId",
                        column: x => x.ResultingPenaltyId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchTeams_LosingMatchTeamId",
                        column: x => x.LosingMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchTeams_MatchTeamId",
                        column: x => x.MatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchTeams_PenaltyMatchTeamId",
                        column: x => x.PenaltyMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchTeams_RequestedByMatchTeamId",
                        column: x => x.RequestedByMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchTeams_ResponsibleMatchTeamId",
                        column: x => x.ResponsibleMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchTeams_ScoringMatchTeamId",
                        column: x => x.ScoringMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchTeams_ShootingMatchTeamId",
                        column: x => x.ShootingMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatchTeams_WinningMatchTeamId",
                        column: x => x.WinningMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchEvents_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchLines",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: true),
                    LineType = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsLocked = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchLines", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyMatchLines_HockeyMatchTeams_MatchTeamId",
                        column: x => x.MatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchPlayerSelections",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ConfirmedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchPlayerSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyMatchPlayerSelections_HockeyMatchTeams_MatchTeamId",
                        column: x => x.MatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchPlayerStatistics",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchActivePlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Goals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Assists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PlusMinusRating = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsOnGoal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    Hits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BlockedShots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Takeaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Giveaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TimeOnIceSeconds = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Shifts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchPlayerStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyMatchPlayerStatistics_HockeyMatchActivePlayers_MatchA~",
                        column: x => x.MatchActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchPlayerStatistics_HockeyMatchTeams_MatchTeamId",
                        column: x => x.MatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchPlayerStatistics_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchPlayerStatistics_HockeyPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchPlayerStatistics_HockeyTeamPlayers_TeamPlayerId",
                        column: x => x.TeamPlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeamPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchPlayerStatistics_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchTeamStatistics",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoalsFor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsOnGoal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MissedShots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BlockedShotAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    Saves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    TeamSavePercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    PowerPlayOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PowerPlayGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PowerPlayPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    PenaltyKillOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PenaltyKillSuccesses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PenaltyKillPercentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    Penalties = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Hits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BlockedShots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Takeaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Giveaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchTeamStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyMatchTeamStatistics_HockeyMatchTeams_MatchTeamId",
                        column: x => x.MatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchTeamStatistics_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchTeamStatistics_HockeyTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HockeyOnIceStates",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyOnIceStates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyOnIceStates_HockeyMatchTeams_MatchTeamId",
                        column: x => x.MatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyPeriodScores",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    PeriodType = table.Column<string>(type: "text", nullable: false),
                    HomeMatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayMatchTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeGoals = table.Column<int>(type: "integer", nullable: false),
                    AwayGoals = table.Column<int>(type: "integer", nullable: false),
                    HomeShots = table.Column<int>(type: "integer", nullable: false),
                    AwayShots = table.Column<int>(type: "integer", nullable: false),
                    HomeFaceoffWins = table.Column<int>(type: "integer", nullable: false),
                    AwayFaceoffWins = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyPeriodScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyPeriodScores_HockeyMatchTeams_AwayMatchTeamId",
                        column: x => x.AwayMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyPeriodScores_HockeyMatchTeams_HomeMatchTeamId",
                        column: x => x.HomeMatchTeamId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyPeriodScores_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyMatchLinePlayers",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchLineId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchActivePlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slot = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchLinePlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyMatchLinePlayers_HockeyMatchActivePlayers_MatchActive~",
                        column: x => x.MatchActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyMatchLinePlayers_HockeyMatchLines_MatchLineId",
                        column: x => x.MatchLineId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyOnIceChanges",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    OnIceStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChangeType = table.Column<string>(type: "text", nullable: false),
                    OutgoingActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    IncomingActivePlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    AppliedLineId = table.Column<Guid>(type: "uuid", nullable: true),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: true),
                    GameTime = table.Column<TimeSpan>(type: "interval", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyOnIceChanges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyOnIceChanges_HockeyMatchActivePlayers_IncomingActiveP~",
                        column: x => x.IncomingActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyOnIceChanges_HockeyMatchActivePlayers_OutgoingActiveP~",
                        column: x => x.OutgoingActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyOnIceChanges_HockeyMatchLines_AppliedLineId",
                        column: x => x.AppliedLineId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_HockeyOnIceChanges_HockeyOnIceStates_OnIceStateId",
                        column: x => x.OnIceStateId,
                        principalSchema: "hockey",
                        principalTable: "HockeyOnIceStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HockeyOnIcePlayers",
                schema: "hockey",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    OnIceStateId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchActivePlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slot = table.Column<string>(type: "text", nullable: true),
                    Order = table.Column<int>(type: "integer", nullable: true),
                    IsGoalie = table.Column<bool>(type: "boolean", nullable: false),
                    IsExtraAttacker = table.Column<bool>(type: "boolean", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyOnIcePlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HockeyOnIcePlayers_HockeyMatchActivePlayers_MatchActivePlay~",
                        column: x => x.MatchActivePlayerId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatchActivePlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HockeyOnIcePlayers_HockeyOnIceStates_OnIceStateId",
                        column: x => x.OnIceStateId,
                        principalSchema: "hockey",
                        principalTable: "HockeyOnIceStates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatches_CompetitionDivisionId",
                schema: "hockey",
                table: "HockeyMatches",
                column: "CompetitionDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatches_PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyMatches",
                column: "PlayoffSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatches_ScheduledStartTime",
                schema: "hockey",
                table: "HockeyMatches",
                column: "ScheduledStartTime");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatches_TournamentGroupId",
                schema: "hockey",
                table: "HockeyMatches",
                column: "TournamentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_Audit",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_Competition_SavePct",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                columns: new[] { "CompetitionId", "SavePercentage" });

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_CompetitionDivisionId",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                column: "CompetitionDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_CreatedAt",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                column: "PlayoffSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_TeamId",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_TeamPlayerId",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                column: "TeamPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_TournamentGroupId",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                column: "TournamentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_UniqueScope",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                columns: new[] { "PlayerId", "TeamId", "CompetitionId", "Scope", "CompetitionDivisionId", "TournamentGroupId", "PlayoffSeriesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieCompetitionStatistics_UpdatedAt",
                schema: "hockey",
                table: "HockeyGoalieCompetitionStatistics",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_Audit",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_CreatedAt",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_Match_ActivePlayer",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                columns: new[] { "MatchId", "MatchActivePlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_MatchActivePlayerId",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "MatchActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_MatchTeamId",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "MatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_PlayerId",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_TeamId",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_TeamPlayerId",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "TeamPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoalieMatchStatistics_UpdatedAt",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_Audit",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_CreatedAt",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_GoalieMatchStatisticsId",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "GoalieMatchStatisticsId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_Match_ActivePlayer_Period",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                columns: new[] { "MatchId", "MatchActivePlayerId", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_MatchActivePlayerId",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "MatchActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_MatchTeamId",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "MatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_PlayerId",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_TeamId",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_TeamPlayerId",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "TeamPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyGoaliePeriodStatistics_UpdatedAt",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchActivePlayer_Audit",
                schema: "hockey",
                table: "HockeyMatchActivePlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchActivePlayer_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchActivePlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchActivePlayer_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchActivePlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchActivePlayers_Selection_TeamPlayer",
                schema: "hockey",
                table: "HockeyMatchActivePlayers",
                columns: new[] { "MatchPlayerSelectionId", "TeamPlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvent_Audit",
                schema: "hockey",
                table: "HockeyMatchEvents",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvent_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvent_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_GoalieActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "GoalieActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_HockeyShootoutAttempt_GoalieActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "HockeyShootoutAttempt_GoalieActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_HockeyShot_GoalieActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "HockeyShot_GoalieActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_HockeyShot_ShooterActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "HockeyShot_ShooterActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_IncomingGoalieActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "IncomingGoalieActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_LosingActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "LosingActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_LosingMatchTeamId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "LosingMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_MatchActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "MatchActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_MatchId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_MatchId_Period_GameTime",
                schema: "hockey",
                table: "HockeyMatchEvents",
                columns: new[] { "MatchId", "PeriodNumber", "GameTime" });

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_MatchTeamId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "MatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_OutgoingGoalieActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "OutgoingGoalieActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_PenalizedActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "PenalizedActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_PenaltyMatchTeamId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "PenaltyMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_PrimaryAssistActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "PrimaryAssistActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_RelatedShotId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "RelatedShotId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_RequestedByMatchTeamId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "RequestedByMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_ResponsibleActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "ResponsibleActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_ResponsibleMatchTeamId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "ResponsibleMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_ResultingPenaltyId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "ResultingPenaltyId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_ScorerActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "ScorerActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_ScoringMatchTeamId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "ScoringMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_SecondaryAssistActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "SecondaryAssistActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_ServedByActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "ServedByActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_ShooterActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "ShooterActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_ShootingMatchTeamId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "ShootingMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_WinningActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "WinningActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchEvents_WinningMatchTeamId",
                schema: "hockey",
                table: "HockeyMatchEvents",
                column: "WinningMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLinePlayer_Audit",
                schema: "hockey",
                table: "HockeyMatchLinePlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLinePlayer_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchLinePlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLinePlayer_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchLinePlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLinePlayers_Line_ActivePlayer",
                schema: "hockey",
                table: "HockeyMatchLinePlayers",
                columns: new[] { "MatchLineId", "MatchActivePlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLinePlayers_MatchActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchLinePlayers",
                column: "MatchActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLine_Audit",
                schema: "hockey",
                table: "HockeyMatchLines",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLine_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchLines",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLine_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchLines",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchLines_MatchTeamId",
                schema: "hockey",
                table: "HockeyMatchLines",
                column: "MatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchOfficial_Audit",
                schema: "hockey",
                table: "HockeyMatchOfficials",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchOfficial_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchOfficials",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchOfficial_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchOfficials",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchOfficials_Match_Official",
                schema: "hockey",
                table: "HockeyMatchOfficials",
                columns: new[] { "MatchId", "OfficialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerSelection_Audit",
                schema: "hockey",
                table: "HockeyMatchPlayerSelections",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerSelection_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchPlayerSelections",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerSelection_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchPlayerSelections",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerSelections_MatchTeamId",
                schema: "hockey",
                table: "HockeyMatchPlayerSelections",
                column: "MatchTeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_Audit",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_Match_ActivePlayer",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                columns: new[] { "MatchId", "MatchActivePlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_MatchActivePlayerId",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                column: "MatchActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_MatchTeamId",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                column: "MatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_PlayerId",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_TeamId",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_TeamPlayerId",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                column: "TeamPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchPlayerStatistics_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchPlayerStatistics",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeam_Audit",
                schema: "hockey",
                table: "HockeyMatchTeams",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeam_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchTeams",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeam_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchTeams",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeams_ActiveGoalieMatchPlayerId",
                schema: "hockey",
                table: "HockeyMatchTeams",
                column: "ActiveGoalieMatchPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeams_CompetitionTeamId",
                schema: "hockey",
                table: "HockeyMatchTeams",
                column: "CompetitionTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeams_Match_Slot",
                schema: "hockey",
                table: "HockeyMatchTeams",
                columns: new[] { "MatchId", "TeamSlot" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeams_TeamId",
                schema: "hockey",
                table: "HockeyMatchTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeamStatistics_Audit",
                schema: "hockey",
                table: "HockeyMatchTeamStatistics",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeamStatistics_CreatedAt",
                schema: "hockey",
                table: "HockeyMatchTeamStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeamStatistics_Match_MatchTeam",
                schema: "hockey",
                table: "HockeyMatchTeamStatistics",
                columns: new[] { "MatchId", "MatchTeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeamStatistics_MatchTeamId",
                schema: "hockey",
                table: "HockeyMatchTeamStatistics",
                column: "MatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeamStatistics_TeamId",
                schema: "hockey",
                table: "HockeyMatchTeamStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyMatchTeamStatistics_UpdatedAt",
                schema: "hockey",
                table: "HockeyMatchTeamStatistics",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceChange_Audit",
                schema: "hockey",
                table: "HockeyOnIceChanges",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceChange_CreatedAt",
                schema: "hockey",
                table: "HockeyOnIceChanges",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceChange_UpdatedAt",
                schema: "hockey",
                table: "HockeyOnIceChanges",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceChanges_AppliedLineId",
                schema: "hockey",
                table: "HockeyOnIceChanges",
                column: "AppliedLineId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceChanges_IncomingActivePlayerId",
                schema: "hockey",
                table: "HockeyOnIceChanges",
                column: "IncomingActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceChanges_OnIceStateId",
                schema: "hockey",
                table: "HockeyOnIceChanges",
                column: "OnIceStateId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceChanges_OutgoingActivePlayerId",
                schema: "hockey",
                table: "HockeyOnIceChanges",
                column: "OutgoingActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIcePlayer_Audit",
                schema: "hockey",
                table: "HockeyOnIcePlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIcePlayer_CreatedAt",
                schema: "hockey",
                table: "HockeyOnIcePlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIcePlayer_UpdatedAt",
                schema: "hockey",
                table: "HockeyOnIcePlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIcePlayers_MatchActivePlayerId",
                schema: "hockey",
                table: "HockeyOnIcePlayers",
                column: "MatchActivePlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIcePlayers_State_ActivePlayer",
                schema: "hockey",
                table: "HockeyOnIcePlayers",
                columns: new[] { "OnIceStateId", "MatchActivePlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceState_Audit",
                schema: "hockey",
                table: "HockeyOnIceStates",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceState_CreatedAt",
                schema: "hockey",
                table: "HockeyOnIceStates",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceState_UpdatedAt",
                schema: "hockey",
                table: "HockeyOnIceStates",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyOnIceStates_MatchTeamId",
                schema: "hockey",
                table: "HockeyOnIceStates",
                column: "MatchTeamId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPeriodScore_Audit",
                schema: "hockey",
                table: "HockeyPeriodScores",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPeriodScore_CreatedAt",
                schema: "hockey",
                table: "HockeyPeriodScores",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPeriodScore_UpdatedAt",
                schema: "hockey",
                table: "HockeyPeriodScores",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPeriodScores_AwayMatchTeamId",
                schema: "hockey",
                table: "HockeyPeriodScores",
                column: "AwayMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPeriodScores_HomeMatchTeamId",
                schema: "hockey",
                table: "HockeyPeriodScores",
                column: "HomeMatchTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPeriodScores_Match_Period",
                schema: "hockey",
                table: "HockeyPeriodScores",
                columns: new[] { "MatchId", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_Audit",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_Competition_Points",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                columns: new[] { "CompetitionId", "Points" });

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_CompetitionDivisionId",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                column: "CompetitionDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_CreatedAt",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                column: "PlayoffSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_TeamId",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_TeamPlayerId",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                column: "TeamPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_TournamentGroupId",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                column: "TournamentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_UniqueScope",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                columns: new[] { "PlayerId", "TeamId", "CompetitionId", "Scope", "CompetitionDivisionId", "TournamentGroupId", "PlayoffSeriesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyPlayerCompetitionStatistics_UpdatedAt",
                schema: "hockey",
                table: "HockeyPlayerCompetitionStatistics",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_Audit",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_CacheKey",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_CompetitionDivisionId",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "CompetitionDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_CompetitionId",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_CreatedAt",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_MatchId",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_PlayerId",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "PlayoffSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_TeamId",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_TournamentGroupId",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "TournamentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyStatisticsCache_UpdatedAt",
                schema: "hockey",
                table: "HockeyStatisticsCache",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamCompetitionStatistics_Audit",
                schema: "hockey",
                table: "HockeyTeamCompetitionStatistics",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamCompetitionStatistics_Competition_Points",
                schema: "hockey",
                table: "HockeyTeamCompetitionStatistics",
                columns: new[] { "CompetitionId", "Points" });

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamCompetitionStatistics_CompetitionDivisionId",
                schema: "hockey",
                table: "HockeyTeamCompetitionStatistics",
                column: "CompetitionDivisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamCompetitionStatistics_CreatedAt",
                schema: "hockey",
                table: "HockeyTeamCompetitionStatistics",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamCompetitionStatistics_PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyTeamCompetitionStatistics",
                column: "PlayoffSeriesId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamCompetitionStatistics_TournamentGroupId",
                schema: "hockey",
                table: "HockeyTeamCompetitionStatistics",
                column: "TournamentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamCompetitionStatistics_UniqueScope",
                schema: "hockey",
                table: "HockeyTeamCompetitionStatistics",
                columns: new[] { "TeamId", "CompetitionId", "Scope", "CompetitionDivisionId", "TournamentGroupId", "PlayoffSeriesId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HockeyTeamCompetitionStatistics_UpdatedAt",
                schema: "hockey",
                table: "HockeyTeamCompetitionStatistics",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyMatches_HockeyCompetitionDivisions_CompetitionDivisio~",
                schema: "hockey",
                table: "HockeyMatches",
                column: "CompetitionDivisionId",
                principalSchema: "hockey",
                principalTable: "HockeyCompetitionDivisions",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyMatches_HockeyPlayoffSeries_PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyMatches",
                column: "PlayoffSeriesId",
                principalSchema: "hockey",
                principalTable: "HockeyPlayoffSeries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyMatches_HockeyTournamentGroups_TournamentGroupId",
                schema: "hockey",
                table: "HockeyMatches",
                column: "TournamentGroupId",
                principalSchema: "hockey",
                principalTable: "HockeyTournamentGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyGoalieMatchStatistics_HockeyMatchActivePlayers_MatchA~",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "MatchActivePlayerId",
                principalSchema: "hockey",
                principalTable: "HockeyMatchActivePlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyGoalieMatchStatistics_HockeyMatchTeams_MatchTeamId",
                schema: "hockey",
                table: "HockeyGoalieMatchStatistics",
                column: "MatchTeamId",
                principalSchema: "hockey",
                principalTable: "HockeyMatchTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyGoaliePeriodStatistics_HockeyMatchActivePlayers_Match~",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "MatchActivePlayerId",
                principalSchema: "hockey",
                principalTable: "HockeyMatchActivePlayers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyGoaliePeriodStatistics_HockeyMatchTeams_MatchTeamId",
                schema: "hockey",
                table: "HockeyGoaliePeriodStatistics",
                column: "MatchTeamId",
                principalSchema: "hockey",
                principalTable: "HockeyMatchTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HockeyMatchActivePlayers_HockeyMatchPlayerSelections_MatchP~",
                schema: "hockey",
                table: "HockeyMatchActivePlayers",
                column: "MatchPlayerSelectionId",
                principalSchema: "hockey",
                principalTable: "HockeyMatchPlayerSelections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HockeyMatches_HockeyCompetitionDivisions_CompetitionDivisio~",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_HockeyMatches_HockeyPlayoffSeries_PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_HockeyMatches_HockeyTournamentGroups_TournamentGroupId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropForeignKey(
                name: "FK_HockeyMatchTeams_HockeyMatchActivePlayers_ActiveGoalieMatch~",
                schema: "hockey",
                table: "HockeyMatchTeams");

            migrationBuilder.DropTable(
                name: "HockeyGoalieCompetitionStatistics",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyGoaliePeriodStatistics",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchEvents",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchLinePlayers",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchOfficials",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchPlayerStatistics",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchTeamStatistics",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyOnIceChanges",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyOnIcePlayers",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyPeriodScores",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyPlayerCompetitionStatistics",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyStatisticsCache",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyTeamCompetitionStatistics",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyGoalieMatchStatistics",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchLines",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyOnIceStates",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchActivePlayers",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchPlayerSelections",
                schema: "hockey");

            migrationBuilder.DropTable(
                name: "HockeyMatchTeams",
                schema: "hockey");

            migrationBuilder.DropIndex(
                name: "IX_HockeyMatches_CompetitionDivisionId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropIndex(
                name: "IX_HockeyMatches_PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropIndex(
                name: "IX_HockeyMatches_ScheduledStartTime",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropIndex(
                name: "IX_HockeyMatches_TournamentGroupId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "ActualEndTime",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "ActualStartTime",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "CompetitionDivisionId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "CountsTowardGoalieStatistics",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "CountsTowardPlayerStatistics",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "CountsTowardStandings",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "CountsTowardTeamStatistics",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "CurrentPeriodNumber",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_DelayedOffsideEnabled",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_GoaliePullAllowed",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_IcingRule",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_OffsideEnabled",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_OvertimeEnabled",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_OvertimeLengthMinutes",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_PenaltyShotEnabled",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_RegularPeriodCount",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_RegularPeriodLengthMinutes",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_ShootoutEnabled",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchRules_StopClock",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "MatchType",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "ResultType",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "ScheduledStartTime",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "Status",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "UsesLineManagement",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "Venue",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "WentToOvertime",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "WentToShootout",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "TournamentGroupId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.DropColumn(
                name: "PlayoffSeriesId",
                schema: "hockey",
                table: "HockeyMatches");

            migrationBuilder.AlterColumn<Guid>(
                name: "CompetitionId",
                schema: "hockey",
                table: "HockeyMatches",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HomeCompetitionTeamId",
                schema: "hockey",
                table: "HockeyMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "AwayCompetitionTeamId",
                schema: "hockey",
                table: "HockeyMatches",
                type: "uuid",
                nullable: true);
        }
    }
}
