using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontline.Migrations
{
    /// <inheritdoc />
    public partial class Dropships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dropships",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DropshipId = table.Column<int>(type: "int", nullable: false),
                    SlotIndex = table.Column<int>(type: "int", nullable: false),
                    ItemId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dropships", x => new { x.UserId, x.DropshipId, x.SlotIndex });
                    table.ForeignKey(
                        name: "FK_Dropships_Items_UserId_ItemId",
                        columns: x => new { x.UserId, x.ItemId },
                        principalTable: "Items",
                        principalColumns: new[] { "UserId", "ItemId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Dropships_Players_UserId",
                        column: x => x.UserId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Dropships_UserId_ItemId",
                table: "Dropships",
                columns: new[] { "UserId", "ItemId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Dropships");
        }
    }
}
