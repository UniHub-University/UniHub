using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    public VendedoresController(VendedorPublicoService vendedorPublicoService)
    {
        _vendedorPublicoService = vendedorPublicoService;
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
}
