using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FamilyTree.Migrations
{
    /// <inheritdoc />
    public partial class AddFamilyEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "FamilyId",
                table: "Persons",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Families",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OriginPlace = table.Column<string>(type: "text", nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false),
                    AllowPublicTree = table.Column<bool>(type: "boolean", nullable: false),
                    SeoTitle = table.Column<string>(type: "text", nullable: true),
                    SeoDescription = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    ItemOrder = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Families", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Persons_FamilyId",
                table: "Persons",
                column: "FamilyId");

            migrationBuilder.CreateIndex(
                name: "IX_Families_Slug",
                table: "Families",
                column: "Slug",
                unique: true,
                filter: "\"Deleted\" = false");

            migrationBuilder.AddForeignKey(
                name: "FK_Persons_Families_FamilyId",
                table: "Persons",
                column: "FamilyId",
                principalTable: "Families",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Persons_Families_FamilyId",
                table: "Persons");

            migrationBuilder.DropTable(
                name: "Families");

            migrationBuilder.DropIndex(
                name: "IX_Persons_FamilyId",
                table: "Persons");

            migrationBuilder.DropColumn(
                name: "FamilyId",
                table: "Persons");
        }
    }
}
