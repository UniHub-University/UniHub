using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AjusteModelosIntegracao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolicitacaoApoio",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolicitanteId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoPublico = table.Column<string>(type: "text", nullable: false),
                    Categoria = table.Column<string>(type: "text", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitacaoApoio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitacaoApoio_Usuarios_SolicitanteId",
                        column: x => x.SolicitanteId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Voluntario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CodigoPublico = table.Column<string>(type: "text", nullable: false),
                    AreasDeAtuacao = table.Column<List<string>>(type: "text[]", nullable: false),
                    Disponivel = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Voluntario", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Voluntario_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Correspondencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolicitacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoluntarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    IdentidadeRevelada = table.Column<bool>(type: "boolean", nullable: false),
                    IdentidadeReveladaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AceitaEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correspondencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Correspondencia_SolicitacaoApoio_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "SolicitacaoApoio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Correspondencia_Voluntario_VoluntarioId",
                        column: x => x.VoluntarioId,
                        principalTable: "Voluntario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Doacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SolicitacaoId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoluntarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    Descricao = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EntregueEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Doacao_SolicitacaoApoio_SolicitacaoId",
                        column: x => x.SolicitacaoId,
                        principalTable: "SolicitacaoApoio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Doacao_Voluntario_VoluntarioId",
                        column: x => x.VoluntarioId,
                        principalTable: "Voluntario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Correspondencia_SolicitacaoId",
                table: "Correspondencia",
                column: "SolicitacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Correspondencia_VoluntarioId",
                table: "Correspondencia",
                column: "VoluntarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Doacao_SolicitacaoId",
                table: "Doacao",
                column: "SolicitacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Doacao_VoluntarioId",
                table: "Doacao",
                column: "VoluntarioId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitacaoApoio_SolicitanteId",
                table: "SolicitacaoApoio",
                column: "SolicitanteId");

            migrationBuilder.CreateIndex(
                name: "IX_Voluntario_UsuarioId",
                table: "Voluntario",
                column: "UsuarioId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Correspondencia");

            migrationBuilder.DropTable(
                name: "Doacao");

            migrationBuilder.DropTable(
                name: "SolicitacaoApoio");

            migrationBuilder.DropTable(
                name: "Voluntario");
        }
    }
}
