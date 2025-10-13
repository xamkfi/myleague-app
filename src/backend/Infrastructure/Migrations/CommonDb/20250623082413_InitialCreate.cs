using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "common");

            migrationBuilder.CreateTable(
                name: "Clubs",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FoundingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "text", nullable: false),
                    LogoUrl = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clubs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Divisions",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    SportType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW() AT TIME ZONE 'UTC'"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Divisions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NewsArticles",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MainImage = table.Column<string>(type: "text", nullable: true),
                    ContentHtml = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrls = table.Column<string>(type: "text", nullable: false),
                    Author = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SportCategory = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Tags = table.Column<string>(type: "text", nullable: false),
                    IsArchived = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NewsArticles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Persons",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the entity"),
                    FirstName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Address_Street1 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_Street2 = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Address_City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address_PostalCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Address_Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContactInfo_Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    ContactInfo_Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContactInfo_AlternativePhone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsRegistered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "UTC timestamp when the entity was created"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "UTC timestamp when the entity was last updated")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Persons", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_IsActive",
                schema: "common",
                table: "Divisions",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Level",
                schema: "common",
                table: "Divisions",
                column: "Level");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_Name_SportType",
                schema: "common",
                table: "Divisions",
                columns: new[] { "Name", "SportType" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_SportType",
                schema: "common",
                table: "Divisions",
                column: "SportType");

            migrationBuilder.CreateIndex(
                name: "IX_Divisions_SportType_IsActive",
                schema: "common",
                table: "Divisions",
                columns: new[] { "SportType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_News_Author",
                schema: "common",
                table: "NewsArticles",
                column: "Author");

            migrationBuilder.CreateIndex(
                name: "IX_News_Category",
                schema: "common",
                table: "NewsArticles",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_News_CreatedAt",
                schema: "common",
                table: "NewsArticles",
                column: "CreatedAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_News_IsArchived",
                schema: "common",
                table: "NewsArticles",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_News_IsArchived_CreatedAt",
                schema: "common",
                table: "NewsArticles",
                columns: new[] { "IsArchived", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_News_SportCategory",
                schema: "common",
                table: "NewsArticles",
                column: "SportCategory");

            migrationBuilder.CreateIndex(
                name: "IX_Person_Audit",
                schema: "common",
                table: "Persons",
                columns: new[] { "CreatedAt", "UpdatedAt" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Person_CreatedAt",
                schema: "common",
                table: "Persons",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Person_UpdatedAt",
                schema: "common",
                table: "Persons",
                column: "UpdatedAt",
                filter: "\"UpdatedAt\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Clubs",
                schema: "common");

            migrationBuilder.DropTable(
                name: "Divisions",
                schema: "common");

            migrationBuilder.DropTable(
                name: "NewsArticles",
                schema: "common");

            migrationBuilder.DropTable(
                name: "Persons",
                schema: "common");
        }
    }
}
