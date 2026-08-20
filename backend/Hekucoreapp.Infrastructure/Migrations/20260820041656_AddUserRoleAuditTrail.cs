using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRoleAuditTrail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    user_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    role_id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    starts_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    expires_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    revoked_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_roles", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_roles_AspNetRoles_role_id",
                        column: x => x.role_id,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_roles_AspNetUsers_user_id",
                        column: x => x.user_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_created_at",
                table: "user_roles",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_role_id",
                table: "user_roles",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_updated_at",
                table: "user_roles",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_user_id_role_id",
                table: "user_roles",
                columns: new[] { "user_id", "role_id" },
                unique: true,
                filter: "[revoked_at] IS NULL");

            // Backfill existing AspNetUserRoles grants into the new audit-trailed table so no
            // one loses access at cutover. Original grant date/actor predates this table, so
            // CreatedBy/UpdatedBy are stamped 'migration' rather than guessed. Must run before
            // AspNetUserRoles is dropped below.
            migrationBuilder.Sql(@"
                INSERT INTO user_roles (user_id, role_id, created_at, created_by, updated_at, updated_by)
                SELECT UserId, RoleId, SYSUTCDATETIME(), 'migration', SYSUTCDATETIME(), 'migration'
                FROM AspNetUserRoles;
            ");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            // Restore currently-active grants (the filtered unique index on user_roles guarantees
            // at most one per user/role pair, so this can't collide with AspNetUserRoles' PK).
            // History (revoked/scheduled rows) has no equivalent in the old table's shape and is
            // dropped along with user_roles below.
            migrationBuilder.Sql(@"
                INSERT INTO AspNetUserRoles (UserId, RoleId)
                SELECT user_id, role_id FROM user_roles WHERE revoked_at IS NULL;
            ");

            migrationBuilder.DropTable(
                name: "user_roles");
        }
    }
}
