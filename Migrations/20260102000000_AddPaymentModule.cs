using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── 1. Plans ──────────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Price = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PlanType = table.Column<int>(type: "int", nullable: false),
                    DurationDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // ── 2. CashRegisters ──────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CashRegisters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Date = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ClosingBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    OpenedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClosedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    OpenedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClosedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Notes = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashRegisters", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CashRegisters_Date",
                table: "CashRegisters",
                column: "Date",
                unique: true);

            // ── 3. Add FK columns to Payments (no FK constraints yet) ─────────────
            migrationBuilder.AddColumn<int>(
                name: "PlanId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CashRegisterId",
                table: "Payments",
                type: "int",
                nullable: true);

            // ── 4. MemberPlans ────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "MemberPlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MemberId = table.Column<int>(type: "int", nullable: false),
                    PlanId = table.Column<int>(type: "int", nullable: false),
                    PaymentId = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MemberPlans_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberPlans_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPlans_MemberId",
                table: "MemberPlans",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPlans_PlanId",
                table: "MemberPlans",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberPlans_PaymentId",
                table: "MemberPlans",
                column: "PaymentId");

            // ── 5. Add MemberPlanId to Payments ───────────────────────────────────
            migrationBuilder.AddColumn<int>(
                name: "MemberPlanId",
                table: "Payments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PlanId",
                table: "Payments",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CashRegisterId",
                table: "Payments",
                column: "CashRegisterId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MemberPlanId",
                table: "Payments",
                column: "MemberPlanId");

            // ── 6. Foreign keys on Payments ───────────────────────────────────────
            migrationBuilder.AddForeignKey(
                name: "FK_Payments_Plans_PlanId",
                table: "Payments",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_CashRegisters_CashRegisterId",
                table: "Payments",
                column: "CashRegisterId",
                principalTable: "CashRegisters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Payments_MemberPlans_MemberPlanId",
                table: "Payments",
                column: "MemberPlanId",
                principalTable: "MemberPlans",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── 7. FK on MemberPlans.PaymentId (circular, nullable) ───────────────
            migrationBuilder.AddForeignKey(
                name: "FK_MemberPlans_Payments_PaymentId",
                table: "MemberPlans",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            // ── 8. CashTransactions ───────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "CashTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CashRegisterId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Method = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashTransactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashTransactions_CashRegisters_CashRegisterId",
                        column: x => x.CashRegisterId,
                        principalTable: "CashRegisters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CashTransactions_CashRegisterId",
                table: "CashTransactions",
                column: "CashRegisterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop FK constraints first
            migrationBuilder.DropForeignKey("FK_MemberPlans_Payments_PaymentId", "MemberPlans");
            migrationBuilder.DropForeignKey("FK_Payments_MemberPlans_MemberPlanId", "Payments");
            migrationBuilder.DropForeignKey("FK_Payments_Plans_PlanId", "Payments");
            migrationBuilder.DropForeignKey("FK_Payments_CashRegisters_CashRegisterId", "Payments");

            migrationBuilder.DropTable(name: "CashTransactions");

            migrationBuilder.DropIndex("IX_Payments_MemberPlanId", "Payments");
            migrationBuilder.DropIndex("IX_Payments_PlanId", "Payments");
            migrationBuilder.DropIndex("IX_Payments_CashRegisterId", "Payments");

            migrationBuilder.DropColumn("MemberPlanId", "Payments");
            migrationBuilder.DropColumn("PlanId", "Payments");
            migrationBuilder.DropColumn("CashRegisterId", "Payments");

            migrationBuilder.DropTable(name: "MemberPlans");
            migrationBuilder.DropTable(name: "CashRegisters");
            migrationBuilder.DropTable(name: "Plans");
        }
    }
}
