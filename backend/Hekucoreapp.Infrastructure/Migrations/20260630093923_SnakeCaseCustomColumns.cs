using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SnakeCaseCustomColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "FullName",
                table: "AspNetUsers",
                newName: "fullname");

            migrationBuilder.RenameColumn(
                name: "PreferredTheme",
                table: "AspNetUsers",
                newName: "preferred_theme");

            migrationBuilder.RenameColumn(
                name: "PreferredLanguage",
                table: "AspNetUsers",
                newName: "preferred_language");

            migrationBuilder.RenameColumn(
                name: "MustChangePassword",
                table: "AspNetUsers",
                newName: "must_change_password");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "AspNetUsers",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "AspNetUsers",
                newName: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "fullname",
                table: "AspNetUsers",
                newName: "FullName");

            migrationBuilder.RenameColumn(
                name: "preferred_theme",
                table: "AspNetUsers",
                newName: "PreferredTheme");

            migrationBuilder.RenameColumn(
                name: "preferred_language",
                table: "AspNetUsers",
                newName: "PreferredLanguage");

            migrationBuilder.RenameColumn(
                name: "must_change_password",
                table: "AspNetUsers",
                newName: "MustChangePassword");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "AspNetUsers",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "AspNetUsers",
                newName: "CreatedAt");
        }
    }
}
