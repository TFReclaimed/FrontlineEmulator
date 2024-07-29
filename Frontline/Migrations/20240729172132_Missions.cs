using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontline.Migrations
{
    /// <inheritdoc />
    public partial class Missions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveMissions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MissionKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Start = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    RequiredCardItemId = table.Column<int>(type: "int", nullable: false),
                    BonusCard1ItemId = table.Column<int>(type: "int", nullable: true),
                    BonusCard2ItemId = table.Column<int>(type: "int", nullable: true),
                    Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Bonus1Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Bonus2Success = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Casualty = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Bonus1Casualty = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Bonus2Casualty = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveMissions", x => new { x.UserId, x.MissionKey });
                    table.ForeignKey(
                        name: "FK_ActiveMissions_Items_UserId_BonusCard1ItemId",
                        columns: x => new { x.UserId, x.BonusCard1ItemId },
                        principalTable: "Items",
                        principalColumns: new[] { "UserId", "ItemId" });
                    table.ForeignKey(
                        name: "FK_ActiveMissions_Items_UserId_BonusCard2ItemId",
                        columns: x => new { x.UserId, x.BonusCard2ItemId },
                        principalTable: "Items",
                        principalColumns: new[] { "UserId", "ItemId" });
                    table.ForeignKey(
                        name: "FK_ActiveMissions_Items_UserId_RequiredCardItemId",
                        columns: x => new { x.UserId, x.RequiredCardItemId },
                        principalTable: "Items",
                        principalColumns: new[] { "UserId", "ItemId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActiveMissions_Players_UserId",
                        column: x => x.UserId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FinishedMissions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MissionKey = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinishedMissions", x => new { x.UserId, x.MissionKey });
                    table.ForeignKey(
                        name: "FK_FinishedMissions_Players_UserId",
                        column: x => x.UserId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ActiveMissions_UserId_BonusCard1ItemId",
                table: "ActiveMissions",
                columns: new[] { "UserId", "BonusCard1ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveMissions_UserId_BonusCard2ItemId",
                table: "ActiveMissions",
                columns: new[] { "UserId", "BonusCard2ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_ActiveMissions_UserId_RequiredCardItemId",
                table: "ActiveMissions",
                columns: new[] { "UserId", "RequiredCardItemId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveMissions");

            migrationBuilder.DropTable(
                name: "FinishedMissions");
        }
    }
}
