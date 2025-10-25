using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    public partial class SeasonDivisions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FloorballSeasonDivisions",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    DivisionId = table.Column<Guid>(type: "uuid", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonDivisions_Season_Division",
                schema: "floorball",
                table: "FloorballSeasonDivisions",
                columns: new[] { "SeasonId", "DivisionId" },
                unique: true);

            migrationBuilder.CreateTable(
                name: "FloorballSeasonDivisionTeams",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonDivisionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballSeasonDivisionTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FloorballSeasonDivisionTeams_FloorballSeasonDivisions_SeasonDivisionId",
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
                name: "IX_FloorballSeasonDivisionTeams_SeasonDivision_Team",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                columns: new[] { "SeasonDivisionId", "TeamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FloorballSeasonDivisionTeams_Season_Team",
                schema: "floorball",
                table: "FloorballSeasonDivisionTeams",
                columns: new[] { "SeasonId", "TeamId" },
                unique: true);

            // Backfill: create season-division from existing seasons
            migrationBuilder.Sql(@"
                INSERT INTO floorball.""FloorballSeasonDivisions"" (""Id"", ""CreatedAt"", ""UpdatedAt"", ""SeasonId"", ""DivisionId"")
                SELECT gen_random_uuid(), NOW() AT TIME ZONE 'UTC', NULL, s.""Id"", s.""DivisionId""
                FROM floorball.""FloorballSeasons"" s
                ON CONFLICT DO NOTHING
            ");

            // Backfill: move memberships from FloorballSeasonTeam to SeasonDivisionTeam
            migrationBuilder.Sql(@"
                INSERT INTO floorball.""FloorballSeasonDivisionTeams"" (""Id"", ""CreatedAt"", ""UpdatedAt"", ""SeasonId"", ""SeasonDivisionId"", ""TeamId"")
                SELECT gen_random_uuid(), NOW() AT TIME ZONE 'UTC', NULL, fst.""SeasonsId"", fsd.""Id"", fst.""TeamsId""
                FROM floorball.""FloorballSeasonTeam"" fst
                JOIN floorball.""FloorballSeasons"" s ON s.""Id"" = fst.""SeasonsId""
                JOIN floorball.""FloorballSeasonDivisions"" fsd ON fsd.""SeasonId"" = s.""Id"" AND fsd.""DivisionId"" = s.""DivisionId""
                ON CONFLICT DO NOTHING
            ");
        }

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


