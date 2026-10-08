using Microsoft.EntityFrameworkCore;
using UniHub.Application.Interfaces.AcaoSolidaria;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;

namespace UniHub.Infrastructure.Repositories.AcaoSolidaria;

public class SolicitacaoApoioRepository : ISolicitacaoApoioRepository
{
    private readonly AppDbContext _context;

    public SolicitacaoApoioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<bool> TryMarcarEmAndamentoAsync(Guid solicitacaoId)
    {
        // UPDATE condicional atômico: a transição Aberta -> EmAndamento só
        // acontece se a solicitação AINDA estiver Aberta. Dois voluntários
        // aceitando ao mesmo tempo disputam a mesma linha; o banco garante
        // que apenas um UPDATE afeta 1 linha. O outro recebe 0 e sabe que
        // perdeu a corrida. Mesmo padrão de TryDecrementarEstoqueAsync.
        var linhasAfetadas = await _context.Database.ExecuteSqlInterpolatedAsync(
            $@"UPDATE ""SolicitacoesApoio""
               SET ""Status"" = {(int)StatusSolicitacao.EmAndamento}
               WHERE ""Id"" = {solicitacaoId}
                 AND ""Status"" = {(int)StatusSolicitacao.Aberta}");

        return linhasAfetadas == 1;
    }
}
