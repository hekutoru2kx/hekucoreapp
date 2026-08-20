using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeletedAccountsTombstone : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "deleted_accounts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    normalized_email = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_deleted_accounts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_deleted_accounts_normalized_email",
                table: "deleted_accounts",
                column: "normalized_email");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "deleted_accounts");
        }
    }
}
