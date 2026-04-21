using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    /// <inheritdoc />
    public partial class AddCashRegisterReopen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WasReopened",
                table: "CashRegisters",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReopenedBy",
                table: "CashRegisters",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReopenedAt",
                table: "CashRegisters",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReopenNotes",
                table: "CashRegisters",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "WasReopened",  table: "CashRegisters");
            migrationBuilder.DropColumn(name: "ReopenedBy",   table: "CashRegisters");
            migrationBuilder.DropColumn(name: "ReopenedAt",   table: "CashRegisters");
            migrationBuilder.DropColumn(name: "ReopenNotes",  table: "CashRegisters");
        }
    }
}
