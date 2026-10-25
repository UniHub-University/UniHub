using System;
using System.Threading.Tasks;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Repositories.Vendas;
using Xunit;

namespace UniHub.Tests.Integracao;

[Trait("Categoria", "Integracao")]
public class ProdutoRepositoryIntegracaoTests : IntegracaoTestBase
{
    [Fact]
    public async Task TryDecrementarEstoque_QuandoHaEstoque_DecrementaERetornaTrue()
    {
        // Arrange — conecta no Neon development e abre transação (tudo será desfeito)
        await using var context = CriarContexto();
        await using var transacao = await context.Database.BeginTransactionAsync();

        // Cadeia de dependências: Usuario -> Vendedor -> Produto.
        // O banco valida as FKs de verdade, então precisamos criar a árvore inteira.
        var usuario = new Usuario
        {
            NomeCompleto = "Vendedor Teste",
            EmailInstitucional = $"teste-{Guid.NewGuid():N}@unifesp.br" // único, para não colidir
        };
        context.Usuarios.Add(usuario);

        var vendedor = new Vendedor { UsuarioId = usuario.Id };
        context.Vendedores.Add(vendedor);

        var alimento = new Alimento
        {
            Nome = "Teste Integração",
            Preco = 10m,
            QuantidadeDisponivel = 5,
            VendedorId = vendedor.Id
        };
        context.Produtos.Add(alimento);
        await context.SaveChangesAsync();

        var repo = new ProdutoRepository(context);

        // Act
        var sucesso = await repo.TryDecrementarEstoqueAsync(alimento.Id, 2);

        // Assert
        Assert.True(sucesso);
        var atualizado = await repo.GetByIdAsync(alimento.Id);
        Assert.Equal(3, atualizado!.QuantidadeDisponivel);

        // Rollback — desfaz tudo; o banco development continua limpo
        await transacao.RollbackAsync();
    }

}
