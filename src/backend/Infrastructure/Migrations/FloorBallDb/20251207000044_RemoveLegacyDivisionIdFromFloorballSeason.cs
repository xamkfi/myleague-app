using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.FloorBallDb
{
    /// <inheritdoc />
    public partial class RemoveLegacyDivisionIdFromFloorballSeason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Migrate existing data - Create FloorballSeasonDivision entries for seasons that have DivisionId
            // but don't have a corresponding FloorballSeasonDivision entry
            migrationBuilder.Sql(@"
                INSERT INTO floorball.""FloorballSeasonDivisions"" (""Id"", ""SeasonId"", ""DivisionId"", ""CreatedAt"", ""UpdatedAt"")
                SELECT 
                    gen_random_uuid() as ""Id"",
                    s.""Id"" as ""SeasonId"",
                    s.""DivisionId"",
                    s.""CreatedAt"",
                    NULL as ""UpdatedAt""
                FROM floorball.""FloorballSeasons"" s
                WHERE s.""DivisionId"" IS NOT NULL
                    AND NOT EXISTS (
                        SELECT 1 
                        FROM floorball.""FloorballSeasonDivisions"" sd 
                        WHERE sd.""SeasonId"" = s.""Id"" 
                            AND sd.""DivisionId"" = s.""DivisionId""
                    );
            ");

            // Step 2: Drop the DivisionId column from FloorballSeasons table
            migrationBuilder.DropColumn(
                name: "DivisionId",
                schema: "floorball",
                table: "FloorballSeasons");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Re-add the DivisionId column (nullable initially to allow data migration)
            migrationBuilder.AddColumn<Guid>(
                name: "DivisionId",
                schema: "floorball",
                table: "FloorballSeasons",
                type: "uuid",
                nullable: true);

            // Migrate data back: Set DivisionId to the first division from FloorballSeasonDivisions
            migrationBuilder.Sql(@"
                UPDATE floorball.""FloorballSeasons"" s
                SET ""DivisionId"" = (
                    SELECT sd.""DivisionId""
                    FROM floorball.""FloorballSeasonDivisions"" sd
                    WHERE sd.""SeasonId"" = s.""Id""
                    ORDER BY sd.""CreatedAt""
                    LIMIT 1
                )
                WHERE EXISTS (
                    SELECT 1 
                    FROM floorball.""FloorballSeasonDivisions"" sd 
                    WHERE sd.""SeasonId"" = s.""Id""
                );
            ");

            // Make DivisionId required (non-nullable) after data migration
            migrationBuilder.AlterColumn<Guid>(
                name: "DivisionId",
                schema: "floorball",
                table: "FloorballSeasons",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);
        }
    }
}

