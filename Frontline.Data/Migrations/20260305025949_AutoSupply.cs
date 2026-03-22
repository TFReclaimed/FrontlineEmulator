using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontline.Data.Migrations
{
    /// <inheritdoc />
    public partial class AutoSupply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastSupplySync",
                table: "Players",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastSupplySync",
                table: "Players");
        }
    }
}
