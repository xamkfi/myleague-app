using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class ConvertFloorballTeamPlayerToIndependentEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballTeamPlayer",
                schema: "floorball");

            migrationBuilder.CreateTable(
                name: "FloorballTeamPlayers",
                schema: "floorball",
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
                        name: "FK_FloorballTeamPlayers_FloorballPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "floorball",
                        principalTable: "FloorballPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FloorballTeamPlayers_FloorballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_Audit",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_CreatedAt",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_PlayerId",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_JerseyNumber",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "JerseyNumber" },
                unique: true,
                filter: "\"JerseyNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_TeamId_PlayerId",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                columns: new[] { "TeamId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamPlayer_UpdatedAt",
                schema: "floorball",
                table: "FloorballTeamPlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballTeamPlayers",
                schema: "floorball");

            migrationBuilder.CreateTable(
                name: "FloorballTeamPlayer",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GamesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Goals = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    JerseyNumber = table.Column<int>(type: "integer", nullable: true),
                    PenaltyMinutes = table.Column<int>(type: "integer", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
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
    }
}
