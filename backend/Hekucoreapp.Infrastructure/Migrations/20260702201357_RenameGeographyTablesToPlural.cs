using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Hekucoreapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameGeographyTablesToPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_person_person_id",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_city_country_country_id",
                table: "city");

            migrationBuilder.DropForeignKey(
                name: "FK_city_state_state_id",
                table: "city");

            migrationBuilder.DropForeignKey(
                name: "FK_person_city_city_id",
                table: "person");

            migrationBuilder.DropForeignKey(
                name: "FK_person_country_country_id",
                table: "person");

            migrationBuilder.DropForeignKey(
                name: "FK_person_state_state_id",
                table: "person");

            migrationBuilder.DropForeignKey(
                name: "FK_state_country_country_id",
                table: "state");

            migrationBuilder.DropPrimaryKey(
                name: "PK_state",
                table: "state");

            migrationBuilder.DropPrimaryKey(
                name: "PK_person",
                table: "person");

            migrationBuilder.DropPrimaryKey(
                name: "PK_country",
                table: "country");

            migrationBuilder.DropPrimaryKey(
                name: "PK_city",
                table: "city");

            migrationBuilder.RenameTable(
                name: "state",
                newName: "states");

            migrationBuilder.RenameTable(
                name: "person",
                newName: "persons");

            migrationBuilder.RenameTable(
                name: "country",
                newName: "countries");

            migrationBuilder.RenameTable(
                name: "city",
                newName: "cities");

            migrationBuilder.RenameIndex(
                name: "IX_state_country_id",
                table: "states",
                newName: "IX_states_country_id");

            migrationBuilder.RenameIndex(
                name: "IX_person_state_id",
                table: "persons",
                newName: "IX_persons_state_id");

            migrationBuilder.RenameIndex(
                name: "IX_person_document_type_document_id",
                table: "persons",
                newName: "IX_persons_document_type_document_id");

            migrationBuilder.RenameIndex(
                name: "IX_person_country_id",
                table: "persons",
                newName: "IX_persons_country_id");

            migrationBuilder.RenameIndex(
                name: "IX_person_city_id",
                table: "persons",
                newName: "IX_persons_city_id");

            migrationBuilder.RenameIndex(
                name: "IX_city_state_id",
                table: "cities",
                newName: "IX_cities_state_id");

            migrationBuilder.RenameIndex(
                name: "IX_city_country_id",
                table: "cities",
                newName: "IX_cities_country_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_states",
                table: "states",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_persons",
                table: "persons",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_countries",
                table: "countries",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_cities",
                table: "cities",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_persons_person_id",
                table: "AspNetUsers",
                column: "person_id",
                principalTable: "persons",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_cities_countries_country_id",
                table: "cities",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_cities_states_state_id",
                table: "cities",
                column: "state_id",
                principalTable: "states",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_persons_cities_city_id",
                table: "persons",
                column: "city_id",
                principalTable: "cities",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_persons_countries_country_id",
                table: "persons",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_persons_states_state_id",
                table: "persons",
                column: "state_id",
                principalTable: "states",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_states_countries_country_id",
                table: "states",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_persons_person_id",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_cities_countries_country_id",
                table: "cities");

            migrationBuilder.DropForeignKey(
                name: "FK_cities_states_state_id",
                table: "cities");

            migrationBuilder.DropForeignKey(
                name: "FK_persons_cities_city_id",
                table: "persons");

            migrationBuilder.DropForeignKey(
                name: "FK_persons_countries_country_id",
                table: "persons");

            migrationBuilder.DropForeignKey(
                name: "FK_persons_states_state_id",
                table: "persons");

            migrationBuilder.DropForeignKey(
                name: "FK_states_countries_country_id",
                table: "states");

            migrationBuilder.DropPrimaryKey(
                name: "PK_states",
                table: "states");

            migrationBuilder.DropPrimaryKey(
                name: "PK_persons",
                table: "persons");

            migrationBuilder.DropPrimaryKey(
                name: "PK_countries",
                table: "countries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_cities",
                table: "cities");

            migrationBuilder.RenameTable(
                name: "states",
                newName: "state");

            migrationBuilder.RenameTable(
                name: "persons",
                newName: "person");

            migrationBuilder.RenameTable(
                name: "countries",
                newName: "country");

            migrationBuilder.RenameTable(
                name: "cities",
                newName: "city");

            migrationBuilder.RenameIndex(
                name: "IX_states_country_id",
                table: "state",
                newName: "IX_state_country_id");

            migrationBuilder.RenameIndex(
                name: "IX_persons_state_id",
                table: "person",
                newName: "IX_person_state_id");

            migrationBuilder.RenameIndex(
                name: "IX_persons_document_type_document_id",
                table: "person",
                newName: "IX_person_document_type_document_id");

            migrationBuilder.RenameIndex(
                name: "IX_persons_country_id",
                table: "person",
                newName: "IX_person_country_id");

            migrationBuilder.RenameIndex(
                name: "IX_persons_city_id",
                table: "person",
                newName: "IX_person_city_id");

            migrationBuilder.RenameIndex(
                name: "IX_cities_state_id",
                table: "city",
                newName: "IX_city_state_id");

            migrationBuilder.RenameIndex(
                name: "IX_cities_country_id",
                table: "city",
                newName: "IX_city_country_id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_state",
                table: "state",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_person",
                table: "person",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_country",
                table: "country",
                column: "id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_city",
                table: "city",
                column: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_person_person_id",
                table: "AspNetUsers",
                column: "person_id",
                principalTable: "person",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_city_country_country_id",
                table: "city",
                column: "country_id",
                principalTable: "country",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_city_state_state_id",
                table: "city",
                column: "state_id",
                principalTable: "state",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_person_city_city_id",
                table: "person",
                column: "city_id",
                principalTable: "city",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_person_country_country_id",
                table: "person",
                column: "country_id",
                principalTable: "country",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_person_state_state_id",
                table: "person",
                column: "state_id",
                principalTable: "state",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_state_country_country_id",
                table: "state",
                column: "country_id",
                principalTable: "country",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
