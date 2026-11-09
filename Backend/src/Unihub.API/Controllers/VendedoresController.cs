using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Produtos;
using UniHub.Application.DTOs.Vendas;
using UniHub.Application.Services.Vendas;

namespace UniHub.API.Controllers;

// Visao PUBLICA de vendedores (o VendedorController, no singular, cuida das
// rotas do proprio vendedor). Exige token, como o restante da API.
[ApiController]
[Route("api/vendedores")]
[Authorize]
public class VendedoresController : ControllerBase
{
    private readonly VendedorPublicoService _vendedorPublicoService;
    private readonly ProdutoService _produtoService;

    public VendedoresController(VendedorPublicoService vendedorPublicoService, ProdutoService produtoService)
    {
        _vendedorPublicoService = vendedorPublicoService;
        _produtoService = produtoService;
    }

    // GET api/vendedores?page=1&pageSize=12
    // Lista publica e paginada de vendedores que tem anuncio disponivel, do mais
    // bem avaliado para o menos. O Service ajusta page/pageSize (padrao 12, maximo 50).
    // Nao expoe telefone, e-mail nem chave PIX.
    [HttpGet]
    public async Task<ActionResult<PaginatedResultDto<VendedorResumoDto>>> Listar(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12)
    {
        var resultado = await _vendedorPublicoService.ListarAsync(page, pageSize);
        return Ok(resultado);
    }

    // GET api/vendedores/{id}
    // Perfil publico: nome, foto, descricao, nota, locais e horarios de venda.
    // Nao expoe telefone, e-mail nem chave PIX. 200 | 404 vendedor inexistente.
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<VendedorPublicoDto>> ObterPerfil(Guid id)
    {
        var perfil = await _vendedorPublicoService.ObterPerfilAsync(id);

        if (perfil is null)
            return NotFound(new { mensagem = "Vendedor nao encontrado." });

        return Ok(perfil);
    }

    // GET api/vendedores/{id}/produtos?termo=&tipo=bazar&precoMin=&precoMax=&page=1&pageSize=12
    // Anuncios disponiveis de UM vendedor, com os mesmos filtros e a mesma
    // paginacao de GET /api/produtos. 200 | 400 filtro invalido | 404 vendedor inexistente.
    [HttpGet("{id:guid}/produtos")]
    public async Task<ActionResult<PaginatedResultDto<ProdutoResumoDto>>> ListarProdutos(
        Guid id, [FromQuery] BuscaProdutosQueryDto query)
    {
        if (await _vendedorPublicoService.ObterPerfilAsync(id) is null)
            return NotFound(new { mensagem = "Vendedor nao encontrado." });

        try
        {
            var resultado = await _produtoService.BuscarAsync(query, id);
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}
