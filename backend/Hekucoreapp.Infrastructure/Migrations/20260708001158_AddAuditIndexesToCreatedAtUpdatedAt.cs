using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditIndexesToCreatedAtUpdatedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_persons_created_at",
                table: "persons",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_persons_updated_at",
                table: "persons",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_deleted_accounts_created_at",
                table: "deleted_accounts",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_deleted_accounts_updated_at",
                table: "deleted_accounts",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_persons_created_at",
                table: "persons");

            migrationBuilder.DropIndex(
                name: "IX_persons_updated_at",
                table: "persons");

            migrationBuilder.DropIndex(
                name: "IX_deleted_accounts_created_at",
                table: "deleted_accounts");

            migrationBuilder.DropIndex(
                name: "IX_deleted_accounts_updated_at",
                table: "deleted_accounts");
        }
    }
}
