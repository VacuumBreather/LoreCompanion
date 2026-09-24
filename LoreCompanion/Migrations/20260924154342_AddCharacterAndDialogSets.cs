using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoreCompanion.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterAndDialogSets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Character_Episodes_EpisodeId",
                table: "Character");

            migrationBuilder.DropForeignKey(
                name: "FK_Character_Locations_LocationId",
                table: "Character");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_Character_CharacterId",
                table: "Dialog");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_Episodes_EpisodeId",
                table: "Dialog");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialog_Locations_LocationId",
                table: "Dialog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dialog",
                table: "Dialog");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Character",
                table: "Character");

            migrationBuilder.RenameTable(
                name: "Dialog",
                newName: "Dialogs");

            migrationBuilder.RenameTable(
                name: "Character",
                newName: "Characters");

            migrationBuilder.RenameIndex(
                name: "IX_Dialog_LocationId",
                table: "Dialogs",
                newName: "IX_Dialogs_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Dialog_EpisodeId",
                table: "Dialogs",
                newName: "IX_Dialogs_EpisodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Dialog_CharacterId",
                table: "Dialogs",
                newName: "IX_Dialogs_CharacterId");

            migrationBuilder.RenameIndex(
                name: "IX_Character_Name",
                table: "Characters",
                newName: "IX_Characters_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Character_LocationId",
                table: "Characters",
                newName: "IX_Characters_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Character_EpisodeId",
                table: "Characters",
                newName: "IX_Characters_EpisodeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dialogs",
                table: "Dialogs",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Characters",
                table: "Characters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Episodes_EpisodeId",
                table: "Characters",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_Locations_LocationId",
                table: "Characters",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Characters_CharacterId",
                table: "Dialogs",
                column: "CharacterId",
                principalTable: "Characters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Episodes_EpisodeId",
                table: "Dialogs",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialogs_Locations_LocationId",
                table: "Dialogs",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Episodes_EpisodeId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_Locations_LocationId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Characters_CharacterId",
                table: "Dialogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Episodes_EpisodeId",
                table: "Dialogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Dialogs_Locations_LocationId",
                table: "Dialogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Dialogs",
                table: "Dialogs");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Characters",
                table: "Characters");

            migrationBuilder.RenameTable(
                name: "Dialogs",
                newName: "Dialog");

            migrationBuilder.RenameTable(
                name: "Characters",
                newName: "Character");

            migrationBuilder.RenameIndex(
                name: "IX_Dialogs_LocationId",
                table: "Dialog",
                newName: "IX_Dialog_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Dialogs_EpisodeId",
                table: "Dialog",
                newName: "IX_Dialog_EpisodeId");

            migrationBuilder.RenameIndex(
                name: "IX_Dialogs_CharacterId",
                table: "Dialog",
                newName: "IX_Dialog_CharacterId");

            migrationBuilder.RenameIndex(
                name: "IX_Characters_Name",
                table: "Character",
                newName: "IX_Character_Name");

            migrationBuilder.RenameIndex(
                name: "IX_Characters_LocationId",
                table: "Character",
                newName: "IX_Character_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Characters_EpisodeId",
                table: "Character",
                newName: "IX_Character_EpisodeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Dialog",
                table: "Dialog",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Character",
                table: "Character",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Character_Episodes_EpisodeId",
                table: "Character",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Character_Locations_LocationId",
                table: "Character",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_Character_CharacterId",
                table: "Dialog",
                column: "CharacterId",
                principalTable: "Character",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_Episodes_EpisodeId",
                table: "Dialog",
                column: "EpisodeId",
                principalTable: "Episodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Dialog_Locations_LocationId",
                table: "Dialog",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
