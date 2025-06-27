using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class AddedFloorballDomainEventStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FloorballDomainEvents",
                schema: "floorball",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredOn = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EventType = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    AggregateId = table.Column<Guid>(type: "uuid", nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FloorballDomainEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FloorballDomainEvents_AggregateId",
                schema: "floorball",
                table: "FloorballDomainEvents",
                column: "AggregateId");

            migrationBuilder.CreateIndex(
                name: "IX_FloorballDomainEvents_AggregateId_Version",
                schema: "floorball",
                table: "FloorballDomainEvents",
                columns: new[] { "AggregateId", "Version" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FloorballDomainEvents",
                schema: "floorball");
        }
    }
}
