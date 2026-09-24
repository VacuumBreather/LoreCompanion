using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoreCompanion.Migrations
{
    /// <inheritdoc />
    public partial class AddEpisodeNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Episodes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Number",
                table: "Episodes");
        }
    }
}
