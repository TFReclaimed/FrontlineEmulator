using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frontline.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Guilds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    Description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AvatarId = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    Mode = table.Column<int>(type: "integer", nullable: false),
                    Locale = table.Column<int>(type: "integer", nullable: false),
                    MaxNumberOfMembers = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guilds", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    AvatarId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    DropshipId = table.Column<int>(type: "integer", nullable: false),
                    Credits = table.Column<int>(type: "integer", nullable: false),
                    Supply = table.Column<int>(type: "integer", nullable: false),
                    Trophies = table.Column<int>(type: "integer", nullable: false),
                    Tokens = table.Column<int>(type: "integer", nullable: false),
                    Wins = table.Column<int>(type: "integer", nullable: false),
                    HighestTrophies = table.Column<int>(type: "integer", nullable: false),
                    MissionsComplete = table.Column<int>(type: "integer", nullable: false),
                    MatchesPlayed = table.Column<int>(type: "integer", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    BoosterPackCount = table.Column<int>(type: "integer", nullable: false),
                    LastGiftSent = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinishedMissions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MissionKey = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "GuildMembers",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    GuildId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rank = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildMembers", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_GuildMembers_Guilds_GuildId",
                        column: x => x.GuildId,
                        principalTable: "Guilds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GuildMembers_Players_UserId",
                        column: x => x.UserId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    TemplateId = table.Column<int>(type: "integer", nullable: false),
                    Xp = table.Column<int>(type: "integer", nullable: false),
                    Rank = table.Column<short>(type: "smallint", nullable: false),
                    Casualty = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => new { x.UserId, x.ItemId });
                    table.ForeignKey(
                        name: "FK_Items_Players_UserId",
                        column: x => x.UserId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ActiveMissions",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MissionKey = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: false),
                    Start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequiredCardItemId = table.Column<int>(type: "integer", nullable: false),
                    BonusCard1ItemId = table.Column<int>(type: "integer", nullable: true),
                    BonusCard2ItemId = table.Column<int>(type: "integer", nullable: true),
                    Success = table.Column<bool>(type: "boolean", nullable: false),
                    Bonus1Success = table.Column<bool>(type: "boolean", nullable: false),
                    Bonus2Success = table.Column<bool>(type: "boolean", nullable: false),
                    Casualty = table.Column<bool>(type: "boolean", nullable: false),
                    Bonus1Casualty = table.Column<bool>(type: "boolean", nullable: false),
                    Bonus2Casualty = table.Column<bool>(type: "boolean", nullable: false)
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
                });

            migrationBuilder.CreateTable(
                name: "Dropships",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DropshipId = table.Column<int>(type: "integer", nullable: false),
                    SlotIndex = table.Column<int>(type: "integer", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false)
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
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Dropships_UserId_ItemId",
                table: "Dropships",
                columns: new[] { "UserId", "ItemId" });

            migrationBuilder.CreateIndex(
                name: "IX_GuildMembers_GuildId",
                table: "GuildMembers",
                column: "GuildId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveMissions");

            migrationBuilder.DropTable(
                name: "Dropships");

            migrationBuilder.DropTable(
                name: "FinishedMissions");

            migrationBuilder.DropTable(
                name: "GuildMembers");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Guilds");

            migrationBuilder.DropTable(
                name: "Players");
        }
    }
}
