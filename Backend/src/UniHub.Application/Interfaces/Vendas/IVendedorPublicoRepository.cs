using UniHub.Domain.Entities;

namespace UniHub.Application.Interfaces.Vendas;

// Consultas de leitura sobre vendedores para a visao publica.
public interface IVendedorPublicoRepository
{
    // Vendedor com Usuario, Locais e Horarios carregados (somente leitura).
    // Devolve null se o vendedor nao existe ou se o usuario dele esta inativo.
    Task<Vendedor?> ObterPublicoAsync(Guid vendedorId);
}
