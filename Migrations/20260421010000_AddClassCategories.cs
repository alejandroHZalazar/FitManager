using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FitManager.Migrations
{
    public partial class AddClassCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1 — Crear tabla ClassCategories
            migrationBuilder.CreateTable(
                name: "ClassCategories",
                columns: table => new
                {
                    Id          = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name        = table.Column<string>(type: "varchar(100)", maxLength: 100,  nullable: false),
                    Description = table.Column<string>(type: "varchar(300)", maxLength: 300,  nullable: true),
                    Color       = table.Column<string>(type: "varchar(7)",   maxLength: 7,    nullable: false, defaultValue: "#6c757d"),
                    Icon        = table.Column<string>(type: "varchar(60)",  maxLength: 60,   nullable: false, defaultValue: "fa-dumbbell"),
                    OrderIndex  = table.Column<int>(type: "int", nullable: false, defaultValue: 99),
                    IsActive    = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    CreatedAt   = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassCategories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_ClassCategories_Name",
                table: "ClassCategories",
                column: "Name",
                unique: true);

            // 2 — Insertar categorías por defecto
            migrationBuilder.InsertData(
                table: "ClassCategories",
                columns: new[] { "Name", "Description", "Color", "Icon", "OrderIndex", "IsActive", "CreatedAt" },
                values: new object[,]
                {
                    { "Spinning",    "Ciclismo estacionario de alta intensidad",  "#e74c3c", "fa-bicycle",        1, true, DateTime.UtcNow },
                    { "Yoga",        "Práctica de posturas y respiración",         "#9b59b6", "fa-spa",            2, true, DateTime.UtcNow },
                    { "Pilates",     "Ejercicios de fuerza y flexibilidad",        "#3498db", "fa-female",         3, true, DateTime.UtcNow },
                    { "Zumba",       "Danza aeróbica con ritmos latinos",           "#e67e22", "fa-music",          4, true, DateTime.UtcNow },
                    { "CrossFit",    "Entrenamiento funcional de alta intensidad", "#e74c3c", "fa-fire",           5, true, DateTime.UtcNow },
                    { "Funcional",   "Ejercicios funcionales y de resistencia",    "#2ecc71", "fa-running",        6, true, DateTime.UtcNow },
                    { "Kickboxing",  "Boxeo y artes marciales aplicadas",          "#c0392b", "fa-fist-raised",    7, true, DateTime.UtcNow },
                    { "Natación",    "Disciplinas acuáticas",                      "#00b4d8", "fa-swimmer",        8, true, DateTime.UtcNow },
                    { "Musculación", "Entrenamiento con pesas y máquinas",         "#f39c12", "fa-dumbbell",       9, true, DateTime.UtcNow },
                    { "Otro",        "Otra actividad",                             "#6c757d", "fa-star",          99, true, DateTime.UtcNow }
                });

            // 3 — Agregar columna ClassCategoryId a FitnessClasses
            migrationBuilder.AddColumn<int>(
                name: "ClassCategoryId",
                table: "FitnessClasses",
                type: "int",
                nullable: true);

            // 4 — Eliminar columna vieja Category (enum int)
            migrationBuilder.DropColumn(
                name: "Category",
                table: "FitnessClasses");

            // 5 — Agregar FK
            migrationBuilder.AddForeignKey(
                name: "FK_FitnessClasses_ClassCategories_ClassCategoryId",
                table: "FitnessClasses",
                column: "ClassCategoryId",
                principalTable: "ClassCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_FitnessClasses_ClassCategoryId",
                table: "FitnessClasses",
                column: "ClassCategoryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey("FK_FitnessClasses_ClassCategories_ClassCategoryId", "FitnessClasses");
            migrationBuilder.DropIndex("IX_FitnessClasses_ClassCategoryId", "FitnessClasses");
            migrationBuilder.DropColumn("ClassCategoryId", "FitnessClasses");
            migrationBuilder.AddColumn<int>("Category", "FitnessClasses", type: "int", nullable: false, defaultValue: 99);
            migrationBuilder.DropTable("ClassCategories");
        }
    }
}
