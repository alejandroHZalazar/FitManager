using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    public partial class AddWhatsAppSettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppEnabled",
                table: "CompanySettings",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppApiKey",
                table: "CompanySettings",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppApiUrl",
                table: "CompanySettings",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "WhatsAppEventName",
                table: "CompanySettings",
                type: "varchar(150)",
                maxLength: 150,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "WhatsAppEnabled",   table: "CompanySettings");
            migrationBuilder.DropColumn(name: "WhatsAppApiKey",    table: "CompanySettings");
            migrationBuilder.DropColumn(name: "WhatsAppApiUrl",    table: "CompanySettings");
            migrationBuilder.DropColumn(name: "WhatsAppEventName", table: "CompanySettings");
        }
    }
}
