using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "content_allowed_content_types",
                table: "app_settings",
                type: "text",
                nullable: false,
                defaultValue: "image/jpeg,image/png,image/webp");

            migrationBuilder.AddColumn<int>(
                name: "content_avatar_max_dimension",
                table: "app_settings",
                type: "integer",
                nullable: false,
                defaultValue: 512);

            migrationBuilder.AddColumn<long>(
                name: "content_max_bytes",
                table: "app_settings",
                type: "bigint",
                nullable: false,
                defaultValue: 5242880L);

            migrationBuilder.AddColumn<int>(
                name: "content_max_image_dimension",
                table: "app_settings",
                type: "integer",
                nullable: false,
                defaultValue: 2048);

            migrationBuilder.CreateTable(
                name: "stored_files",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    container = table.Column<string>(type: "text", nullable: false),
                    blob_name = table.Column<string>(type: "text", nullable: false),
                    original_file_name = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    byte_size = table.Column<long>(type: "bigint", nullable: false),
                    sha256 = table.Column<string>(type: "text", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stored_files", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "content_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    owner_type = table.Column<string>(type: "text", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    slot = table.Column<string>(type: "text", nullable: true),
                    kind = table.Column<int>(type: "integer", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    body = table.Column<string>(type: "text", nullable: true),
                    url = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: true),
                    stored_file_id = table.Column<int>(type: "integer", nullable: true),
                    display_order = table.Column<int>(type: "integer", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    archived_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_content_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_content_items_stored_files_stored_file_id",
                        column: x => x.stored_file_id,
                        principalTable: "stored_files",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_content_items_created_at",
                table: "content_items",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_content_items_owner_type_owner_id_display_order",
                table: "content_items",
                columns: new[] { "owner_type", "owner_id", "display_order" });

            migrationBuilder.CreateIndex(
                name: "ix_content_items_owner_type_owner_id_slot",
                table: "content_items",
                columns: new[] { "owner_type", "owner_id", "slot" },
                unique: true,
                filter: "slot IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_content_items_stored_file_id",
                table: "content_items",
                column: "stored_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_content_items_updated_at",
                table: "content_items",
                column: "updated_at");

            migrationBuilder.CreateIndex(
                name: "ix_stored_files_created_at",
                table: "stored_files",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_stored_files_sha256",
                table: "stored_files",
                column: "sha256");

            migrationBuilder.CreateIndex(
                name: "ix_stored_files_updated_at",
                table: "stored_files",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_items");

            migrationBuilder.DropTable(
                name: "stored_files");

            migrationBuilder.DropColumn(
                name: "content_allowed_content_types",
                table: "app_settings");

            migrationBuilder.DropColumn(
                name: "content_avatar_max_dimension",
                table: "app_settings");

            migrationBuilder.DropColumn(
                name: "content_max_bytes",
                table: "app_settings");

            migrationBuilder.DropColumn(
                name: "content_max_image_dimension",
                table: "app_settings");
        }
    }
}
