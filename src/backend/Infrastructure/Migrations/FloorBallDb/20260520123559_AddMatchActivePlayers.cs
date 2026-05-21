using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddMatchActivePlayers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FloorballMatchActivePlayers",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the match this lineup entry belongs to"),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false, comment: "Team ID (always equals the match's HomeTeamId or AwayTeamId)"),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false, comment: "ID of the player marked as an active field player"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatchActivePlayers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballMatchActivePlayers_FloorballMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "floorball",
                        principalTable: "FloorballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FloorballMatchActivePlayers_FloorballPlayers_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "floorball",
                        principalTable: "FloorballPlayers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchActivePlayer_Audit",
                schema: "floorball",
                table: "FloorballMatchActivePlayers",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchActivePlayer_CreatedAt",
                schema: "floorball",
                table: "FloorballMatchActivePlayers",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchActivePlayer_Match_Team",
                schema: "floorball",
                table: "FloorballMatchActivePlayers",
                columns: new[] { "MatchId", "TeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchActivePlayer_Match_Team_Player",
                schema: "floorball",
                table: "FloorballMatchActivePlayers",
                columns: new[] { "MatchId", "TeamId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchActivePlayer_UpdatedAt",
                schema: "floorball",
                table: "FloorballMatchActivePlayers",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballMatchActivePlayers_PlayerId",
                schema: "floorball",
                table: "FloorballMatchActivePlayers",
                column: "PlayerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballMatchActivePlayers",
                schema: "floorball");
        }
    }
}
