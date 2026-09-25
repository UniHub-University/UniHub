using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarInfoVendedor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CardapioUrl",
                table: "Vendedores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescricaoNegocio",
                table: "Vendedores",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "Vendedores",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CardapioUrl",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "DescricaoNegocio",
                table: "Vendedores");

            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "Vendedores");
        }
    }
}
