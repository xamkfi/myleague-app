using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorballDb
{
    /// <inheritdoc />
    public partial class RemovedDomainEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventSourcedFloorballMatches",
                schema: "floorball"
                );
            migrationBuilder.DropTable(
                name: "FloorballEventStore",
                schema: "floorball"
                );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
