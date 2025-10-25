using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddedSeasonDivison : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                        name: "FK_FloorballSeasonDivisions_FloorballSeasons_SeasonId",
                        column: x => x.SeasonId,
                        principalSchema: "floorball",
                        principalTable: "FloorballSeasons",
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
                        name: "FK_FloorballSeasonDivisionTeams_FloorballTeams_TeamId",
                        column: x => x.TeamId,
                        principalSchema: "floorball",
                        principalTable: "FloorballTeams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballSeasonDivisionTeams",
                schema: "floorball");

            migrationBuilder.DropTable(
                name: "FloorballSeasonDivisions",
                schema: "floorball");
        }
    }
}
