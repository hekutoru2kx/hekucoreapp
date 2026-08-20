using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonAndGeography : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "person_id",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "country",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    iso2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    iso3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phone_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    capital = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    currency = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    region = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    subregion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_country", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "state",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    state_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    country_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_state", x => x.id);
                    table.ForeignKey(
                        name: "FK_state_country_country_id",
                        column: x => x.country_id,
                        principalTable: "country",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "city",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    state_id = table.Column<int>(type: "int", nullable: false),
                    country_id = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_city", x => x.id);
                    table.ForeignKey(
                        name: "FK_city_country_country_id",
                        column: x => x.country_id,
                        principalTable: "country",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_city_state_state_id",
                        column: x => x.state_id,
                        principalTable: "state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "person",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    first_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    last_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    birthday = table.Column<DateTime>(type: "datetime2", nullable: true),
                    document_type = table.Column<int>(type: "int", nullable: true),
                    document_id = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone_extension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gender = table.Column<int>(type: "int", nullable: true),
                    country_id = table.Column<int>(type: "int", nullable: true),
                    state_id = table.Column<int>(type: "int", nullable: true),
                    city_id = table.Column<int>(type: "int", nullable: true),
                    postal_code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    created_by = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_by = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_person", x => x.id);
                    table.ForeignKey(
                        name: "FK_person_city_city_id",
                        column: x => x.city_id,
                        principalTable: "city",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_person_country_country_id",
                        column: x => x.country_id,
                        principalTable: "country",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_person_state_state_id",
                        column: x => x.state_id,
                        principalTable: "state",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_person_id",
                table: "AspNetUsers",
                column: "person_id",
                unique: true,
                filter: "[person_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_city_country_id",
                table: "city",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_city_state_id",
                table: "city",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "IX_person_city_id",
                table: "person",
                column: "city_id");

            migrationBuilder.CreateIndex(
                name: "IX_person_country_id",
                table: "person",
                column: "country_id");

            migrationBuilder.CreateIndex(
                name: "IX_person_document_type_document_id",
                table: "person",
                columns: new[] { "document_type", "document_id" },
                unique: true,
                filter: "[document_type] IS NOT NULL AND [document_id] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_person_state_id",
                table: "person",
                column: "state_id");

            migrationBuilder.CreateIndex(
                name: "IX_state_country_id",
                table: "state",
                column: "country_id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_person_person_id",
                table: "AspNetUsers",
                column: "person_id",
                principalTable: "person",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_person_person_id",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "person");

            migrationBuilder.DropTable(
                name: "city");

            migrationBuilder.DropTable(
                name: "state");

            migrationBuilder.DropTable(
                name: "country");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_person_id",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "person_id",
                table: "AspNetUsers");
        }
    }
}
