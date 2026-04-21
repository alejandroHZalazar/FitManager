using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    public partial class AddClassesModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FitnessClasses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name           = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    Description    = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    Category       = table.Column<int>(type: "int", nullable: false),
                    InstructorName = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    Location       = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    Color          = table.Column<string>(type: "varchar(7)",   maxLength: 7,   nullable: false, defaultValue: "#ff6b35"),
                    StartDate      = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndDate        = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MaxCapacity    = table.Column<int>(type: "int", nullable: true),
                    IsActive       = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt      = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedBy      = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FitnessClasses", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassSchedules",
                columns: table => new
                {
                    Id             = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FitnessClassId = table.Column<int>(type: "int", nullable: false),
                    ScheduleType   = table.Column<int>(type: "int", nullable: false),
                    DaysOfWeek     = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DayOfMonth     = table.Column<int>(type: "int", nullable: true),
                    SpecificDate   = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    StartTime      = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    EndTime        = table.Column<TimeSpan>(type: "time(6)", nullable: false),
                    EffectiveFrom  = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EffectiveTo    = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsActive       = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    Notes          = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSchedules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassSchedules_FitnessClasses_FitnessClassId",
                        column: x => x.FitnessClassId,
                        principalTable: "FitnessClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ClassEnrollments",
                columns: table => new
                {
                    Id             = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FitnessClassId = table.Column<int>(type: "int", nullable: false),
                    MemberId       = table.Column<int>(type: "int", nullable: false),
                    EnrolledAt     = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Status         = table.Column<int>(type: "int", nullable: false),
                    Notes          = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    EnrolledBy     = table.Column<string>(type: "varchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassEnrollments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_FitnessClasses_FitnessClassId",
                        column: x => x.FitnessClassId,
                        principalTable: "FitnessClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassEnrollments_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_FitnessClassId",
                table: "ClassSchedules",
                column: "FitnessClassId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_FitnessClassId_MemberId",
                table: "ClassEnrollments",
                columns: new[] { "FitnessClassId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_ClassEnrollments_MemberId",
                table: "ClassEnrollments",
                column: "MemberId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ClassEnrollments");
            migrationBuilder.DropTable(name: "ClassSchedules");
            migrationBuilder.DropTable(name: "FitnessClasses");
        }
    }
}
