using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidacaoPerfisSprint2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HorariosVenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiaSemana = table.Column<int>(type: "integer", nullable: false),
                    HoraInicio = table.Column<TimeSpan>(type: "interval", nullable: false),
                    HoraFim = table.Column<TimeSpan>(type: "interval", nullable: false),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosVenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosVenda_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocaisVenda",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Campus = table.Column<string>(type: "text", nullable: false),
                    PontoDeEncontro = table.Column<string>(type: "text", nullable: false),
                    VendedorId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocaisVenda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocaisVenda_Vendedores_VendedorId",
                        column: x => x.VendedorId,
                        principalTable: "Vendedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RestricoesAlimentares",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestricoesAlimentares", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsuarioRestricaoAlimentar",
                columns: table => new
                {
                    RestricoesAlimentaresId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuariosId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioRestricaoAlimentar", x => new { x.RestricoesAlimentaresId, x.UsuariosId });
                    table.ForeignKey(
                        name: "FK_UsuarioRestricaoAlimentar_RestricoesAlimentares_RestricoesA~",
                        column: x => x.RestricoesAlimentaresId,
                        principalTable: "RestricoesAlimentares",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioRestricaoAlimentar_Usuarios_UsuariosId",
                        column: x => x.UsuariosId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HorariosVenda_VendedorId",
                table: "HorariosVenda",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_LocaisVenda_VendedorId",
                table: "LocaisVenda",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioRestricaoAlimentar_UsuariosId",
                table: "UsuarioRestricaoAlimentar",
                column: "UsuariosId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HorariosVenda");

            migrationBuilder.DropTable(
                name: "LocaisVenda");

            migrationBuilder.DropTable(
                name: "UsuarioRestricaoAlimentar");

            migrationBuilder.DropTable(
                name: "RestricoesAlimentares");
        }
    }
}
