using Microsoft.EntityFrameworkCore;
using UniHub.Application.DTOs;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Application.Services;

public class VoluntarioService
{
    private const string SemPerfil = "Usuário não possui um perfil de voluntário.";

    private readonly AppDbContext _context;

    public VoluntarioService(AppDbContext context)
    {
        _context = context;
    }

    /// Cria o perfil de Voluntario do usuario, se ele ainda nao tiver um.
    /// Ser voluntario NAO altera Usuario.Role nem depende de ser (ou nao)
    /// Vendedor: os dois perfis sao independentes (1:0..1 cada um).
    public async Task<VoluntarioResultDto> TornarSeVoluntarioAsync(Guid usuarioId)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Voluntario)
            .FirstOrDefaultAsync(u => u.Id == usuarioId);

        if (usuario is null)
            return Falha("Usuário não encontrado.");

        if (!usuario.IsActive)
            return Falha("Conta suspensa. Procure a administração.");

        if (usuario.Voluntario is not null)
            return new VoluntarioResultDto(false, "Usuário já possui um perfil de voluntário.", ParaDto(usuario.Voluntario));

        var voluntario = usuario.TornarSeVoluntario();

        // Add explicito (mesmo padrao do TornarSeVendedorAsync): o Id e gerado no
        // construtor, entao sem o Add o EF trataria a entidade como "Modified" e
        // tentaria um UPDATE em vez de INSERT.
        _context.Voluntarios.Add(voluntario);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Duas requisicoes simultaneas: o indice unico em Voluntarios.UsuarioId
            // barra a segunda. Qualquer outra falha continua subindo.
            if (await _context.Voluntarios.AnyAsync(v => v.UsuarioId == usuarioId))
                return Falha("Usuário já possui um perfil de voluntário.");
            throw;
        }

        return new VoluntarioResultDto(true, "Perfil de voluntário criado com sucesso!", ParaDto(voluntario));
    }

    public async Task<VoluntarioResultDto> ObterMeuPerfilAsync(Guid usuarioId)
    {
        var voluntario = await _context.Voluntarios
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);

        return voluntario is null
            ? Falha(SemPerfil)
            : new VoluntarioResultDto(true, "Perfil de voluntário.", ParaDto(voluntario));
    }

    /// Define (ou edita) as areas de atuacao. Substitui a lista inteira.
    public async Task<VoluntarioResultDto> DefinirAreasAsync(Guid usuarioId, DefinirAreasDto dto)
    {
        if (dto.Areas is null || dto.Areas.Count == 0)
            return Falha("Informe ao menos uma área de atuação.");

        // sem JsonStringEnumConverter, o JSON aceita qualquer inteiro (ex.: 99)
        if (dto.Areas.Any(a => !Enum.IsDefined(a)))
            return Falha("Área de atuação inválida.");

        var voluntario = await _context.Voluntarios.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
        if (voluntario is null)
            return Falha(SemPerfil);

        voluntario.AreasDeAtuacao = dto.Areas.Distinct().OrderBy(a => a).ToList();
        await _context.SaveChangesAsync();

        return new VoluntarioResultDto(true, "Áreas de atuação atualizadas com sucesso!", ParaDto(voluntario));
    }

    public Task<VoluntarioResultDto> PausarAtuacaoAsync(Guid usuarioId) =>
        AlterarDisponibilidadeAsync(usuarioId, disponivel: false);

    public Task<VoluntarioResultDto> RetomarAtuacaoAsync(Guid usuarioId) =>
        AlterarDisponibilidadeAsync(usuarioId, disponivel: true);

    private async Task<VoluntarioResultDto> AlterarDisponibilidadeAsync(Guid usuarioId, bool disponivel)
    {
        var voluntario = await _context.Voluntarios.FirstOrDefaultAsync(v => v.UsuarioId == usuarioId);
        if (voluntario is null)
            return Falha(SemPerfil);

        if (disponivel)
            voluntario.RetomarAtuacao();
        else
            voluntario.PausarAtuacao();

        await _context.SaveChangesAsync();

        return new VoluntarioResultDto(
            true,
            disponivel ? "Atuação retomada." : "Atuação pausada.",
            ParaDto(voluntario));
    }

    private static VoluntarioResultDto Falha(string mensagem) =>
        new VoluntarioResultDto(false, mensagem, null);

    private static VoluntarioRespostaDto ParaDto(Voluntario v) =>
        new VoluntarioRespostaDto(v.CodigoPublico, v.AreasDeAtuacao.ToList(), v.Disponivel);
}
