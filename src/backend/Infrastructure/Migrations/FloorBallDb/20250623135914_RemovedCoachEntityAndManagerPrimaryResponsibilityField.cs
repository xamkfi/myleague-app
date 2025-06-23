using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class RemovedCoachEntityAndManagerPrimaryResponsibilityField : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballCoaches",
                schema: "floorball");

            migrationBuilder.DropColumn(
                name: "PrimaryResponsibility",
                schema: "floorball",
                table: "FloorballTeamManagers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PrimaryResponsibility",
                schema: "floorball",
                table: "FloorballTeamManagers",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "FloorballCoaches",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CertificationLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PersonId = table.Column<Guid>(type: "uuid", nullable: false),
                    Specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    YearsOfExperience = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballCoaches", x => x.Id);
                    table.CheckConstraint("CK_FloorballCoach_YearsOfExperience", "\"YearsOfExperience\" >= 0");
                });

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
        }
    }
}
