using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoreCompanion.Migrations
{
    /// <inheritdoc />
    public partial class MakeNamesUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Locations_Name",
                table: "Locations",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Items_Name",
                table: "Items",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_Name",
                table: "Episodes",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_VideoKey",
                table: "Episodes",
                column: "VideoKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Locations_Name",
                table: "Locations");

            migrationBuilder.DropIndex(
                name: "IX_Items_Name",
                table: "Items");

            migrationBuilder.DropIndex(
                name: "IX_Episodes_Name",
                table: "Episodes");

            migrationBuilder.DropIndex(
                name: "IX_Episodes_VideoKey",
                table: "Episodes");
        }
    }
}
