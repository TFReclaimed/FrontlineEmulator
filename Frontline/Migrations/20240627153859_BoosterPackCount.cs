using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontline.Migrations
{
    /// <inheritdoc />
    public partial class BoosterPackCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BoosterPackCount",
                table: "Players",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BoosterPackCount",
                table: "Players");
        }
    }
}
