using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "floorball");

            migrationBuilder.CreateTable(
                name: "FloorballCoaches",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CertificationLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballCoaches", x => x.Id);
                    table.CheckConstraint("CK_FloorballCoach_YearsOfExperience", "\"YearsOfExperience\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "FloorballPlayers",
                schema: "floorball",
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
                    table.PrimaryKey("PK_FloorballPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballReferees",
                schema: "floorball",
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
                    table.PrimaryKey("PK_FloorballReferees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballSeasons",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeamManagers",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PrimaryResponsibility = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTeamManagers", x => x.Id);
                    table.CheckConstraint("CK_FloorballTeamManager_YearsOfExperience", "\"YearsOfExperience\" >= 0");
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeams",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamCategory = table.Column<string>(type: "text", nullable: false),
                    HomeArena = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrimaryJerseyColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryJerseyColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventSourcedFloorballMatches",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    HomeTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    AwayTeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    ScheduledDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Venue = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
                    WentToOvertime = table.Column<bool>(type: "boolean", nullable: false),
                    WentToShootout = table.Column<bool>(type: "boolean", nullable: false),
                    OfficialIdsJson = table.Column<string>(type: "text", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventSourcedFloorballMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventSourcedFloorballMatches_FloorballSeasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "floorball",
                        principalTable: "FloorballSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventSourcedFloorballMatches_FloorballTeams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EventSourcedFloorballMatches_FloorballTeams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatches",
                schema: "floorball",
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
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballMatches_FloorballSeasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "floorball",
                        principalTable: "FloorballSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatches_FloorballTeams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FloorballMatches_FloorballTeams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloorballSeasonTeam",
                schema: "floorball",
                columns: table => new
                {
                    SeasonsId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballSeasonTeam", x => new { x.SeasonsId, x.TeamsId });
                    table.ForeignKey(
                        name: "FK_FloorballSeasonTeam_FloorballSeasons_SeasonsId",
                        column: x => x.SeasonsId,
                        principalSchema: "floorball",
                        principalTable: "FloorballSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballSeasonTeam_FloorballTeams_TeamsId",
                        column: x => x.TeamsId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeamPlayer",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTeamPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballTeamPlayer_FloorballPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "floorball",
                        principalTable: "FloorballPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FloorballTeamPlayer_FloorballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatchEvents",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    TimeInSeconds = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AssistingPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    DurationInMinutes = table.Column<int>(type: "integer", nullable: true),
                    EventType = table.Column<string>(type: "character varying(21)", maxLength: 21, nullable: false),
                    GoalType = table.Column<int>(type: "integer", nullable: true),
                    IsOvertime = table.Column<bool>(type: "boolean", nullable: true),
                    IsShootout = table.Column<bool>(type: "boolean", nullable: true),
                    PenaltyType = table.Column<int>(type: "integer", nullable: true),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: true),
                    ScoringPlayerId = table.Column<Guid>(type: "uuid", nullable: true),
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
                        name: "FK_FloorballMatchEvents_FloorballMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "floorball",
                        principalTable: "FloorballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatchEvents_FloorballPlayers_AssistingPlayerId",
                        column: x => x.AssistingPlayerId,
                        principalSchema: "floorball",
                        principalTable: "FloorballPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FloorballMatchEvents_FloorballPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "floorball",
                        principalTable: "FloorballPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FloorballMatchEvents_FloorballPlayers_ScoringPlayerId",
                        column: x => x.ScoringPlayerId,
                        principalSchema: "floorball",
                        principalTable: "FloorballPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FloorballMatchEvents_FloorballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatchOfficial",
                schema: "floorball",
                columns: table => new
                {
                    MatchesId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfficialsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatchOfficial", x => new { x.MatchesId, x.OfficialsId });
                    table.ForeignKey(
                        name: "FK_FloorballMatchOfficial_FloorballMatches_MatchesId",
                        column: x => x.MatchesId,
                        principalSchema: "floorball",
                        principalTable: "FloorballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatchOfficial_FloorballReferees_OfficialsId",
                        column: x => x.OfficialsId,
                        principalSchema: "floorball",
                        principalTable: "FloorballReferees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballPeriodScores",
                schema: "floorball",
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
                        name: "FK_FloorballPeriodScores_FloorballMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "floorball",
                        principalTable: "FloorballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventSourcedFloorballMatches_AwayTeamId",
                schema: "floorball",
                table: "EventSourcedFloorballMatches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSourcedFloorballMatches_HomeTeamId",
                schema: "floorball",
                table: "EventSourcedFloorballMatches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_EventSourcedFloorballMatches_SeasonId",
                schema: "floorball",
                table: "EventSourcedFloorballMatches",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballCoach_IsActive",
                schema: "floorball",
                table: "FloorballCoaches",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballCoach_PersonId",
                schema: "floorball",
                table: "FloorballCoaches",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballCoach_Specialization",
                schema: "floorball",
                table: "FloorballCoaches",
                column: "Specialization",
                filter: "\"Specialization\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_AwayTeamId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_HomeTeamId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_SeasonId",
                schema: "floorball",
                table: "FloorballMatches",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_AssistingPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "AssistingPlayerId",
                filter: "\"AssistingPlayerId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_DurationInMinutes",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "DurationInMinutes",
                filter: "\"DurationInMinutes\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_GoalType",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "GoalType",
                filter: "\"GoalType\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_MatchId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_MatchId_Period_Time",
                schema: "floorball",
                table: "FloorballMatchEvents",
                columns: new[] { "MatchId", "PeriodNumber", "TimeInSeconds" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_PenaltyType",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "PenaltyType",
                filter: "\"PenaltyType\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_PlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "PlayerId",
                filter: "\"PlayerId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_ScoringPlayerId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "ScoringPlayerId",
                filter: "\"ScoringPlayerId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchEvent_TeamId",
                schema: "floorball",
                table: "FloorballMatchEvents",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchOfficial_OfficialsId",
                schema: "floorball",
                table: "FloorballMatchOfficial",
                column: "OfficialsId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_Audit",
                schema: "floorball",
                table: "FloorballPeriodScores",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_CreatedAt",
                schema: "floorball",
                table: "FloorballPeriodScores",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_Match_Period",
                schema: "floorball",
                table: "FloorballPeriodScores",
                columns: new[] { "MatchId", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_MatchId",
                schema: "floorball",
                table: "FloorballPeriodScores",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_UpdatedAt",
                schema: "floorball",
                table: "FloorballPeriodScores",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonTeam_TeamsId",
                schema: "floorball",
                table: "FloorballSeasonTeam",
                column: "TeamsId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamManager_IsActive",
                schema: "floorball",
                table: "FloorballTeamManagers",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamManager_PersonId",
                schema: "floorball",
                table: "FloorballTeamManagers",
                column: "PersonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_PlayerId",
                schema: "floorball",
                table: "FloorballTeamPlayer",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_JerseyNumber",
                schema: "floorball",
                table: "FloorballTeamPlayer",
                columns: new[] { "TeamId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSourcedFloorballMatches",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballCoaches",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballMatchEvents",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballMatchOfficial",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballPeriodScores",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballSeasonTeam",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballTeamManagers",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballTeamPlayer",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballReferees",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballMatches",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballPlayers",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballSeasons",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballTeams",
                schema: "floorball");
        }
    }
}
