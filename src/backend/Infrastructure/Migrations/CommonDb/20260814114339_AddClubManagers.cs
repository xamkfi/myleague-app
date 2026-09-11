using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddClubManagers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClubManagers",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClubId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClubManagers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClubManagers_Clubs_ClubId",
                        column: x => x.ClubId,
                        principalSchema: "common",
                        principalTable: "Clubs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClubManagers_Persons_PersonId",
                        column: x => x.PersonId,
                        principalSchema: "common",
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClubManagers_ClubId",
                schema: "common",
                table: "ClubManagers",
                column: "ClubId");

            migrationBuilder.CreateIndex(
                name: "IX_ClubManagers_PersonId_ClubId",
                schema: "common",
                table: "ClubManagers",
                columns: new[] { "PersonId", "ClubId" },
                unique: true);

            // The TeamLeader role was replaced by the club-scoped ClubAdmin role
            migrationBuilder.Sql("UPDATE common.\"Users\" SET \"Role\" = 'ClubAdmin' WHERE \"Role\" = 'TeamLeader';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClubManagers",
                schema: "common");
        }
    }
}
