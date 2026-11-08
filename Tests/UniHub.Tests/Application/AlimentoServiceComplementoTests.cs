using NSubstitute;
using UniHub.Application.DTOs.Alimentacao;
using UniHub.Application.Interfaces.Alimentacao;
using UniHub.Application.Services.Alimentacao;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

/// <summary>
/// Testes complementares do AlimentoService — cobrem lacunas apontadas no review
/// do PR #68 (BRV-152): autorização por posse em Inativar/Reativar, reativação
/// bem-sucedida, e validações de preço e categoria.
///
/// Mesmo padrão do AlimentoServiceTests: NSubstitute no IAlimentoRepository,
/// sem banco (unidade).
/// </summary>
[Trait("Categoria", "Unit")]
public class AlimentoServiceComplementoTests
{
    private readonly IAlimentoRepository _repository =
        Substitute.For<IAlimentoRepository>();

    private AlimentoService CriarService() => new AlimentoService(_repository);

    // ---------- Posse: não-dono em Inativar ----------
    [Fact]
    public async Task Inativar_AlimentoDeOutroVendedor_NaoInativa()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var alimentoId = Guid.NewGuid();

        _repository.ObterVendedorIdPorUsuarioAsync(usuarioId).Returns(vendedorId);
        // alimento não pertence a este vendedor -> repositório devolve null
        _repository.ObterDoVendedorAsync(alimentoId, vendedorId).Returns((Alimento?)null);

        var resultado = await CriarService().InativarAsync(usuarioId, alimentoId);

        Assert.False(resultado);
        await _repository.DidNotReceive().InativarAsync(Arg.Any<Guid>());
    }

    // ---------- Posse: não-dono em Reativar ----------
    [Fact]
    public async Task Reativar_AlimentoDeOutroVendedor_NaoReativa()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var alimentoId = Guid.NewGuid();

        _repository.ObterVendedorIdPorUsuarioAsync(usuarioId).Returns(vendedorId);
        _repository.ObterDoVendedorAsync(alimentoId, vendedorId).Returns((Alimento?)null);

        var resultado = await CriarService().ReativarAsync(usuarioId, alimentoId);

        Assert.False(resultado);
        await _repository.DidNotReceive().ReativarAsync(Arg.Any<Guid>());
    }

    // ---------- Reativação bem-sucedida (dono + estoque) ----------
    [Fact]
    public async Task Reativar_AlimentoDoVendedorComEstoque_Reativa()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var alimentoId = Guid.NewGuid();

        _repository.ObterVendedorIdPorUsuarioAsync(usuarioId).Returns(vendedorId);
        _repository.ObterDoVendedorAsync(alimentoId, vendedorId).Returns(new Alimento
        {
            Id = alimentoId,
            VendedorId = vendedorId,
            QuantidadeDisponivel = 5,
            Status = ProdutoStatus.Inativo
        });
        _repository.ReativarAsync(alimentoId).Returns(true);

        var resultado = await CriarService().ReativarAsync(usuarioId, alimentoId);

        Assert.True(resultado);
        await _repository.Received(1).ReativarAsync(alimentoId);
    }

    // ---------- Validação: preço negativo ----------
    [Fact]
    public async Task Criar_PrecoNegativo_LancaExcecao()
    {
        var dto = new CriarAlimentoDto(
            "Coxinha",
            null,
            null,
            -1,              // preço negativo
            2,
            "Salgado",
            null,
            new TimeSpan(18, 0, 0));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CriarService().CriarAsync(Guid.NewGuid(), dto));

        await _repository.DidNotReceive().AdicionarAsync(Arg.Any<Alimento>());
    }

    // ---------- Validação: categoria vazia ----------
    [Fact]
    public async Task Criar_CategoriaVazia_LancaExcecao()
    {
        var dto = new CriarAlimentoDto(
            "Coxinha",
            null,
            null,
            5,
            2,
            "   ",            // categoria em branco
            null,
            new TimeSpan(18, 0, 0));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CriarService().CriarAsync(Guid.NewGuid(), dto));

        await _repository.DidNotReceive().AdicionarAsync(Arg.Any<Alimento>());
    }
}
