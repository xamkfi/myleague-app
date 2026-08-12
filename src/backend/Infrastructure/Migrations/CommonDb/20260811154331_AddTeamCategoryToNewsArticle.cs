using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddTeamCategoryToNewsArticle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamCategory",
                schema: "common",
                table: "NewsArticles",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_News_TeamCategory",
                schema: "common",
                table: "NewsArticles",
                column: "TeamCategory");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_News_TeamCategory",
                schema: "common",
                table: "NewsArticles");

            migrationBuilder.DropColumn(
                name: "TeamCategory",
                schema: "common",
                table: "NewsArticles");
        }
    }
}
