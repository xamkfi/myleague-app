using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyLeague.Infrastructure.Migrations.CommonDb
{
    /// <inheritdoc />
    /// <remarks>
    /// Marker migration for relaxing the owned-type ContactInfo.Email requirement on Person.
    ///
    /// The underlying <c>ContactInfo_Email</c> column has been <c>nullable: true</c> at the DB
    /// level since <c>20250623082413_InitialCreate</c>. The only thing that changed in this
    /// release was the EF model configuration (<c>IsRequired()</c> -&gt; <c>IsRequired(false)</c>)
    /// and the domain validators, so no <c>ALTER COLUMN</c> statement is needed. The migration
    /// exists so the model snapshot is regenerated and downstream environments record that the
    /// shift to optional email took place here.
    /// </remarks>
    public partial class MakePersonContactEmailOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
