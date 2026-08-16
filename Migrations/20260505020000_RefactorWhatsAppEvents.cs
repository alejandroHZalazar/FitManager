using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    /// <inheritdoc />
    public partial class RefactorWhatsAppEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TriggerType",
                table: "WhatsAppEvents");

            migrationBuilder.DropColumn(
                name: "DaysBeforeDue",
                table: "WhatsAppEvents");

            migrationBuilder.AddColumn<int>(
                name: "SystemEvent",
                table: "WhatsAppEvents",
                type: "int",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SystemEvent",
                table: "WhatsAppEvents");

            migrationBuilder.AddColumn<int>(
                name: "TriggerType",
                table: "WhatsAppEvents",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "DaysBeforeDue",
                table: "WhatsAppEvents",
                type: "int",
                nullable: true);
        }
    }
}
