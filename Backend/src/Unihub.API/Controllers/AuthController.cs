using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using UniHub.Application.DTOs;
using UniHub.Application.Services;

namespace UniHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    // Recebe o serviço via Injeção de Dependência (configurada no Program.cs)
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
            
            // Chama a nova função que gera o Token
            var token = _authService.GerarTokenJwt(usuarioValidado);

            return Ok(new 
            { 
                Mensagem = "Login autorizado!", 
                Usuario = usuarioValidado.NomeCompleto,
                Token = token // O front-end usará isso para acessar as rotas protegidas
            });
        }
        catch (System.Exception ex)
        {
            return BadRequest(new { Erro = ex.Message });
        }
    }

    // Rota criada especificamente para testar o bloqueio do middleware
    [Authorize]
    [HttpGet("teste-protegido")]
    public IActionResult TesteProtegido()
    {
        return Ok(new { Mensagem = "Você tem acesso! Seu token JWT é válido e foi reconhecido." });
    }
}