using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UniHub.Application.DTOs.Denuncias;
using UniHub.Application.Interfaces;
using UniHub.Application.Interfaces.Vendas;
using UniHub.Domain.Entities;
using Microsoft.AspNetCore.RateLimiting;

namespace UniHub.API.Controllers
{
    [ApiController]
    [Route("api/denuncias")]
    [Authorize] // apenas usuários logados podem denunciar
    public class DenunciasController : ControllerBase
    {
        private readonly IDenunciaRepository _denunciaRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly IVendedorConsultaRepository _vendedorRepository;

        public DenunciasController(IDenunciaRepository denunciaRepository, IUsuarioRepository usuarioRepository, IVendedorConsultaRepository vendedorRepository)
        {
            _denunciaRepository = denunciaRepository;
            _usuarioRepository = usuarioRepository;
            _vendedorRepository = vendedorRepository;
        }

        [EnableRateLimiting("sensivel")]
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

            // Resolve o denunciado: aceita VendedorId (traduz para o UsuarioId dono)
            // ou DenunciadoId direto. O front público manda o VendedorId.
            Guid denunciadoId;
            if (dto.VendedorId.HasValue)
            {
                var donoId = await _vendedorRepository.ObterUsuarioIdPorVendedorAsync(dto.VendedorId.Value);
                if (donoId is null) return NotFound(new { mensagem = "Vendedor não encontrado."} );
                denunciadoId = donoId.Value;
            }
            else if (dto.DenunciadoId.HasValue)
            {
                denunciadoId = dto.DenunciadoId.Value;
            }
            else
            {
                return BadRequest(new {mensagem = "Informe o vendedor (VendedorId) ou o usuário denunciado (DenunciadoId)."});
            }

            // Impedir auto-denúncia
            if(denunciadoId == usuarioLogadoId)
            {
                return BadRequest(new { mensagem = "Você não pode denunciar a si mesmo."});
            }

            // Validar se o denunciado existe no banco para evitar erro de Foreign Key 500
            var denunciado = await _usuarioRepository.ObterPorIdAsync(denunciadoId);
            if (denunciado == null)
            {
                return NotFound(new {mensagem = "O usuário denunciado não foi encontrado no sistema."});
            }

            //Converte a string enviada para o Enum DenunciaMotivo
            if (!Enum.TryParse<DenunciaMotivo>(dto.Motivo, ignoreCase: true, out var motivo))
                return BadRequest(new { mensagem = "Motivo inválido. Use: ConteudoOfensivo, ComportamentoInadequado, "
                                +"Assedio, Fraude, ProdutoInadequado ou Outro." });

            // Bloqueia denúncia duplicada: mesmo denunciante, mesmo denunciado e mesmo
            // produto (incluindo o caso sem produto) já com uma denúncia em andamento.
            var jaExiste = await _denunciaRepository.ExisteDenunciaEmAndamentoAsync(usuarioLogadoId, denunciadoId, dto.ProdutoId);
            if (jaExiste)
                return Conflict(new { mensagem = "Você já tem uma denúncia em andamento para este alvo." });

            // Mapeia o DTO 
            var denuncia = new Denuncia
            {
                DenuncianteId = usuarioLogadoId,
                DenunciadoId = denunciadoId,
                ProdutoId = dto.ProdutoId,
                Descricao = dto.Descricao,
                Motivo = motivo
            };
    
            await _denunciaRepository.CriarAsync(denuncia);

            return StatusCode(201, new {mensagem = "Denúncia registrada com sucesso para análise da administração."});
        }
    }
}
