using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateFloorballEntitiesForDivisionId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Division",
                table: "FloorballTeams");

            migrationBuilder.DropColumn(
                name: "Division",
                table: "FloorballSeasons");

            migrationBuilder.AddColumn<Guid>(
                name: "DivisionId",
                table: "FloorballTeams",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "DivisionId",
                table: "FloorballSeasons",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "FloorballTeams");

            migrationBuilder.DropColumn(
                name: "DivisionId",
                table: "FloorballSeasons");

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "FloorballTeams",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Division",
                table: "FloorballSeasons",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
