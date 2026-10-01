using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.HockeyDb
{
    /// <inheritdoc />
    public partial class AddHockeyMatchScorekeepers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HockeyMatchScorekeepers",
                schema: "hockey",
                columns: table => new
                {
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HockeyMatchScorekeepers", x => new { x.MatchId, x.PersonId });
                    table.ForeignKey(
                        name: "FK_HockeyMatchScorekeepers_HockeyMatches_MatchId",
                        column: x => x.MatchId,
                        principalSchema: "hockey",
                        principalTable: "HockeyMatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HockeyMatchScorekeepers",
                schema: "hockey");
        }
    }
}
