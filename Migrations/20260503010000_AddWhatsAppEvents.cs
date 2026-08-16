using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    public partial class AddWhatsAppEvents : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Quitar el campo que agregó AddWhatsAppSettings (siempre presente porque esa migración ya corrió)
            migrationBuilder.DropColumn(
                name: "WhatsAppEventName",
                table: "CompanySettings");

            migrationBuilder.CreateTable(
                name: "WhatsAppEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CompanySettingsId = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Label       = table.Column<string>(type: "varchar(100)",  maxLength: 100,  nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                    EventName   = table.Column<string>(type: "varchar(150)",  maxLength: 150,  nullable: false).Annotation("MySql:CharSet", "utf8mb4"),
                    TriggerType = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    DaysBeforeDue = table.Column<int>(type: "int", nullable: true),
                    IsActive    = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt   = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WhatsAppEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WhatsAppEvents_CompanySettings_CompanySettingsId",
                        column: x => x.CompanySettingsId,
                        principalTable: "CompanySettings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_WhatsAppEvents_CompanySettingsId",
                table: "WhatsAppEvents",
                column: "CompanySettingsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "WhatsAppEvents");

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppEventName",
                table: "CompanySettings",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
