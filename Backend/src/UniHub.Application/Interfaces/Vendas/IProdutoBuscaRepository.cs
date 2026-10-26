using UniHub.Application.DTOs.Common;
using UniHub.Application.DTOs.Produtos;

namespace UniHub.Application.Interfaces.Vendas;

// Contrato de leitura/consulta de produtos. Separado do IProdutoRepository
// (que cuida de estoque/concorrencia) para nao misturar responsabilidades.
public interface IProdutoBuscaRepository
{
    Task<PaginatedResultDto<ProdutoResumoDto>> BuscarAsync(FiltrosBuscaProdutos filtros);
}
