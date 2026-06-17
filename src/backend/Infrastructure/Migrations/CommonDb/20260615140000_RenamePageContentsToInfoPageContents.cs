using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class RenamePageContentsToInfoPageContents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.tables
                        WHERE table_schema = 'common'
                          AND table_name = 'PageContents'
                    ) THEN
                        ALTER TABLE common."PageContents" RENAME TO "InfoPageContents";
                    END IF;
                END $$;
                """);

            migrationBuilder.Sql("""
                CREATE TABLE IF NOT EXISTS common."InfoPageContents" (
                    "Id" uuid NOT NULL,
                    "PageSlug" character varying(200) NOT NULL,
                    "Title" character varying(500) NOT NULL,
                    "ContentHtml" text NOT NULL,
                    "LastModifiedBy" character varying(256) NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NULL,
                    CONSTRAINT "PK_InfoPageContents" PRIMARY KEY ("Id")
                );

                CREATE UNIQUE INDEX IF NOT EXISTS "IX_InfoPageContents_PageSlug"
                    ON common."InfoPageContents" ("PageSlug");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.tables
                        WHERE table_schema = 'common'
                          AND table_name = 'InfoPageContents'
                    ) THEN
                        ALTER TABLE common."InfoPageContents" RENAME TO "PageContents";
                    END IF;
                END $$;
                """);
        }
    }
}
