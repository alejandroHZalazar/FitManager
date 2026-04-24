using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    public partial class AddNutritionModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── NutritionPlans ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "NutritionPlans",
                columns: table => new
                {
                    Id           = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name         = table.Column<string>(maxLength: 150, nullable: false),
                    Description  = table.Column<string>(maxLength: 500, nullable: true),
                    Goal         = table.Column<int>(nullable: false),
                    DurationDays = table.Column<int>(nullable: false, defaultValue: 30),
                    GeneralNotes = table.Column<string>(maxLength: 1000, nullable: true),
                    IsTemplate   = table.Column<bool>(nullable: false, defaultValue: true),
                    IsActive     = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt    = table.Column<DateTime>(nullable: false),
                    CreatedBy    = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_NutritionPlans", x => x.Id));

            // ── NutritionMeals ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "NutritionMeals",
                columns: table => new
                {
                    Id              = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NutritionPlanId = table.Column<int>(nullable: false),
                    MealType        = table.Column<int>(nullable: false),
                    CustomName      = table.Column<string>(maxLength: 100, nullable: true),
                    Content         = table.Column<string>(maxLength: 2000, nullable: true),
                    Quantities      = table.Column<string>(maxLength: 500, nullable: true),
                    Notes           = table.Column<string>(maxLength: 500, nullable: true),
                    OrderIndex      = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NutritionMeals", x => x.Id);
                    table.ForeignKey("FK_NutritionMeals_NutritionPlans_NutritionPlanId",
                        x => x.NutritionPlanId, "NutritionPlans", "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // ── MemberNutritionPlans ──────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "MemberNutritionPlans",
                columns: table => new
                {
                    Id              = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MemberId        = table.Column<int>(nullable: false),
                    NutritionPlanId = table.Column<int>(nullable: false),
                    AssignedAt      = table.Column<DateTime>(nullable: false),
                    StartDate       = table.Column<DateTime>(nullable: true),
                    EndDate         = table.Column<DateTime>(nullable: true),
                    IsActive        = table.Column<bool>(nullable: false, defaultValue: true),
                    Notes           = table.Column<string>(maxLength: 500, nullable: true),
                    AssignedBy      = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberNutritionPlans", x => x.Id);
                    table.ForeignKey("FK_MemberNutritionPlans_Members_MemberId",
                        x => x.MemberId, "Members", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_MemberNutritionPlans_NutritionPlans_NutritionPlanId",
                        x => x.NutritionPlanId, "NutritionPlans", "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // ── Indexes ───────────────────────────────────────────────────────
            migrationBuilder.CreateIndex("IX_NutritionMeals_NutritionPlanId",        "NutritionMeals",       "NutritionPlanId");
            migrationBuilder.CreateIndex("IX_MemberNutritionPlans_MemberId_IsActive", "MemberNutritionPlans", new[] { "MemberId", "IsActive" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("MemberNutritionPlans");
            migrationBuilder.DropTable("NutritionMeals");
            migrationBuilder.DropTable("NutritionPlans");
        }
    }
}
