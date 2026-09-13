using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using UniHub.Application.DTOs;
using UniHub.Application.Services;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    // Em vez de criar o AuthService manualmente com "new" (o que exigiria
    // passar AppDbContext e IConfiguration na mão, coisas que o Controller
    // nem deveria conhecer), o ASP.NET entrega um AuthService já pronto
    // aqui, com tudo que ele precisa por dentro. Isso é injeção de dependência:
    // o Controller só "pede" o serviço, quem monta ele é o framework.
    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("google-login")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginDTO request)
    {
        try
        {
            var usuarioValidado = await _authService.ValidarLoginGoogleAsync(request.IdToken);
            return Ok(new { Mensagem = "Login autorizado!", Usuario = usuarioValidado.NomeCompleto });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { Erro = ex.Message });
        }
    }
}