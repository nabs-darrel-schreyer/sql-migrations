using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SqlMigrations.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SqlMigrationsPersistence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FirstName",
                schema: "test",
                table: "Person",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LastName",
                schema: "test",
                table: "Person",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FirstName",
                schema: "test",
                table: "Person");

            migrationBuilder.DropColumn(
                name: "LastName",
                schema: "test",
                table: "Person");
        }
    }
}
