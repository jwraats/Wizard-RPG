using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WizardRPG.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    MagicBonus = table.Column<int>(type: "integer", nullable: false),
                    StrengthBonus = table.Column<int>(type: "integer", nullable: false),
                    WisdomBonus = table.Column<int>(type: "integer", nullable: false),
                    SpeedBonus = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    GoldCoins = table.Column<long>(type: "bigint", nullable: false, defaultValue: 0L),
                    Level = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Experience = table.Column<long>(type: "bigint", nullable: false),
                    MagicPower = table.Column<int>(type: "integer", nullable: false),
                    Strength = table.Column<int>(type: "integer", nullable: false),
                    Wisdom = table.Column<int>(type: "integer", nullable: false),
                    Speed = table.Column<int>(type: "integer", nullable: false),
                    ReferralCode = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsAdmin = table.Column<bool>(type: "boolean", nullable: false),
                    RefreshToken = table.Column<string>(type: "text", nullable: true),
                    RefreshTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Spells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ManaCost = table.Column<int>(type: "integer", nullable: false),
                    BaseDamage = table.Column<int>(type: "integer", nullable: false),
                    Effect = table.Column<string>(type: "text", nullable: false),
                    Element = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Spells", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BankAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    GoldBalance = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankAccounts_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    AcquiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BankItems_Items_ItemId",
                        column: x => x.ItemId,
                        principalTable: "Items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BankItems_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Battles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChallengerId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WinnerId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    FinishedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NarratorStory = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Battles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Battles_Players_ChallengerId",
                        column: x => x.ChallengerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Battles_Players_DefenderId",
                        column: x => x.DefenderId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Battles_Players_WinnerId",
                        column: x => x.WinnerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Fellowships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReferralCode = table.Column<string>(type: "text", nullable: false),
                    GoldPerHour = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Fellowships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Fellowships_Players_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BattleTurns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BattleId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttackerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpellId = table.Column<Guid>(type: "uuid", nullable: false),
                    DamageDealt = table.Column<int>(type: "integer", nullable: false),
                    TurnNumber = table.Column<int>(type: "integer", nullable: false),
                    Narrative = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattleTurns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BattleTurns_Battles_BattleId",
                        column: x => x.BattleId,
                        principalTable: "Battles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BattleTurns_Players_AttackerId",
                        column: x => x.AttackerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BattleTurns_Spells_SpellId",
                        column: x => x.SpellId,
                        principalTable: "Spells",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FellowshipMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FellowshipId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContributionPercent = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FellowshipMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FellowshipMembers_Fellowships_FellowshipId",
                        column: x => x.FellowshipId,
                        principalTable: "Fellowships",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FellowshipMembers_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BroomBets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Payout = table.Column<long>(type: "bigint", nullable: false),
                    PlacedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BroomBets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BroomBets_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BroomLeagues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    WinnerTeamId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BroomLeagues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BroomTeams",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Odds = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    LeagueId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BroomTeams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BroomTeams_BroomLeagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "BroomLeagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_PlayerId",
                table: "BankAccounts",
                column: "PlayerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BankItems_ItemId",
                table: "BankItems",
                column: "ItemId");

            migrationBuilder.CreateIndex(
                name: "IX_BankItems_PlayerId",
                table: "BankItems",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_ChallengerId",
                table: "Battles",
                column: "ChallengerId");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_DefenderId",
                table: "Battles",
                column: "DefenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Battles_WinnerId",
                table: "Battles",
                column: "WinnerId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleTurns_AttackerId",
                table: "BattleTurns",
                column: "AttackerId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleTurns_BattleId",
                table: "BattleTurns",
                column: "BattleId");

            migrationBuilder.CreateIndex(
                name: "IX_BattleTurns_SpellId",
                table: "BattleTurns",
                column: "SpellId");

            migrationBuilder.CreateIndex(
                name: "IX_BroomBets_LeagueId",
                table: "BroomBets",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_BroomBets_PlayerId",
                table: "BroomBets",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_BroomBets_TeamId",
                table: "BroomBets",
                column: "TeamId");

            migrationBuilder.CreateIndex(
                name: "IX_BroomLeagues_WinnerTeamId",
                table: "BroomLeagues",
                column: "WinnerTeamId");

            migrationBuilder.CreateIndex(
                name: "IX_BroomTeams_LeagueId",
                table: "BroomTeams",
                column: "LeagueId");

            migrationBuilder.CreateIndex(
                name: "IX_FellowshipMembers_FellowshipId",
                table: "FellowshipMembers",
                column: "FellowshipId");

            migrationBuilder.CreateIndex(
                name: "IX_FellowshipMembers_PlayerId",
                table: "FellowshipMembers",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Fellowships_OwnerId",
                table: "Fellowships",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_Fellowships_ReferralCode",
                table: "Fellowships",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Email",
                table: "Players",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_ReferralCode",
                table: "Players",
                column: "ReferralCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Players_Username",
                table: "Players",
                column: "Username",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_BroomBets_BroomLeagues_LeagueId",
                table: "BroomBets",
                column: "LeagueId",
                principalTable: "BroomLeagues",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BroomBets_BroomTeams_TeamId",
                table: "BroomBets",
                column: "TeamId",
                principalTable: "BroomTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BroomLeagues_BroomTeams_WinnerTeamId",
                table: "BroomLeagues",
                column: "WinnerTeamId",
                principalTable: "BroomTeams",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BroomTeams_BroomLeagues_LeagueId",
                table: "BroomTeams");

            migrationBuilder.DropTable(
                name: "BankAccounts");

            migrationBuilder.DropTable(
                name: "BankItems");

            migrationBuilder.DropTable(
                name: "BattleTurns");

            migrationBuilder.DropTable(
                name: "BroomBets");

            migrationBuilder.DropTable(
                name: "FellowshipMembers");

            migrationBuilder.DropTable(
                name: "Items");

            migrationBuilder.DropTable(
                name: "Battles");

            migrationBuilder.DropTable(
                name: "Spells");

            migrationBuilder.DropTable(
                name: "Fellowships");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "BroomLeagues");

            migrationBuilder.DropTable(
                name: "BroomTeams");
        }
    }
}
