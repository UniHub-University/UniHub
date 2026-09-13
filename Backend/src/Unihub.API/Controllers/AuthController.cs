using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System; 
using UniHub.Application.DTOs.Seguranca; 
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
            // O AuthService agora devolve um LoginResult pronto (Token, Id,
            // NomeCompleto, EmailInstitucional, FotoPerfilUrl), em vez de
            // só o Usuario cru
            var resultadoLogin = await _authService.ValidarLoginGoogleAsync(request.IdToken);

            // Devolve o LoginResult direto: o ASP.NET Core serializa esse
            // objeto pra JSON automaticamente, incluindo o Token que o
            // frontend vai guardar e reenviar nas próximas requisições
            return Ok(resultadoLogin);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { Erro = ex.Message });  // 401 Unauthorized
        }
        catch (Exception ex)
        {
            return BadRequest(new { Erro = ex.Message });  // 400 Bad Request
        }
    }
}