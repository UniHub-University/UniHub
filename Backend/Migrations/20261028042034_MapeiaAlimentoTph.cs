using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class MapeiaAlimentoTph : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Alimento_Categoria",
                table: "Produtos",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HorarioDisponibilidade",
                table: "Produtos",
                type: "interval",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Restricoes",
                table: "Produtos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Alimento_Categoria",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "HorarioDisponibilidade",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Restricoes",
                table: "Produtos");
        }
    }
}
