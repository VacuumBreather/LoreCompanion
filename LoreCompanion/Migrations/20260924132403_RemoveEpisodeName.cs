using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoreCompanion.Migrations
{
    /// <inheritdoc />
    public partial class RemoveEpisodeName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Episodes_Name",
                table: "Episodes");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Episodes");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_Number",
                table: "Episodes",
                column: "Number",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_Episodes_Number_Min",
                table: "Episodes",
                sql: "\"Number\" >= 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Episodes_Number",
                table: "Episodes");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Episodes_Number_Min",
                table: "Episodes");

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Episodes",
                type: "TEXT",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Episodes_Name",
                table: "Episodes",
                column: "Name",
                unique: true);
        }
    }
}
