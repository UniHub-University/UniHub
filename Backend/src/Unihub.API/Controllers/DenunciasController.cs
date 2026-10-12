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
    public class DenunciasController : ControllerBase
    {
        private readonly IDenunciaRepository _denunciaRepository;
        private readonly IUsuarioRepository _usuarioRepository;

        public DenunciasController(IDenunciaRepository denunciaRepository, IUsuarioRepository usuarioRepository)
        {
            _denunciaRepository = denunciaRepository;
            _usuarioRepository = usuarioRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CriarDenuncia([FromBody] CriarDenunciaDto dto)
        {
            //validacao automatica do ModelState ocorre aqui por conta do [ApiController] e as DataAnnotations do DTO

            //Extrai o ID do usuário logado de dentro do token JWT (autenticação)
           var userIdString = User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid usuarioLogadoId))
            {
                return Unauthorized(new { mensagem = "Usuário inválido ou não autenticado. Verifique as Claims do Token." });
            }

            // Impedir auto-denúncia
            if(dto.DenunciadoId == usuarioLogadoId)
            {
                return BadRequest(new { mensagem = "Você não pode denunciar a si mesmo."});
            }

            // Validar se o denunciado existe no banco para evitar erro de Foreign Key 500
            var denunciado = await _usuarioRepository.ObterPorIdAsync(dto.DenunciadoId);
            if (denunciado == null)
            {
                return NotFound(new {mensagem = "O usuário denunciado não foi encontrado no sistema."});
            }
            
            // Mapeia o DTO 
            var denuncia = new Denuncia
            {
                DenuncianteId = usuarioLogadoId, // pego com segurança do Token
                DenunciadoId = dto.DenunciadoId,
                ProdutoId = dto.ProdutoId,
                Descricao = dto.Descricao,

                //Converte a string enviada para o Enum DenunciaMotivo
               if (!Enum.TryParse<DenunciaMotivo>(dto.Motivo, ignoreCase: true, out var motivo))
    return BadRequest(new { mensagem = "Motivo inválido. Use: ConteudoOfensivo, ComportamentoInadequado, Assedio, Fraude, ProdutoInadequado ou Outro." });

var denuncia = new Denuncia
{
    DenuncianteId = usuarioLogadoId,
    DenunciadoId = dto.DenunciadoId,
    ProdutoId = dto.ProdutoId,
    Descricao = dto.Descricao,
    Motivo = motivo
};


            };

            await _denunciaRepository.CriarAsync(denuncia);

            return StatusCode(201, new {mensagem = "Denúncia registrada com sucesso para análise da administração."});
        }
    }
}