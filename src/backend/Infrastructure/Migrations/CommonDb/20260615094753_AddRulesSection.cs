using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddRulesSection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RulesSections",
                schema: "common",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
                    SectionType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParentSectionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContentHtml = table.Column<string>(type: "text", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RulesSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RulesSections_RulesSections_ParentSectionId",
                        column: x => x.ParentSectionId,
                        principalSchema: "common",
                        principalTable: "RulesSections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RulesSections_ParentSectionId",
                schema: "common",
                table: "RulesSections",
                column: "ParentSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_RulesSections_SortOrder",
                schema: "common",
                table: "RulesSections",
                column: "SortOrder");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RulesSections",
                schema: "common");
        }
    }
}
