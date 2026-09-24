using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoreCompanion.Migrations
{
    /// <inheritdoc />
    public partial class ChangeItemLocationToForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Location",
                table: "Items");

            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Items",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_LocationId",
                table: "Items",
                column: "LocationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Items_Locations_LocationId",
                table: "Items",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Items_Locations_LocationId",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Items_LocationId",
                table: "Items");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Items");

            migrationBuilder.AddColumn<string>(
                name: "Location",
                table: "Items",
                type: "TEXT",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
