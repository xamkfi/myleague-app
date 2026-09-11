using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class EnsureRulesSectionsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // AddRulesSection was generated with an empty Up(); databases that already
            // applied that migration need the table created without re-running it.
            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS common."RulesSections" (
                    "Id" uuid NOT NULL,
                    "Title" character varying(200) NOT NULL,
                    "SortOrder" integer NOT NULL,
                    "SectionType" character varying(50) NOT NULL,
                    "ParentSectionId" uuid NULL,
                    "ContentHtml" text NOT NULL,
                    "LastModifiedBy" character varying(256) NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NULL,
                    CONSTRAINT "PK_RulesSections" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_RulesSections_RulesSections_ParentSectionId" FOREIGN KEY ("ParentSectionId")
                        REFERENCES common."RulesSections" ("Id") ON DELETE RESTRICT
                );

                CREATE INDEX IF NOT EXISTS "IX_RulesSections_ParentSectionId"
                    ON common."RulesSections" ("ParentSectionId");

                CREATE INDEX IF NOT EXISTS "IX_RulesSections_SortOrder"
                    ON common."RulesSections" ("SortOrder");
                """);
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
