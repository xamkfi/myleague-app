using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class EnsureSiteSettingsTableExists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"CREATE TABLE IF NOT EXISTS common.""SiteSettings"" (
                    ""Id"" uuid NOT NULL,
                    ""Key"" character varying(150) NOT NULL,
                    ""ValueJson"" text NOT NULL,
                    ""LastModifiedBy"" character varying(100),
                    ""CreatedAt"" timestamp with time zone NOT NULL,
                    ""UpdatedAt"" timestamp with time zone,
                    CONSTRAINT ""PK_SiteSettings"" PRIMARY KEY (""Id"")
                );");

            migrationBuilder.Sql(
                @"CREATE UNIQUE INDEX IF NOT EXISTS ""IX_SiteSettings_Key""
                  ON common.""SiteSettings"" (""Key"");");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP TABLE IF EXISTS common.""SiteSettings"";");
        }
    }
}
