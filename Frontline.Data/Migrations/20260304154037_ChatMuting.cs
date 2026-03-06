using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontline.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChatMuting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ChatBanEnd",
                table: "Players",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatBanEnd",
                table: "Players");
        }
    }
}
