using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    // POST /api/perfil/voluntario
    // Torna o usuario autenticado um voluntario. O usuario vem SEMPRE do token
    // (claim "sub"), nunca da requisicao. Espelha VendedorController.TornarSeVendedor.
    [HttpPost]
    public async Task<IActionResult> TornarSeVoluntario()
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _voluntarioService.TornarSeVoluntarioAsync(usuarioId);

        if (!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(new { mensagem = resultado.Mensagem, voluntarioId = resultado.VoluntarioId });
    }

    /// DELETE /api/perfil/voluntario
    /// Remove o perfil de voluntario. Bloqueado se houver apoio em andamento.
    [HttpDelete]
    public async Task<IActionResult> Desistir()
    {
        var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
        var resultado = await _voluntarioService.DesistirAsync(usuarioId);

        if (!resultado.Sucesso)
            return BadRequest(new { mensagem = resultado.Mensagem });

        return Ok(new { mensagem = resultado.Mensagem });
    }

}
