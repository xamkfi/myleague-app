using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FootballDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "football");

            migrationBuilder.CreateTable(
                name: "FootballCompetitions",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    TeamCategory = table.Column<string>(type: "text", nullable: false, defaultValue: "Adult"),
                    MatchRules_NumberOfHalves = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    MatchRules_HalfDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 45),
                    MatchRules_PlayersOnField = table.Column<int>(type: "integer", nullable: false, defaultValue: 11),
                    MatchRules_RequireGoalkeeper = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MatchRules_MaxSubstitutions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MatchRules_RequireOfficialsToStart = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MatchRules_AllowExtraTime = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MatchRules_ExtraTimeHalfCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MatchRules_ExtraTimeHalfDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MatchRules_AllowPenaltyShootout = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    StandingRules_WinPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    StandingRules_DrawPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    StandingRules_LossPoints = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CompetitionType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    ContentHtml = table.Column<string>(type: "character varying(50000)", maxLength: 50000, nullable: true),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TournamentStatus = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_GroupStage_NumberOfHalves = table.Column<int>(type: "integer", nullable: true, defaultValue: 2),
                    TournamentRules_GroupStage_HalfDurationMinutes = table.Column<int>(type: "integer", nullable: true, defaultValue: 45),
                    TournamentRules_GroupStage_PlayersOnField = table.Column<int>(type: "integer", nullable: true, defaultValue: 11),
                    TournamentRules_GroupStage_RequireGoalkeeper = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    TournamentRules_GroupStage_MaxSubstitutions = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    TournamentRules_GroupStage_RequireOfficialsToStart = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    TournamentRules_GroupStage_AllowExtraTime = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    TournamentRules_GroupStage_ExtraTimeHalfCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    TournamentRules_GroupStage_ExtraTimeHalfDurationMinutes = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    TournamentRules_GroupStage_AllowPenaltyShootout = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    TournamentRules_Playoff_NumberOfHalves = table.Column<int>(type: "integer", nullable: true, defaultValue: 2),
                    TournamentRules_Playoff_HalfDurationMinutes = table.Column<int>(type: "integer", nullable: true, defaultValue: 45),
                    TournamentRules_Playoff_PlayersOnField = table.Column<int>(type: "integer", nullable: true, defaultValue: 11),
                    TournamentRules_Playoff_RequireGoalkeeper = table.Column<bool>(type: "boolean", nullable: true, defaultValue: true),
                    TournamentRules_Playoff_MaxSubstitutions = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    TournamentRules_Playoff_RequireOfficialsToStart = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    TournamentRules_Playoff_AllowExtraTime = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    TournamentRules_Playoff_ExtraTimeHalfCount = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    TournamentRules_Playoff_ExtraTimeHalfDurationMinutes = table.Column<int>(type: "integer", nullable: true, defaultValue: 0),
                    TournamentRules_Playoff_AllowPenaltyShootout = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    TournamentRules_TeamsAdvancingPerGroup = table.Column<int>(type: "integer", nullable: true),
                    TournamentRules_HasPlayoffStage = table.Column<bool>(type: "boolean", nullable: true),
                    TournamentRules_HasThirdPlaceMatch = table.Column<bool>(type: "boolean", nullable: true),
                    ChampionTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayoffSchedule = table.Column<string>(type: "jsonb", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballCompetitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballMatchTeamStatistics",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    YellowCards = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RedCards = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Substitutions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CleanSheet = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballMatchTeamStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballPlayers",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Position_PrimaryPosition = table.Column<string>(type: "text", nullable: false),
                    Position_SecondaryPosition = table.Column<string>(type: "text", nullable: true),
                    CareerGoals = table.Column<int>(type: "integer", nullable: false),
                    CareerAssists = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballPlayerSeasonStatistics",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Goals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Assists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    YellowCards = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RedCards = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballPlayerSeasonStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballReferees",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LicenseIssueDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MatchesOfficiated = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballReferees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballStatisticsCache",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: true),
                    JsonData = table.Column<string>(type: "text", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballStatisticsCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballTeams",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamCategory = table.Column<string>(type: "text", nullable: false),
                    HomeArena = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrimaryJerseyColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryJerseyColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballTeamSeasonStatistics",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Draws = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalsFor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    GoalDifference = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HomeWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AwayWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    HomeLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AwayLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CleanSheets = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    YellowCards = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RedCards = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballTeamSeasonStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FootballSeasonDivisions",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballSeasonDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballSeasonDivisions_FootballCompetitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalSchema: "football",
                        principalTable: "FootballCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootballTournamentGroups",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballTournamentGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballTournamentGroups_FootballCompetitions_TournamentId",
                        column: x => x.TournamentId,
                        principalSchema: "football",
                        principalTable: "FootballCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootballCompetitionTeam",
                schema: "football",
                columns: table => new
                {
                    CompetitionsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballCompetitionTeam", x => new { x.CompetitionsId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_FootballCompetitionTeam_FootballCompetitions_CompetitionsId",
                        column: x => x.CompetitionsId,
                        principalSchema: "football",
                        principalTable: "FootballCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FootballCompetitionTeam_FootballTeams_TeamsId",
                        column: x => x.TeamsId,
                        principalSchema: "football",
                        principalTable: "FootballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootballTeamManagers",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballTeamManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballTeamManagers_FootballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "football",
                        principalTable: "FootballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootballTeamPlayers",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    RequestedJerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    YellowCards = table.Column<int>(type: "integer", nullable: false),
                    RedCards = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballTeamPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballTeamPlayers_FootballPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "football",
                        principalTable: "FootballPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FootballTeamPlayers_FootballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "football",
                        principalTable: "FootballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootballSeasonDivisionTeams",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionDivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballSeasonDivisionTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballSeasonDivisionTeams_FootballSeasonDivisions_Competi~",
                        column: x => x.CompetitionDivisionId,
                        principalSchema: "football",
                        principalTable: "FootballSeasonDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FootballSeasonDivisionTeams_FootballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "football",
                        principalTable: "FootballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FootballMatches",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScheduledDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
                    WentToExtraTime = table.Column<bool>(type: "boolean", nullable: false),
                    WentToPenaltyShootout = table.Column<bool>(type: "boolean", nullable: false),
                    MatchRules_NumberOfHalves = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    MatchRules_HalfDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 45),
                    MatchRules_PlayersOnField = table.Column<int>(type: "integer", nullable: false, defaultValue: 11),
                    MatchRules_RequireGoalkeeper = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MatchRules_MaxSubstitutions = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MatchRules_RequireOfficialsToStart = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MatchRules_AllowExtraTime = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    MatchRules_ExtraTimeHalfCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MatchRules_ExtraTimeHalfDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    MatchRules_AllowPenaltyShootout = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TournamentStage = table.Column<int>(type: "integer", nullable: true),
                    TournamentGroupId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayoffRound = table.Column<int>(type: "integer", nullable: true),
                    PlayoffMatchOrder = table.Column<int>(type: "integer", nullable: true),
                    NextMatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    NextMatchSlot = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballMatches_FootballCompetitions_CompetitionId",
                        column: x => x.CompetitionId,
                        principalSchema: "football",
                        principalTable: "FootballCompetitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FootballMatches_FootballMatches_NextMatchId",
                        column: x => x.NextMatchId,
                        principalSchema: "football",
                        principalTable: "FootballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FootballMatches_FootballTeams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalSchema: "football",
                        principalTable: "FootballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FootballMatches_FootballTeams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalSchema: "football",
                        principalTable: "FootballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FootballMatches_FootballTournamentGroups_TournamentGroupId",
                        column: x => x.TournamentGroupId,
                        principalSchema: "football",
                        principalTable: "FootballTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FootballTournamentGroupTeams",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TournamentGroupId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballTournamentGroupTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballTournamentGroupTeams_FootballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "football",
                        principalTable: "FootballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FootballTournamentGroupTeams_FootballTournamentGroups_Tourn~",
                        column: x => x.TournamentGroupId,
                        principalSchema: "football",
                        principalTable: "FootballTournamentGroups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootballMatchEvents",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    TimeInSeconds = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EventType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    CardType = table.Column<string>(type: "text", nullable: true),
                    ScoringPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssistingPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoalType = table.Column<int>(type: "integer", nullable: true),
                    PlayerOffId = table.Column<Guid>(type: "uuid", nullable: true),
                    PlayerOnId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballMatchEvents", x => x.Id);
                    table.CheckConstraint("CK_FootballMatchEvent_PeriodNumber", "\"PeriodNumber\" > 0");
                    table.CheckConstraint("CK_FootballMatchEvent_TimeInSeconds", "\"TimeInSeconds\" >= 0");
                    table.ForeignKey(
                        name: "FK_FootballMatchEvents_FootballMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "football",
                        principalTable: "FootballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FootballMatchEvents_FootballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "football",
                        principalTable: "FootballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FootballMatchLineupPlayers",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    IsOnField = table.Column<bool>(type: "boolean", nullable: false),
                    IsSentOff = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballMatchLineupPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballMatchLineupPlayers_FootballMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "football",
                        principalTable: "FootballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FootballMatchLineupPlayers_FootballPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "football",
                        principalTable: "FootballPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FootballMatchOfficial",
                schema: "football",
                columns: table => new
                {
                    MatchesId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfficialsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballMatchOfficial", x => new { x.MatchesId, x.OfficialsId });
                    table.ForeignKey(
                        name: "FK_FootballMatchOfficial_FootballMatches_MatchesId",
                        column: x => x.MatchesId,
                        principalSchema: "football",
                        principalTable: "FootballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FootballMatchOfficial_FootballReferees_OfficialsId",
                        column: x => x.OfficialsId,
                        principalSchema: "football",
                        principalTable: "FootballReferees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FootballPeriodScores",
                schema: "football",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AwayScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FootballPeriodScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FootballPeriodScores_FootballMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "football",
                        principalTable: "FootballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FootballCompetitions_TeamCategory",
                schema: "football",
                table: "FootballCompetitions",
                column: "TeamCategory");

            migrationBuilder.CreateIndex(
                name: "IX_FootballCompetitionTeam_TeamsId",
                schema: "football",
                table: "FootballCompetitionTeam",
                column: "TeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_AwayTeamId",
                schema: "football",
                table: "FootballMatches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_CompetitionId",
                schema: "football",
                table: "FootballMatches",
                column: "CompetitionId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_HomeTeamId",
                schema: "football",
                table: "FootballMatches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_NextMatchId",
                schema: "football",
                table: "FootballMatches",
                column: "NextMatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatches_TournamentGroupId",
                schema: "football",
                table: "FootballMatches",
                column: "TournamentGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchEvent_AssistingPlayerId",
                schema: "football",
                table: "FootballMatchEvents",
                column: "AssistingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchEvent_CardPlayerId",
                schema: "football",
                table: "FootballMatchEvents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchEvent_MatchId",
                schema: "football",
                table: "FootballMatchEvents",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchEvent_MatchId_Period_Time",
                schema: "football",
                table: "FootballMatchEvents",
                columns: new[] { "MatchId", "PeriodNumber", "TimeInSeconds" });

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchEvent_PlayerOffId",
                schema: "football",
                table: "FootballMatchEvents",
                column: "PlayerOffId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchEvent_PlayerOnId",
                schema: "football",
                table: "FootballMatchEvents",
                column: "PlayerOnId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchEvent_ScoringPlayerId",
                schema: "football",
                table: "FootballMatchEvents",
                column: "ScoringPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchEvent_TeamId",
                schema: "football",
                table: "FootballMatchEvents",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchLineupPlayer_Audit",
                schema: "football",
                table: "FootballMatchLineupPlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchLineupPlayer_CreatedAt",
                schema: "football",
                table: "FootballMatchLineupPlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchLineupPlayer_Match_Team_Player",
                schema: "football",
                table: "FootballMatchLineupPlayers",
                columns: new[] { "MatchId", "TeamId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchLineupPlayer_UpdatedAt",
                schema: "football",
                table: "FootballMatchLineupPlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchLineupPlayers_PlayerId",
                schema: "football",
                table: "FootballMatchLineupPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchOfficial_OfficialsId",
                schema: "football",
                table: "FootballMatchOfficial",
                column: "OfficialsId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballMatchTeamStatistics_Match_Team",
                schema: "football",
                table: "FootballMatchTeamStatistics",
                columns: new[] { "MatchId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballPeriodScore_Audit",
                schema: "football",
                table: "FootballPeriodScores",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FootballPeriodScore_CreatedAt",
                schema: "football",
                table: "FootballPeriodScores",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FootballPeriodScore_Match_Period",
                schema: "football",
                table: "FootballPeriodScores",
                columns: new[] { "MatchId", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballPeriodScore_UpdatedAt",
                schema: "football",
                table: "FootballPeriodScores",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballPlayerSeasonStatistics_Competition_Goals",
                schema: "football",
                table: "FootballPlayerSeasonStatistics",
                columns: new[] { "CompetitionId", "Goals" });

            migrationBuilder.CreateIndex(
                name: "IX_FootballPlayerSeasonStatistics_Player_Team_Competition",
                schema: "football",
                table: "FootballPlayerSeasonStatistics",
                columns: new[] { "PlayerId", "TeamId", "CompetitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballSeasonDivisions_Season_Division",
                schema: "football",
                table: "FootballSeasonDivisions",
                columns: new[] { "CompetitionId", "DivisionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballSeasonDivisionTeams_Season_Team",
                schema: "football",
                table: "FootballSeasonDivisionTeams",
                columns: new[] { "CompetitionId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballSeasonDivisionTeams_SeasonDivision_Team",
                schema: "football",
                table: "FootballSeasonDivisionTeams",
                columns: new[] { "CompetitionDivisionId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballSeasonDivisionTeams_TeamId",
                schema: "football",
                table: "FootballSeasonDivisionTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballStatisticsCache_CacheKey",
                schema: "football",
                table: "FootballStatisticsCache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamManager_PersonId",
                schema: "football",
                table: "FootballTeamManagers",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamManager_TeamId",
                schema: "football",
                table: "FootballTeamManagers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_Audit",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_CreatedAt",
                schema: "football",
                table: "FootballTeamPlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_PlayerId",
                schema: "football",
                table: "FootballTeamPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_TeamId_JerseyNumber",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "TeamId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_TeamId_PlayerId",
                schema: "football",
                table: "FootballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamPlayer_UpdatedAt",
                schema: "football",
                table: "FootballTeamPlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FootballTeamSeasonStatistics_Team_Competition",
                schema: "football",
                table: "FootballTeamSeasonStatistics",
                columns: new[] { "TeamId", "CompetitionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballTournamentGroups_Tournament_Order",
                schema: "football",
                table: "FootballTournamentGroups",
                columns: new[] { "TournamentId", "Order" });

            migrationBuilder.CreateIndex(
                name: "IX_FootballTournamentGroupTeams_Group_Team",
                schema: "football",
                table: "FootballTournamentGroupTeams",
                columns: new[] { "TournamentGroupId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FootballTournamentGroupTeams_TeamId",
                schema: "football",
                table: "FootballTournamentGroupTeams",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FootballCompetitionTeam",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballMatchEvents",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballMatchLineupPlayers",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballMatchOfficial",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballMatchTeamStatistics",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballPeriodScores",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballPlayerSeasonStatistics",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballSeasonDivisionTeams",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballStatisticsCache",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballTeamManagers",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballTeamPlayers",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballTeamSeasonStatistics",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballTournamentGroupTeams",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballReferees",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballMatches",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballSeasonDivisions",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballPlayers",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballTeams",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballTournamentGroups",
                schema: "football");

            migrationBuilder.DropTable(
                name: "FootballCompetitions",
                schema: "football");
        }
    }
}
