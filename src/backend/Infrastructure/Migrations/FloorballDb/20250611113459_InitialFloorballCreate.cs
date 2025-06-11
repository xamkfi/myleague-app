using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorballDb
{
    /// <inheritdoc />
    public partial class InitialFloorballCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EventSourcedFloorballMatches",
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
                });

            migrationBuilder.CreateTable(
                name: "FloorballPlayers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Position_PrimaryPosition = table.Column<string>(type: "text", nullable: false),
                    Position_SecondaryPosition = table.Column<string>(type: "text", nullable: true),
                    Position_CanPlayAsGoalkeeper = table.Column<bool>(type: "boolean", nullable: false),
                    CareerGoals = table.Column<int>(type: "integer", nullable: false),
                    CareerAssists = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballPlayers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballReferees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LicenseIssueDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    LicenseExpiryDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    MatchesOfficiated = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballReferees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballSeasons",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Division = table.Column<string>(type: "text", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballSeasons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ShortName = table.Column<string>(type: "text", nullable: false),
                    Division = table.Column<string>(type: "text", nullable: false),
                    TeamCategory = table.Column<string>(type: "text", nullable: false),
                    HomeArena = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PrimaryJerseyColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SecondaryJerseyColor = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTeams", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatches",
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
                    WentToShootout = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballMatches_FloorballSeasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "FloorballSeasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatches_FloorballTeams_AwayTeamId",
                        column: x => x.AwayTeamId,
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatches_FloorballTeams_HomeTeamId",
                        column: x => x.HomeTeamId,
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballTeamPlayer",
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
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballTeamPlayer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballTeamPlayer_FloorballTeams_TeamId",
                        column: x => x.TeamId,
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballMatchOfficial",
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
                        principalTable: "FloorballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatchOfficial_FloorballReferees_OfficialsId",
                        column: x => x.OfficialsId,
                        principalTable: "FloorballReferees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FloorballPeriodScore",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PeriodNumber = table.Column<int>(type: "integer", nullable: false),
                    HomeScore = table.Column<int>(type: "integer", nullable: false),
                    AwayScore = table.Column<int>(type: "integer", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballPeriodScore", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballPeriodScore_FloorballMatches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "FloorballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_AwayTeamId",
                table: "FloorballMatches",
                column: "AwayTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_HomeTeamId",
                table: "FloorballMatches",
                column: "HomeTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatches_SeasonId",
                table: "FloorballMatches",
                column: "SeasonId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchOfficial_OfficialsId",
                table: "FloorballMatchOfficial",
                column: "OfficialsId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballPeriodScore_MatchId",
                table: "FloorballPeriodScore",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId",
                table: "FloorballTeamPlayer",
                column: "TeamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSourcedFloorballMatches");

            migrationBuilder.DropTable(
                name: "FloorballMatchOfficial");

            migrationBuilder.DropTable(
                name: "FloorballPeriodScore");

            migrationBuilder.DropTable(
                name: "FloorballPlayers");

            migrationBuilder.DropTable(
                name: "FloorballTeamPlayer");

            migrationBuilder.DropTable(
                name: "FloorballReferees");

            migrationBuilder.DropTable(
                name: "FloorballMatches");

            migrationBuilder.DropTable(
                name: "FloorballSeasons");

            migrationBuilder.DropTable(
                name: "FloorballTeams");
        }
    }
}
