using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class FixedManagerRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FloorballTeamManager_YearsOfExperience",
                schema: "floorball",
                table: "FloorballTeamManagers");

            migrationBuilder.DropColumn(
                name: "YearsOfExperience",
                schema: "floorball",
                table: "FloorballTeamManagers");

            migrationBuilder.AddColumn<Guid>(
                name: "TeamId",
                schema: "floorball",
                table: "FloorballTeamManagers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_FloorballTeamManager_TeamId",
                schema: "floorball",
                table: "FloorballTeamManagers",
                column: "TeamId");

            migrationBuilder.AddForeignKey(
                name: "FK_FloorballTeamManagers_FloorballTeams_TeamId",
                schema: "floorball",
                table: "FloorballTeamManagers",
                column: "TeamId",
                principalSchema: "floorball",
                principalTable: "FloorballTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FloorballTeamManagers_FloorballTeams_TeamId",
                schema: "floorball",
                table: "FloorballTeamManagers");

            migrationBuilder.DropIndex(
                name: "IX_FloorballTeamManager_TeamId",
                schema: "floorball",
                table: "FloorballTeamManagers");

            migrationBuilder.DropColumn(
                name: "TeamId",
                schema: "floorball",
                table: "FloorballTeamManagers");

            migrationBuilder.AddColumn<int>(
                name: "YearsOfExperience",
                schema: "floorball",
                table: "FloorballTeamManagers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddCheckConstraint(
                name: "CK_FloorballTeamManager_YearsOfExperience",
                schema: "floorball",
                table: "FloorballTeamManagers",
                sql: "\"YearsOfExperience\" >= 0");
        }
    }
}
