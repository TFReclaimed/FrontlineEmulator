using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontline.Data.Migrations
{
    /// <inheritdoc />
    public partial class MissionSynergies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Synergy1Success",
                table: "ActiveMissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Synergy2Success",
                table: "ActiveMissions",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Synergy1Success",
                table: "ActiveMissions");

            migrationBuilder.DropColumn(
                name: "Synergy2Success",
                table: "ActiveMissions");
        }
    }
}
