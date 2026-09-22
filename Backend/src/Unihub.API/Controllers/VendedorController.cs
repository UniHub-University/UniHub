using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using UniHub.Application.DTOs;
using UniHub.Application.Services;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/perfil/vendedor")]
[Authorize]
public class VendedorController : ControllerBase
{
    private readonly VendedorService _vendedorService;

    public VendedorController(VendedorService vendedorService)
    {
        _vendedorService = vendedorService;
    }

    [HttpPut("logistica")]
    public async Task<IActionResult> AtualizarLogistica([FromBody] AtualizarLogisticaDTO dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _vendedorService.AtualizarLogisticaAsync(usuarioId, dto);

        if(!resultado.Sucesso)
            return BadRequest(new { mesagem = resultado.Mensagem });
        
        return Ok( new { mensagem = resultado.Mensagem });

    }


    /* PUT /api/perfil/vendedor/info
    Recebe URLs ja geradas via POST /api/upload/imagem (pasta=cardapios
    ou pasta=perfis) e persiste no perfil do vendedor. Nao faz upload diretamente -- ver AtualizarInfoVendedorDto para o motivo dessa
    separacao de responsabilidade. */

    [HttpPut("info")]
    public async Task<IActionResult> AtualizarInfo([FromBody] AtualizarInfoVendedorDto dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _vendedorService.AtualizarInfoAsync(usuarioId, dto);

        if(!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(new { mensagem = resultado.Mensagem });
    }
}