using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using UniHub.Domain.Entities;
using UniHub.Infrastructure.Data;
using UniHub.Infrastructure.Repositories.Vendas;
using Xunit;

namespace UniHub.Tests.Integracao;

/// <summary>
/// Teste do requisito técnico central do projeto (AGENTS.md, seção 7):
/// o decremento de estoque usa UPDATE condicional atômico no Postgres, então
/// NÃO pode haver venda além do estoque, mesmo com N compradores simultâneos
/// disputando a mesma linha.
///
/// POR QUE ESTE TESTE É DIFERENTE DOS OUTROS DE INTEGRAÇÃO:
/// concorrência de verdade exige CONEXÕES SEPARADAS rodando em paralelo (cada
/// Task com seu próprio AppDbContext), com COMMIT real — não dá para usar o
/// padrão transação+rollback da IntegracaoTestBase, porque:
///   - um único DbContext serializaria as operações no cliente (sem disputa real);
///   - transações separadas não enxergariam o produto criado por outra;
///   - o UPDATE precisa commitar para a corrida existir.
///
/// CONSEQUÊNCIA: este teste GRAVA de verdade no banco development e faz a própria
/// limpeza no finally (cria IDs únicos, apaga tudo no fim). É uma exceção
/// consciente ao padrão de rollback — inevitável para provar concorrência.
/// </summary>
public class ConcorrenciaEstoqueIntegracaoTests : IntegracaoTestBase
{
    // Estoque = 1, 10 compradores simultâneos de 1 unidade cada.
    // Esperado: exatamente 1 sucesso, 9 falhas, estoque final = 0 (nunca negativo).
    [Fact]
    public async Task TryDecrementarEstoque_ComDezCompradoresDisputandoUltimaUnidade_SoUmVence()
    {
        await VerificarConcorrenciaAsync(estoqueInicial: 1, compradores: 10, sucessosEsperados: 1);
    }

    // Estoque = 5, 10 compradores simultâneos de 1 unidade cada.
    // Esperado: exatamente 5 sucessos, 5 falhas, estoque final = 0.
    [Fact]
    public async Task TryDecrementarEstoque_ComMaisCompradoresQueEstoque_SoVenceAteEsgotar()
    {
        await VerificarConcorrenciaAsync(estoqueInicial: 5, compradores: 10, sucessosEsperados: 5);
    }

    private async Task VerificarConcorrenciaAsync(int estoqueInicial, int compradores, int sucessosEsperados)
    {
        // Arrange — cria a árvore Usuario -> Vendedor -> Produto e COMMITA,
        // usando um contexto só para o setup.
        var (usuarioId, vendedorId, produtoId) = await SeedProdutoAsync(estoqueInicial);

        try
        {
            // Act — dispara N decrementos em paralelo, cada um com SEU PRÓPRIO
            // AppDbContext (conexão independente). É isso que cria a disputa real.
            var tarefas = Enumerable.Range(0, compradores).Select(async _ =>
            {
                await using var ctx = CriarContexto();
                var repo = new ProdutoRepository(ctx);
                return await repo.TryDecrementarEstoqueAsync(produtoId, 1);
            });

            bool[] resultados = await Task.WhenAll(tarefas);

            // Assert — número exato de vencedores e estoque final coerente
            var sucessos = resultados.Count(r => r);
            Assert.Equal(sucessosEsperados, sucessos);

            await using var verificacao = CriarContexto();
            var produto = await verificacao.Set<Produto>()
                .AsNoTracking()
                .FirstAsync(p => p.Id == produtoId);

            // o estoque nunca pode ficar negativo e deve refletir os sucessos
            Assert.Equal(estoqueInicial - sucessosEsperados, produto.QuantidadeDisponivel);
            Assert.True(produto.QuantidadeDisponivel >= 0, "Estoque ficou negativo — a atomicidade falhou.");
        }
        finally
        {
            // Cleanup — como não há rollback protetor, apagamos o que criamos.
            await LimparAsync(usuarioId, vendedorId, produtoId);
        }
    }

    private async Task<(Guid usuarioId, Guid vendedorId, Guid produtoId)> SeedProdutoAsync(int estoque)
    {
        await using var ctx = CriarContexto();

        var usuario = new Usuario
        {
            NomeCompleto = "Vendedor Concorrencia",
            EmailInstitucional = $"concorrencia-{Guid.NewGuid():N}@unifesp.br",
            IsActive = true
        };
        ctx.Usuarios.Add(usuario);

        var vendedor = new Vendedor { UsuarioId = usuario.Id };
        ctx.Vendedores.Add(vendedor);

        var produto = new ItemBazar
        {
            Nome = "Item Concorrencia",
            Preco = 10m,
            QuantidadeDisponivel = estoque,
            VendedorId = vendedor.Id,
            Status = ProdutoStatus.Disponivel
        };
        ctx.Produtos.Add(produto);

        await ctx.SaveChangesAsync(); // COMMIT real — os dados precisam existir para a disputa
        return (usuario.Id, vendedor.Id, produto.Id);
    }

    private async Task LimparAsync(Guid usuarioId, Guid vendedorId, Guid produtoId)
    {
        await using var ctx = CriarContexto();
        // ordem respeita as FKs: produto -> vendedor -> usuario
        await ctx.Set<Produto>().Where(p => p.Id == produtoId).ExecuteDeleteAsync();
        await ctx.Vendedores.Where(v => v.Id == vendedorId).ExecuteDeleteAsync();
        await ctx.Usuarios.Where(u => u.Id == usuarioId).ExecuteDeleteAsync();
    }
}
