using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs.Denuncias;
using UniHub.Application.Interfaces;
using UniHub.Domain.Entities;

namespace UniHub.API.Controllers
{
    [ApiController]
    [Route("api/denuncias")]
    [Authorize] // apenas usuários logados podem denunciar
    public class DenunciaController : ControllerBase
    {
        private readonly IDenunciaRepository _denunciaRepository;

        public DenunciasController(IDenunciaRepository denunciaRepository)
        {
            _denunciaRepository = denunciaRepository;

        }

        [HttpPost]
        public async Task<IActionResult> CriarDenuncia([FromBody] CriarDenunciaDto dto)
        {
            //validacao automatica do ModelState ocorre aqui por conta do [ApiController] e as DataAnnotations do DTO

            //Extrai o ID do usuário logado de dentro do token JWT (autenticação)
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid usuarioLogadoId))
            {
                return Unauthorized(new {mensagem = "Usuário inválido ou não autenticado."});
            }

            // Mapeia o DTO 
            var denuncia = new Denuncia
            {
                DenuncianteId = usuarioLogadoId, // pego com segurança do Token
                AlvoId = dto.AlvoId,
                TipoAlvo = dto.TipoAlvo,
                Motivo = dto.Motivo,
                Status = "Pendente",
                DataCriacao = DateTime.UtcNow
            };

            await _denunciaRepository.CriarAsync(denuncia);

            return StatusCode(201, new {mensagem = "Denúncia registrada com sucesso para análise da administração."});
        }
    }
}