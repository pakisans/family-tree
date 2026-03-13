using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FamilyTree.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyEntity2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Families_FamilyId",
                table: "Persons");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Families_FamilyId",
                table: "Persons",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Families_FamilyId",
                table: "Persons");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Families_FamilyId",
                table: "Persons",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id");
        }
    }
}
