using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FamilyTree.Migrations
{
    /// <inheritdoc />
    public partial class AddUnionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Relationships_ToPersonId",
                table: "Relationships");

            migrationBuilder.CreateTable(
                name: "Unions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Person1Id = table.Column<long>(type: "bigint", nullable: false),
                    Person2Id = table.Column<long>(type: "bigint", nullable: false),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    DateCreated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false),
                    Archived = table.Column<bool>(type: "boolean", nullable: false),
                    OwnerId = table.Column<long>(type: "bigint", nullable: true),
                    ItemOrder = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Unions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Unions_Persons_Person1Id",
                        column: x => x.Person1Id,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Unions_Persons_Person2Id",
                        column: x => x.Person2Id,
                        principalTable: "Persons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_FromPersonId_RelationshipType",
                table: "Relationships",
                columns: new[] { "FromPersonId", "RelationshipType" });

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ToPersonId_RelationshipType",
                table: "Relationships",
                columns: new[] { "ToPersonId", "RelationshipType" });

            migrationBuilder.CreateIndex(
                name: "IX_Unions_Person1Id_Person2Id_IsActive",
                table: "Unions",
                columns: new[] { "Person1Id", "Person2Id", "IsActive" },
                filter: "\"Deleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_Unions_Person2Id",
                table: "Unions",
                column: "Person2Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Unions");

            migrationBuilder.DropIndex(
                name: "IX_Relationships_FromPersonId_RelationshipType",
                table: "Relationships");

            migrationBuilder.DropIndex(
                name: "IX_Relationships_ToPersonId_RelationshipType",
                table: "Relationships");

            migrationBuilder.CreateIndex(
                name: "IX_Relationships_ToPersonId",
                table: "Relationships",
                column: "ToPersonId");
        }
    }
}
