using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs.AcaoSolidaria;
using UniHub.Application.Interfaces;
using UniHub.Application.Interfaces.AcaoSolidaria;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Solidariedade;

public class SolidariedadeService : ISolidariedadeService
{
    private readonly AppDbContext _db;
    private readonly ISolicitacaoApoioRepository _solicitacoes;

    public SolidariedadeService(AppDbContext db, ISolicitacaoApoioRepository solicitacoes)
    {
        _db = db;
        _solicitacoes = solicitacoes;
    }

    // ---------- solicitante ----------
    public async Task<List<MinhaSolicitacaoDto>> MinhasAsync(Guid usuarioId)
    {
        var lista = await _db.SolicitacoesApoio
            .Include(s => s.Correspondencias).ThenInclude(c => c.Voluntario)
            .Where(s => s.SolicitanteId == usuarioId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return lista.Select(s =>
        {
            var c = s.Correspondencias.FirstOrDefault(x =>
                x.Status is StatusCorrespondencia.AceitaPeloVoluntario or StatusCorrespondencia.Concluida);

            return new MinhaSolicitacaoDto(
                s.Id, s.CodigoPublico, s.Categoria, s.Descricao, s.Status,
                c?.Id, c?.Voluntario?.CodigoPublico, c?.IdentidadeRevelada ?? false);
        }).ToList();
    }

    public async Task AutorizarRevelacaoAsync(Guid usuarioId, Guid correspondenciaId)
    {
        var c = await ObterAsync(correspondenciaId);
        if (c.Solicitacao!.SolicitanteId != usuarioId) throw new UnauthorizedAccessException();

        c.AutorizarRevelacaoIdentidade();
        await _db.SaveChangesAsync();
    }

    // ---------- voluntário ----------
    public async Task TornarSeVoluntarioAsync(Guid usuarioId, CadastrarVoluntarioDto dto)
    {
        if (dto.Areas is null || dto.Areas.Count == 0 || dto.Areas.Any(a => !Enum.IsDefined(a)))
            throw new InvalidOperationException("Informe ao menos uma área de atuação válida.");

        var usuario = await _db.Usuarios.Include(u => u.Voluntario)
            .FirstOrDefaultAsync(u => u.Id == usuarioId)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        usuario.TornarSeVoluntario().AreasDeAtuacao = dto.Areas.Distinct().ToList();
        await _db.SaveChangesAsync();
    }

    // o match: solicitações abertas, das minhas categorias, exceto as minhas, mais antigas primeiro
    public async Task<List<SolicitacaoApoioRespostaDto>> SugestoesAsync(Guid usuarioId)
    {
        var voluntario = await _db.Voluntarios.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId)
            ?? throw new KeyNotFoundException("Você ainda não é voluntário.");

        var areas = voluntario.AreasDeAtuacao.ToList();

        return await _db.SolicitacoesApoio
            .AsNoTracking()
            .Where(s => s.Status == StatusSolicitacao.Aberta
                     && s.SolicitanteId != usuarioId
                     && areas.Contains(s.Categoria))
            .OrderBy(s => s.CreatedAt)
            .Take(20)
            .Select(s => new SolicitacaoApoioRespostaDto(
                s.Id, s.CodigoPublico, s.Categoria, s.Descricao, s.Status, s.CreatedAt))
            .ToListAsync();
    }

    public async Task AceitarAsync(Guid usuarioId, Guid solicitacaoId)
    {
        var voluntario = await _db.Voluntarios.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId)
            ?? throw new KeyNotFoundException("Você ainda não é voluntário.");

        // só valida existência e autoria; a entidade não é rastreada porque o UPDATE atômico altera a linha direto
        var solicitanteId = await _db.SolicitacoesApoio
            .AsNoTracking()
            .Where(s => s.Id == solicitacaoId)
            .Select(s => (Guid?)s.SolicitanteId)
            .FirstOrDefaultAsync()
            ?? throw new KeyNotFoundException("Solicitação não encontrada.");

        if (solicitanteId == usuarioId)
            throw new InvalidOperationException("Você não pode atender a própria solicitação.");

        // transação: se criar a correspondência falhar, o UPDATE da solicitação é desfeito
        await using var tx = await _db.Database.BeginTransactionAsync();

        if (!await _solicitacoes.TryMarcarEmAndamentoAsync(solicitacaoId))
            throw new DbUpdateConcurrencyException("Esta solicitação não está mais disponível.");

        var c = new Correspondencia { SolicitacaoId = solicitacaoId, VoluntarioId = voluntario.Id };
        c.Aceitar();
        _db.Correspondencias.Add(c);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();
    }

    public async Task<List<AtendimentoDto>> AtendimentosAsync(Guid usuarioId)
    {
        var lista = await _db.Correspondencias
            .Include(c => c.Solicitacao).ThenInclude(s => s!.Solicitante)
            .Where(c => c.Voluntario!.UsuarioId == usuarioId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

        return lista.Select(c => new AtendimentoDto(
            c.Id, c.Solicitacao!.CodigoPublico, c.Solicitacao.Categoria, c.Solicitacao.Descricao, c.Status,
            c.IdentidadeRevelada ? c.Solicitacao.Solicitante?.Nome : null,
            c.IdentidadeRevelada ? c.Solicitacao.Solicitante?.Telefone : null)).ToList();
    }

    // ---------- ambos ----------
    public async Task ConcluirAsync(Guid usuarioId, Guid correspondenciaId)
    {
        var c = await ObterAsync(correspondenciaId);
        if (c.Voluntario!.UsuarioId != usuarioId && c.Solicitacao!.SolicitanteId != usuarioId)
            throw new UnauthorizedAccessException();

        c.Concluir();
        await _db.SaveChangesAsync();
    }

    // ---------- helper ----------
    private async Task<Correspondencia> ObterAsync(Guid id) =>
        await _db.Correspondencias
            .Include(c => c.Voluntario)
            .Include(c => c.Solicitacao)
            .FirstOrDefaultAsync(c => c.Id == id)
        ?? throw new KeyNotFoundException("Correspondência não encontrada.");
}
