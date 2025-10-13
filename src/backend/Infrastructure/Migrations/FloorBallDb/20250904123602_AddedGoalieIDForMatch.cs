using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddedGoalieIDForMatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AwayActiveGoalieId",
                schema: "floorball",
                table: "FloorballMatches",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "HomeActiveGoalieId",
                schema: "floorball",
                table: "FloorballMatches",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AwayActiveGoalieId",
                schema: "floorball",
                table: "FloorballMatches");

            migrationBuilder.DropColumn(
                name: "HomeActiveGoalieId",
                schema: "floorball",
                table: "FloorballMatches");
        }
    }
}
