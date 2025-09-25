using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    public partial class AddUserRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Persons_PersonId",
                schema: "common",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_User_PersonId",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PersonId",
                schema: "common",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "role",
                schema: "common",
                table: "Persons");

            migrationBuilder.AddColumn<int>(
                name: "Role",
                schema: "common",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                schema: "common",
                table: "Users");

            migrationBuilder.AddColumn<Guid>(
                name: "PersonId",
                schema: "common",
                table: "Users",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "role",
                schema: "common",
                table: "Persons",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_User_PersonId",
                schema: "common",
                table: "Users",
                column: "PersonId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Persons_PersonId",
                schema: "common",
                table: "Users",
                column: "PersonId",
                principalSchema: "common",
                principalTable: "Persons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
