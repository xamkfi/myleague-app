using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddFloorballMatchScorekeepers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FloorballMatchScorekeepers",
                schema: "floorball",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballMatchScorekeepers", x => new { x.MatchId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_FloorballMatchScorekeepers_FloorballMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "floorball",
                        principalTable: "FloorballMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballMatchScorekeepers",
                schema: "floorball");
        }
    }
}
