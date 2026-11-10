using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces.Vendas;

// Consultas de leitura sobre vendedores para a visao publica.
public interface IVendedorPublicoRepository
{
    // Vendedor com Usuario, Locais e Horarios carregados (somente leitura).
    // Devolve null se o vendedor nao existe ou se o usuario dele esta inativo.
    Task<Vendedor?> ObterPublicoAsync(Guid vendedorId);

    // Lista publica: vendedores com usuario ativo e ao menos um anuncio disponivel
    // (Status Disponivel e estoque > 0), do mais bem avaliado para o menos, com
    // desempate estavel. page eh 1-based; os limites sao validados no Service.
    // Devolve a pagina (com Usuario carregado) e o total de vendedores listaveis.
    Task<(IReadOnlyList<Vendedor> Vendedores, int Total)> ListarPublicosAsync(int page, int pageSize);
}
