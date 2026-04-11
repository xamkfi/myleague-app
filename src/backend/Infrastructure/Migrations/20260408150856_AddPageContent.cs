using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPageContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "floorball");

            migrationBuilder.CreateTable(
                name: "FloorballMatchTeamStatistics",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the match these statistics belong to"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the team these statistics are for"),
                    ShotsOnGoal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Shots on goal"),
                    ShotsTotal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total shots taken"),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Shot percentage"),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Faceoffs won"),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total faceoffs"),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Faceoff win percentage"),
                    PowerPlayOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play opportunities"),
                    PowerPlayGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play goals"),
                    PowerPlayMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play minutes"),
                    PenaltyKillOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Penalty kill opportunities"),
                    PenaltyKillSuccess = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Successful penalty kills"),
                    ShortHandedGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed goals"),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Penalty minutes"),
                    Hits = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Hits delivered"),
                    BlockedShots = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Shots blocked"),
                    Takeaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Takeaways"),
                    Giveaways = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Giveaways"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatchTeamStatistics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballPlayer",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Position_PrimaryPosition = table.Column<string>(type: "text", nullable: false),
                    Position_SecondaryPosition = table.Column<string>(type: "text", nullable: true),
                    Position_CanPlayAsGoalkeeper = table.Column<bool>(type: "boolean", nullable: false),
                    CareerGoals = table.Column<int>(type: "integer", nullable: false),
                    CareerAssists = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballPlayer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballReferee",
                schema: "common",
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
                    table.PrimaryKey("PK_FloorballReferee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballSeason",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    MatchRules_NumberOfPeriods = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    MatchRules_PeriodDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    MatchRules_AllowOvertime = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MatchRules_OvertimeDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    MatchRules_AllowShootout = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballSeason", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballStatisticsCache",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CacheKey = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Unique cache key identifier"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: true, comment: "Optional season ID this cache is associated with"),
                    JsonData = table.Column<string>(type: "text", nullable: false, comment: "Serialized JSON data"),
                    LastUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When this cache entry was last updated"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When this cache entry expires"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballStatisticsCache", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeam",
                schema: "common",
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
                    table.PrimaryKey("PK_FloorballTeam", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PageContents",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PageSlug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentHtml = table.Column<string>(type: "text", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PageContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballSeasonDivisions",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballSeasonDivisions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballSeasonDivisions_FloorballSeason_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "common",
                        principalTable: "FloorballSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballGoalieSeasonStatistics",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the goalie these statistics belong to"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the team the goalie played for"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the season these statistics are for"),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of games played"),
                    GamesStarted = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of games started"),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of wins"),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of losses"),
                    Ties = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of ties"),
                    Saves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total saves made"),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total shots faced"),
                    SavePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Save percentage"),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Goals allowed"),
                    GoalsAgainstAverage = table.Column<decimal>(type: "numeric(4,2)", nullable: false, defaultValue: 0m, comment: "Goals against average"),
                    Shutouts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of shutouts"),
                    MinutesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total minutes played"),
                    PowerPlaySaves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play saves"),
                    PowerPlayShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play shots faced"),
                    PowerPlaySavePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Power play save percentage"),
                    ShortHandedSaves = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed saves"),
                    ShortHandedShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed shots faced"),
                    ShortHandedSavePercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Short-handed save percentage"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballGoalieSeasonStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballGoalieSeasonStatistics_FloorballPlayer_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "common",
                        principalTable: "FloorballPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballGoalieSeasonStatistics_FloorballSeason_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "common",
                        principalTable: "FloorballSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballGoalieSeasonStatistics_FloorballTeam_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatch",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<string>(type: "text", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
                    WentToOvertime = table.Column<bool>(type: "boolean", nullable: false),
                    WentToShootout = table.Column<bool>(type: "boolean", nullable: false),
                    MatchRules_NumberOfPeriods = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    MatchRules_PeriodDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 15),
                    MatchRules_AllowOvertime = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MatchRules_OvertimeDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 5),
                    MatchRules_AllowShootout = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    HomeActiveGoalieId = table.Column<Guid>(type: "uuid", nullable: true),
                    AwayActiveGoalieId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatch", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballMatch_FloorballSeason_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "common",
                        principalTable: "FloorballSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatch_FloorballTeam_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FloorballMatch_FloorballTeam_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloorballPlayerSeasonStatistics",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the player these statistics belong to"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the team the player played for"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the season these statistics are for"),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of games played"),
                    Goals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Goals scored"),
                    Assists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Assists made"),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total points (goals + assists)"),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Penalty minutes"),
                    PlusMinusRating = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Plus/minus rating"),
                    ShotsOnGoal = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Shots on goal"),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Shooting percentage"),
                    PowerPlayGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play goals"),
                    PowerPlayAssists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play assists"),
                    ShortHandedGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed goals"),
                    ShortHandedAssists = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed assists"),
                    GameWinningGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Game-winning goals"),
                    OvertimeGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Overtime goals"),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Faceoffs won"),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total faceoffs taken"),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Faceoff win percentage"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballPlayerSeasonStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballPlayerSeasonStatistics_FloorballPlayer_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "common",
                        principalTable: "FloorballPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballPlayerSeasonStatistics_FloorballSeason_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "common",
                        principalTable: "FloorballSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballPlayerSeasonStatistics_FloorballTeam_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballSeasonTeam",
                schema: "common",
                columns: table => new
                {
                    SeasonsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballSeasonTeam", x => new { x.SeasonsId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_FloorballSeasonTeam_FloorballSeason_SeasonsId",
                        column: x => x.SeasonsId,
                        principalSchema: "common",
                        principalTable: "FloorballSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballSeasonTeam_FloorballTeam_TeamsId",
                        column: x => x.TeamsId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeamManagers",
                schema: "common",
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
                    table.PrimaryKey("PK_FloorballTeamManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballTeamManagers_FloorballTeam_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeamPlayers",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTeamPlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballTeamPlayers_FloorballPlayer_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "common",
                        principalTable: "FloorballPlayer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FloorballTeamPlayers_FloorballTeam_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeamSeasonStatistics",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the team these statistics belong to"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the season these statistics are for"),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of games played"),
                    Wins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of wins"),
                    Losses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of losses"),
                    Ties = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Number of ties/overtime losses"),
                    Points = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total points earned"),
                    GoalsFor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total goals scored"),
                    GoalsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total goals conceded"),
                    GoalDifference = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Goal difference (goals for - goals against)"),
                    ShotsFor = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total shots taken"),
                    ShotsAgainst = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total shots faced"),
                    ShotPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Shot percentage"),
                    PowerPlayGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play goals scored"),
                    PowerPlayOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Power play opportunities"),
                    PowerPlayPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Power play success percentage"),
                    ShortHandedGoals = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Short-handed goals scored"),
                    PenaltyKillOpportunities = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Penalty kill opportunities"),
                    PenaltyKillPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Penalty kill success percentage"),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total penalty minutes"),
                    FaceoffWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Faceoffs won"),
                    FaceoffAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Total faceoffs taken"),
                    FaceoffPercentage = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 0m, comment: "Faceoff win percentage"),
                    HomeWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Home wins"),
                    HomeLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Home losses"),
                    AwayWins = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Away wins"),
                    AwayLosses = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Away losses"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTeamSeasonStatistics", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballTeamSeasonStatistics_FloorballSeason_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "common",
                        principalTable: "FloorballSeason",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballTeamSeasonStatistics_FloorballTeam_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballSeasonDivisionTeams",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonDivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballSeasonDivisionTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballSeasonDivisionTeams_FloorballSeasonDivisions_Seaso~",
                        column: x => x.SeasonDivisionId,
                        principalSchema: "floorball",
                        principalTable: "FloorballSeasonDivisions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballSeasonDivisionTeams_FloorballTeam_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatchEvents",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    TimeInSeconds = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EventType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    ScoringPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssistingPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    SecondaryAssistingPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    GoalType = table.Column<int>(type: "integer", nullable: true),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    PenaltyType = table.Column<int>(type: "integer", nullable: true),
                    DurationInMinutes = table.Column<int>(type: "integer", nullable: true),
                    GoalieId = table.Column<Guid>(type: "uuid", nullable: true),
                    WasInOvertime = table.Column<bool>(type: "boolean", nullable: true),
                    WasInShootout = table.Column<bool>(type: "boolean", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatchEvents", x => x.Id);
                    table.CheckConstraint("CK_FloorballMatchEvent_PeriodNumber", "\"PeriodNumber\" > 0");
                    table.CheckConstraint("CK_FloorballMatchEvent_TimeInSeconds", "\"TimeInSeconds\" >= 0");
                    table.CheckConstraint("CK_FloorballPenalty_DurationInMinutes", "\"DurationInMinutes\" IS NULL OR \"DurationInMinutes\" > 0");
                    table.ForeignKey(
                        name: "FK_FloorballMatchEvents_FloorballMatch_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "common",
                        principalTable: "FloorballMatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatchEvents_FloorballTeam_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "common",
                        principalTable: "FloorballTeam",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatchOfficial",
                schema: "common",
                columns: table => new
                {
                    MatchesId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfficialsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatchOfficial", x => new { x.MatchesId, x.OfficialsId });
                    table.ForeignKey(
                        name: "FK_FloorballMatchOfficial_FloorballMatch_MatchesId",
                        column: x => x.MatchesId,
                        principalSchema: "common",
                        principalTable: "FloorballMatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatchOfficial_FloorballReferee_OfficialsId",
                        column: x => x.OfficialsId,
                        principalSchema: "common",
                        principalTable: "FloorballReferee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballPeriodScores",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the match this period score belongs to"),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false, comment: "The period number (1, 2, 3, etc.)"),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the home team"),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the away team"),
                    HomeScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Home team score for this period"),
                    AwayScore = table.Column<int>(type: "integer", nullable: false, defaultValue: 0, comment: "Away team score for this period"),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Whether the period is completed"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballPeriodScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballPeriodScores_FloorballMatch_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "common",
                        principalTable: "FloorballMatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_PlayerId",
                schema: "common",
                table: "FloorballGoalieSeasonStatistics",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "common",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "PlayerId", "TeamId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId",
                schema: "common",
                table: "FloorballGoalieSeasonStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_GAA",
                schema: "common",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "SeasonId", "GoalsAgainstAverage" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_SavePercentage",
                schema: "common",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "SeasonId", "SavePercentage" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_SeasonId_Wins",
                schema: "common",
                table: "FloorballGoalieSeasonStatistics",
                columns: new[] { "SeasonId", "Wins" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballGoalieSeasonStatistics_TeamId",
                schema: "common",
                table: "FloorballGoalieSeasonStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatch_AwayTeamId",
                schema: "common",
                table: "FloorballMatch",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatch_HomeTeamId",
                schema: "common",
                table: "FloorballMatch",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatch_SeasonId",
                schema: "common",
                table: "FloorballMatch",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_AssistingPlayerId",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "AssistingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_DurationInMinutes",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "DurationInMinutes");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_GoalieId",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "GoalieId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_GoalType",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "GoalType");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_MatchId",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_MatchId_Period_Time",
                schema: "common",
                table: "FloorballMatchEvents",
                columns: new[] { "MatchId", "PeriodNumber", "TimeInSeconds" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_PenaltyType",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "PenaltyType");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_PlayerId",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_ScoringPlayerId",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "ScoringPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_SecondaryAssistingPlayerId",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "SecondaryAssistingPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_TeamId",
                schema: "common",
                table: "FloorballMatchEvents",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchOfficial_OfficialsId",
                schema: "common",
                table: "FloorballMatchOfficial",
                column: "OfficialsId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchTeamStatistics_MatchId",
                schema: "common",
                table: "FloorballMatchTeamStatistics",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchTeamStatistics_MatchId_TeamId",
                schema: "common",
                table: "FloorballMatchTeamStatistics",
                columns: new[] { "MatchId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchTeamStatistics_TeamId",
                schema: "common",
                table: "FloorballMatchTeamStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_Audit",
                schema: "common",
                table: "FloorballPeriodScores",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_CreatedAt",
                schema: "common",
                table: "FloorballPeriodScores",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_Match_Period",
                schema: "common",
                table: "FloorballPeriodScores",
                columns: new[] { "MatchId", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_MatchId",
                schema: "common",
                table: "FloorballPeriodScores",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_UpdatedAt",
                schema: "common",
                table: "FloorballPeriodScores",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_PlayerId",
                schema: "common",
                table: "FloorballPlayerSeasonStatistics",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_PlayerId_TeamId_SeasonId",
                schema: "common",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "PlayerId", "TeamId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId",
                schema: "common",
                table: "FloorballPlayerSeasonStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Assists",
                schema: "common",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "SeasonId", "Assists" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Goals",
                schema: "common",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "SeasonId", "Goals" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_SeasonId_Points",
                schema: "common",
                table: "FloorballPlayerSeasonStatistics",
                columns: new[] { "SeasonId", "Points" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPlayerSeasonStatistics_TeamId",
                schema: "common",
                table: "FloorballPlayerSeasonStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonDivisions_Season_Division",
                schema: "floorball",
                table: "FloorballSeasonDivisions",
                columns: new[] { "SeasonId", "DivisionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonDivisionTeams_Season_Team",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                columns: new[] { "SeasonId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonDivisionTeams_SeasonDivision_Team",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                columns: new[] { "SeasonDivisionId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonDivisionTeams_TeamId",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonTeam_TeamsId",
                schema: "common",
                table: "FloorballSeasonTeam",
                column: "TeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_CacheKey",
                schema: "common",
                table: "FloorballStatisticsCache",
                column: "CacheKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_ExpiresAt",
                schema: "common",
                table: "FloorballStatisticsCache",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_SeasonId",
                schema: "common",
                table: "FloorballStatisticsCache",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballStatisticsCache_SeasonId_ExpiresAt",
                schema: "common",
                table: "FloorballStatisticsCache",
                columns: new[] { "SeasonId", "ExpiresAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamManager_IsActive",
                schema: "common",
                table: "FloorballTeamManagers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamManager_PersonId",
                schema: "common",
                table: "FloorballTeamManagers",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamManager_TeamId",
                schema: "common",
                table: "FloorballTeamManagers",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_Audit",
                schema: "common",
                table: "FloorballTeamPlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_CreatedAt",
                schema: "common",
                table: "FloorballTeamPlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_PlayerId",
                schema: "common",
                table: "FloorballTeamPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_JerseyNumber",
                schema: "common",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_PlayerId",
                schema: "common",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_UpdatedAt",
                schema: "common",
                table: "FloorballTeamPlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_SeasonId",
                schema: "common",
                table: "FloorballTeamSeasonStatistics",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_TeamId",
                schema: "common",
                table: "FloorballTeamSeasonStatistics",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamSeasonStatistics_TeamId_SeasonId",
                schema: "common",
                table: "FloorballTeamSeasonStatistics",
                columns: new[] { "TeamId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PageContents_PageSlug",
                schema: "common",
                table: "PageContents",
                column: "PageSlug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballGoalieSeasonStatistics",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballMatchEvents",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballMatchOfficial",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballMatchTeamStatistics",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballPeriodScores",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballPlayerSeasonStatistics",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballSeasonDivisionTeams",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballSeasonTeam",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballStatisticsCache",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballTeamManagers",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballTeamPlayers",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballTeamSeasonStatistics",
                schema: "common");

            migrationBuilder.DropTable(
                name: "PageContents",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballReferee",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballMatch",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballSeasonDivisions",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballPlayer",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballTeam",
                schema: "common");

            migrationBuilder.DropTable(
                name: "FloorballSeason",
                schema: "common");
        }
    }
}
