using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                table: "persons",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "email",
                table: "persons");
        }
    }
}
