using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    public partial class AddCompanySettings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompanySettings",
                columns: table => new
                {
                    Id        = table.Column<int>(nullable: false),
                    Name      = table.Column<string>(maxLength: 150, nullable: false),
                    Slogan    = table.Column<string>(maxLength: 200, nullable: true),
                    LogoPath  = table.Column<string>(maxLength: 300, nullable: true),
                    Address   = table.Column<string>(maxLength: 250, nullable: true),
                    City      = table.Column<string>(maxLength: 100, nullable: true),
                    Province  = table.Column<string>(maxLength: 100, nullable: true),
                    Country   = table.Column<string>(maxLength: 100, nullable: true),
                    TaxId     = table.Column<string>(maxLength: 30,  nullable: true),
                    Phone     = table.Column<string>(maxLength: 30,  nullable: true),
                    Phone2    = table.Column<string>(maxLength: 30,  nullable: true),
                    Email     = table.Column<string>(maxLength: 150, nullable: true),
                    Website   = table.Column<string>(maxLength: 150, nullable: true),
                    Notes     = table.Column<string>(maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanySettings", x => x.Id);
                });

            // Default singleton record
            migrationBuilder.InsertData(
                table: "CompanySettings",
                columns: new[] { "Id", "Name", "UpdatedAt" },
                values: new object[] { 1, "FitManager", DateTime.UtcNow });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CompanySettings");
        }
    }
}
