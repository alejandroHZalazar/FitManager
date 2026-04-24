using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    public partial class AddRoutinesModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Exercises ─────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Exercises",
                columns: table => new
                {
                    Id           = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name         = table.Column<string>(maxLength: 120, nullable: false),
                    Description  = table.Column<string>(maxLength: 500, nullable: true),
                    MuscleGroup  = table.Column<int>(nullable: false),
                    ExerciseType = table.Column<int>(nullable: false),
                    IsCustom     = table.Column<bool>(nullable: false, defaultValue: false),
                    IsActive     = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt    = table.Column<DateTime>(nullable: false)
                },
                constraints: table => table.PrimaryKey("PK_Exercises", x => x.Id));

            // ── Routines ──────────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "Routines",
                columns: table => new
                {
                    Id               = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name             = table.Column<string>(maxLength: 150, nullable: false),
                    Description      = table.Column<string>(maxLength: 500, nullable: true),
                    Goal             = table.Column<int>(nullable: false),
                    DurationWeeks    = table.Column<int>(nullable: false, defaultValue: 4),
                    FrequencyPerWeek = table.Column<int>(nullable: false, defaultValue: 3),
                    IsGeneral        = table.Column<bool>(nullable: false, defaultValue: true),
                    IsActive         = table.Column<bool>(nullable: false, defaultValue: true),
                    CreatedAt        = table.Column<DateTime>(nullable: false),
                    CreatedBy        = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table => table.PrimaryKey("PK_Routines", x => x.Id));

            // ── RoutineDays ───────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "RoutineDays",
                columns: table => new
                {
                    Id         = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoutineId  = table.Column<int>(nullable: false),
                    DayNumber  = table.Column<int>(nullable: false),
                    Name       = table.Column<string>(maxLength: 150, nullable: true),
                    IsRestDay  = table.Column<bool>(nullable: false, defaultValue: false),
                    OrderIndex = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutineDays", x => x.Id);
                    table.ForeignKey("FK_RoutineDays_Routines_RoutineId", x => x.RoutineId, "Routines", "Id", onDelete: ReferentialAction.Cascade);
                });

            // ── RoutineExercises ──────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "RoutineExercises",
                columns: table => new
                {
                    Id           = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoutineDayId = table.Column<int>(nullable: false),
                    ExerciseId   = table.Column<int>(nullable: false),
                    Sets         = table.Column<int>(nullable: false, defaultValue: 3),
                    Reps         = table.Column<string>(maxLength: 30, nullable: false, defaultValue: "10"),
                    Weight       = table.Column<decimal>(type: "decimal(8,2)", nullable: true),
                    RestSeconds  = table.Column<int>(nullable: true),
                    Notes        = table.Column<string>(maxLength: 300, nullable: true),
                    OrderIndex   = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoutineExercises", x => x.Id);
                    table.ForeignKey("FK_RoutineExercises_RoutineDays_RoutineDayId", x => x.RoutineDayId, "RoutineDays", "Id", onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_RoutineExercises_Exercises_ExerciseId", x => x.ExerciseId, "Exercises", "Id", onDelete: ReferentialAction.Restrict);
                });

            // ── MemberRoutines ────────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "MemberRoutines",
                columns: table => new
                {
                    Id         = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    MemberId   = table.Column<int>(nullable: false),
                    RoutineId  = table.Column<int>(nullable: false),
                    AssignedAt = table.Column<DateTime>(nullable: false),
                    StartDate  = table.Column<DateTime>(nullable: false),
                    EndDate    = table.Column<DateTime>(nullable: true),
                    IsActive   = table.Column<bool>(nullable: false, defaultValue: true),
                    Notes      = table.Column<string>(maxLength: 500, nullable: true),
                    AssignedBy = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberRoutines", x => x.Id);
                    table.ForeignKey("FK_MemberRoutines_Members_MemberId", x => x.MemberId, "Members", "Id", onDelete: ReferentialAction.Restrict);
                    table.ForeignKey("FK_MemberRoutines_Routines_RoutineId", x => x.RoutineId, "Routines", "Id", onDelete: ReferentialAction.Restrict);
                });

            // ── Indexes ───────────────────────────────────────────────────────
            migrationBuilder.CreateIndex("IX_RoutineDays_RoutineId",        "RoutineDays",      "RoutineId");
            migrationBuilder.CreateIndex("IX_RoutineExercises_RoutineDayId", "RoutineExercises", "RoutineDayId");
            migrationBuilder.CreateIndex("IX_RoutineExercises_ExerciseId",   "RoutineExercises", "ExerciseId");
            migrationBuilder.CreateIndex("IX_MemberRoutines_MemberId_IsActive", "MemberRoutines", new[] { "MemberId", "IsActive" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable("MemberRoutines");
            migrationBuilder.DropTable("RoutineExercises");
            migrationBuilder.DropTable("RoutineDays");
            migrationBuilder.DropTable("Routines");
            migrationBuilder.DropTable("Exercises");
        }
    }
}
