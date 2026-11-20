using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Denuncias;
using UniHub.Application.Interfaces;

namespace UniHub.API.Controllers;

/// <summary>
/// Endpoints administrativos de consulta de denúncias.
/// Este controller contém o GET paginado; o PATCH pode ficar em outro
/// controller, desde que use a rota e a autorização combinadas pela equipe.
/// </summary>
[ApiController]
[Route("api/admin/denuncias")]
// A role é verificada no token JWT. A configuração atual usa RoleClaimType = "role".
[Authorize(Roles = "Admin")]
public class DenunciasAdminController : ControllerBase
{
    private readonly IDenunciaRepository _denunciaRepository;

    public DenunciasAdminController(IDenunciaRepository denunciaRepository)
    {
        _denunciaRepository = denunciaRepository;
    }

    /// <summary>
    /// Lista denúncias para o painel, com paginação e nomes resolvidos.
    /// Exemplo: GET /api/admin/denuncias?page=1&amp;pageSize=20
    /// </summary>
    /// <param name="page">Página solicitada; a primeira página é 1.</param>
    /// <param name="pageSize">Itens por página; aceitamos de 1 a 100.</param>
    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<AdminDenunciaDto>>> Listar(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        // Impede páginas inválidas e limites excessivos de itens por resposta.
        if (page < 1 || pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new
            {
                mensagem = "page deve ser maior que zero e pageSize deve estar entre 1 e 100."
            });
        }

        // O repositório retorna apenas a página solicitada e o total geral.
        var (denuncias, total) =
            await _denunciaRepository.ListarParaAdminAsync(page, pageSize);

        // Converte entidades em DTOs. Assim, a resposta não expõe entidades
        // completas nem propriedades de navegação desnecessárias.
        var items = denuncias.Select(d => new AdminDenunciaDto(
            d.Id,
            d.Denunciante?.NomeCompleto ?? "Usuário não identificado",
            d.Denunciado?.NomeCompleto ?? "Usuário não identificado",
            d.Produto?.Nome,
            d.Motivo,
            d.Descricao,
            d.Status,
            d.CriadaEm
        )).ToList();

        // O resultado comum do projeto contém a página, o tamanho, o total
        // e o número de páginas. Uma lista vazia resulta em zero páginas.
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        return Ok(new PaginatedResultDto<AdminDenunciaDto>(
            items,
            page,
            pageSize,
            total,
            totalPages
        ));
    }
}