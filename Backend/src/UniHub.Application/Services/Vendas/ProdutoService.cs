using UniHub.Application.Interfaces.Vendas;

namespace UniHub.Application.Services.Vendas;

public class ProdutoService
{
    private readonly IProdutoRepository _produtoRepository;

    public ProdutoService(IProdutoRepository produtoRepository)
    {
        _produtoRepository = produtoRepository;
    }

    // Lugar para qualquer regra de negócio futura
    public async Task<bool> InativarProdutoAsync(Guid produtoId)
    {
        return await _produtoRepository.InativarAsync(produtoId);
    }
}