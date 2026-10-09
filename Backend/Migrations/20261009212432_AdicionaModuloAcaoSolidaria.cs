using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Backend.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaModuloAcaoSolidaria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Correspondencia_SolicitacaoApoio_SolicitacaoId",
                table: "Correspondencia");

            migrationBuilder.DropForeignKey(
                name: "FK_Correspondencia_Voluntario_VoluntarioId",
                table: "Correspondencia");

            migrationBuilder.DropForeignKey(
                name: "FK_Doacao_SolicitacaoApoio_SolicitacaoId",
                table: "Doacao");

            migrationBuilder.DropForeignKey(
                name: "FK_Doacao_Voluntario_VoluntarioId",
                table: "Doacao");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitacaoApoio_Usuarios_SolicitanteId",
                table: "SolicitacaoApoio");

            migrationBuilder.DropForeignKey(
                name: "FK_Voluntario_Usuarios_UsuarioId",
                table: "Voluntario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Voluntario",
                table: "Voluntario");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SolicitacaoApoio",
                table: "SolicitacaoApoio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Doacao",
                table: "Doacao");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Correspondencia",
                table: "Correspondencia");

            migrationBuilder.RenameTable(
                name: "Voluntario",
                newName: "Voluntarios");

            migrationBuilder.RenameTable(
                name: "SolicitacaoApoio",
                newName: "SolicitacoesApoio");

            migrationBuilder.RenameTable(
                name: "Doacao",
                newName: "Doacoes");

            migrationBuilder.RenameTable(
                name: "Correspondencia",
                newName: "Correspondencias");

            migrationBuilder.RenameIndex(
                name: "IX_Voluntario_UsuarioId",
                table: "Voluntarios",
                newName: "IX_Voluntarios_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_SolicitacaoApoio_SolicitanteId",
                table: "SolicitacoesApoio",
                newName: "IX_SolicitacoesApoio_SolicitanteId");

            migrationBuilder.RenameIndex(
                name: "IX_Doacao_VoluntarioId",
                table: "Doacoes",
                newName: "IX_Doacoes_VoluntarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Doacao_SolicitacaoId",
                table: "Doacoes",
                newName: "IX_Doacoes_SolicitacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Correspondencia_VoluntarioId",
                table: "Correspondencias",
                newName: "IX_Correspondencias_VoluntarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Correspondencia_SolicitacaoId",
                table: "Correspondencias",
                newName: "IX_Correspondencias_SolicitacaoId");

            migrationBuilder.AlterColumn<int[]>(
                name: "AreasDeAtuacao",
                table: "Voluntarios",
                type: "integer[]",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "text[]");

            migrationBuilder.AlterColumn<int>(
                name: "Categoria",
                table: "SolicitacoesApoio",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Voluntarios",
                table: "Voluntarios",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SolicitacoesApoio",
                table: "SolicitacoesApoio",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Doacoes",
                table: "Doacoes",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Correspondencias",
                table: "Correspondencias",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Correspondencias_SolicitacoesApoio_SolicitacaoId",
                table: "Correspondencias",
                column: "SolicitacaoId",
                principalTable: "SolicitacoesApoio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Correspondencias_Voluntarios_VoluntarioId",
                table: "Correspondencias",
                column: "VoluntarioId",
                principalTable: "Voluntarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Doacoes_SolicitacoesApoio_SolicitacaoId",
                table: "Doacoes",
                column: "SolicitacaoId",
                principalTable: "SolicitacoesApoio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Doacoes_Voluntarios_VoluntarioId",
                table: "Doacoes",
                column: "VoluntarioId",
                principalTable: "Voluntarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitacoesApoio_Usuarios_SolicitanteId",
                table: "SolicitacoesApoio",
                column: "SolicitanteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Voluntarios_Usuarios_UsuarioId",
                table: "Voluntarios",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Correspondencias_SolicitacoesApoio_SolicitacaoId",
                table: "Correspondencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Correspondencias_Voluntarios_VoluntarioId",
                table: "Correspondencias");

            migrationBuilder.DropForeignKey(
                name: "FK_Doacoes_SolicitacoesApoio_SolicitacaoId",
                table: "Doacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_Doacoes_Voluntarios_VoluntarioId",
                table: "Doacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_SolicitacoesApoio_Usuarios_SolicitanteId",
                table: "SolicitacoesApoio");

            migrationBuilder.DropForeignKey(
                name: "FK_Voluntarios_Usuarios_UsuarioId",
                table: "Voluntarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Voluntarios",
                table: "Voluntarios");

            migrationBuilder.DropPrimaryKey(
                name: "PK_SolicitacoesApoio",
                table: "SolicitacoesApoio");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Doacoes",
                table: "Doacoes");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Correspondencias",
                table: "Correspondencias");

            migrationBuilder.RenameTable(
                name: "Voluntarios",
                newName: "Voluntario");

            migrationBuilder.RenameTable(
                name: "SolicitacoesApoio",
                newName: "SolicitacaoApoio");

            migrationBuilder.RenameTable(
                name: "Doacoes",
                newName: "Doacao");

            migrationBuilder.RenameTable(
                name: "Correspondencias",
                newName: "Correspondencia");

            migrationBuilder.RenameIndex(
                name: "IX_Voluntarios_UsuarioId",
                table: "Voluntario",
                newName: "IX_Voluntario_UsuarioId");

            migrationBuilder.RenameIndex(
                name: "IX_SolicitacoesApoio_SolicitanteId",
                table: "SolicitacaoApoio",
                newName: "IX_SolicitacaoApoio_SolicitanteId");

            migrationBuilder.RenameIndex(
                name: "IX_Doacoes_VoluntarioId",
                table: "Doacao",
                newName: "IX_Doacao_VoluntarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Doacoes_SolicitacaoId",
                table: "Doacao",
                newName: "IX_Doacao_SolicitacaoId");

            migrationBuilder.RenameIndex(
                name: "IX_Correspondencias_VoluntarioId",
                table: "Correspondencia",
                newName: "IX_Correspondencia_VoluntarioId");

            migrationBuilder.RenameIndex(
                name: "IX_Correspondencias_SolicitacaoId",
                table: "Correspondencia",
                newName: "IX_Correspondencia_SolicitacaoId");

            migrationBuilder.AlterColumn<List<string>>(
                name: "AreasDeAtuacao",
                table: "Voluntario",
                type: "text[]",
                nullable: false,
                oldClrType: typeof(int[]),
                oldType: "integer[]");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "SolicitacaoApoio",
                type: "text",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Voluntario",
                table: "Voluntario",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_SolicitacaoApoio",
                table: "SolicitacaoApoio",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Doacao",
                table: "Doacao",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Correspondencia",
                table: "Correspondencia",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Correspondencia_SolicitacaoApoio_SolicitacaoId",
                table: "Correspondencia",
                column: "SolicitacaoId",
                principalTable: "SolicitacaoApoio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Correspondencia_Voluntario_VoluntarioId",
                table: "Correspondencia",
                column: "VoluntarioId",
                principalTable: "Voluntario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doacao_SolicitacaoApoio_SolicitacaoId",
                table: "Doacao",
                column: "SolicitacaoId",
                principalTable: "SolicitacaoApoio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Doacao_Voluntario_VoluntarioId",
                table: "Doacao",
                column: "VoluntarioId",
                principalTable: "Voluntario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SolicitacaoApoio_Usuarios_SolicitanteId",
                table: "SolicitacaoApoio",
                column: "SolicitanteId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Voluntario_Usuarios_UsuarioId",
                table: "Voluntario",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
