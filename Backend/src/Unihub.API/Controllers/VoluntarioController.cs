using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs;
using UniHub.Application.Services;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/perfil/voluntario")]
[Authorize]
public class VoluntarioController : ControllerBase
{
    private readonly VoluntarioService _voluntarioService;

    public VoluntarioController(VoluntarioService voluntarioService)
    {
        _voluntarioService = voluntarioService;
    }

    /// GET /api/perfil/voluntario
    /// Devolve o perfil de voluntario do usuario autenticado (para o front
    /// preencher a tela de edicao das areas e o botao pausar/retomar).
    [HttpGet]
    public async Task<IActionResult> ObterMeuPerfil()
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _voluntarioService.ObterMeuPerfilAsync(usuarioId);

        if (!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(resultado.Voluntario);
    }

    /// POST /api/perfil/voluntario
    /// Cria o perfil de Voluntario para o usuario autenticado, se ele ainda
    /// nao tiver um. Pre-requisito para as rotas /areas, /pausar e /retomar.
    /// Independe de o usuario ja ser Vendedor.
    [HttpPost]
    public async Task<IActionResult> TornarSeVoluntario()
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _voluntarioService.TornarSeVoluntarioAsync(usuarioId);

        if (!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(new { mensagem = resultado.Mensagem, voluntario = resultado.Voluntario });
    }

    /// PUT /api/perfil/voluntario/areas
    /// Define ou edita as areas de atuacao (substitui a lista inteira).
    [HttpPut("areas")]
    public async Task<IActionResult> DefinirAreas([FromBody] DefinirAreasDto dto)
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _voluntarioService.DefinirAreasAsync(usuarioId, dto);

        if (!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(new { mensagem = resultado.Mensagem, voluntario = resultado.Voluntario });
    }

    /// PATCH /api/perfil/voluntario/pausar
    [HttpPatch("pausar")]
    public async Task<IActionResult> PausarAtuacao()
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _voluntarioService.PausarAtuacaoAsync(usuarioId);

        if (!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(new { mensagem = resultado.Mensagem, voluntario = resultado.Voluntario });
    }

    /// PATCH /api/perfil/voluntario/retomar
    [HttpPatch("retomar")]
    public async Task<IActionResult> RetomarAtuacao()
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _voluntarioService.RetomarAtuacaoAsync(usuarioId);

        if (!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(new { mensagem = resultado.Mensagem, voluntario = resultado.Voluntario });
    }
}
