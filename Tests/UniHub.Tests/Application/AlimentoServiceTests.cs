using NSubstitute;
using UniHub.Application.DTOs.Alimentacao;
using UniHub.Application.Interfaces.Alimentacao;
using UniHub.Application.Services.Alimentacao;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

/// <summary>
/// Testes de unidade do AlimentoService.
///
/// Os testes usam NSubstitute para substituir o IAlimentoRepository.
/// Dessa forma, validamos as regras de negócio do service sem acessar o Neon,
/// sem depender de migration e sem executar consultas reais no banco.
///
/// O conjunto cobre:
/// - criação de alimentos;
/// - validação de quantidade e horário;
/// - definição automática do status pelo estoque;
/// - exigência de perfil de vendedor;
/// - autorização por posse do alimento;
/// - inativação lógica;
/// - bloqueio da reativação quando não há estoque.
///
/// Fora de escopo:
/// - testes de integração com o Neon;
/// - testes HTTP do AlimentosController;
/// - fluxo de compra e criação de pedidos;
/// - histórico de pedidos;
/// - endpoint geral de produtos do vendedor.
///
/// Esses comportamentos pertencem, respectivamente, aos testes de integração,
/// à camada API, à Frente B e ao Squad C.
/// </summary>

[Trait("Categoria", "Unit")]
public class AlimentoServiceTests
{
    private readonly IAlimentoRepository _repository =
        Substitute.For<IAlimentoRepository>();

    private AlimentoService CriarService()
    {
        return new AlimentoService(_repository);
    }

    [Fact]
    public async Task Criar_QuantidadePositiva_DefineStatusDisponivel()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();

        _repository
            .ObterVendedorIdPorUsuarioAsync(usuarioId)
            .Returns(vendedorId);

        _repository
            .ObterPorIdComVendedorAsync(Arg.Any<Guid>())
            .Returns(new Alimento
            {
                Nome = "Coxinha",
                Preco = 5,
                QuantidadeDisponivel = 3,
                Categoria = "Salgado",
                Status = ProdutoStatus.Disponivel,
                VendedorId = vendedorId,
                Vendedor = new Vendedor
                {
                    Id = vendedorId
                }
            });

        var dto = new CriarAlimentoDto(
            "Coxinha",
            "Coxinha de frango",
            5,
            3,
            "Salgado",
            null,
            new TimeSpan(18, 0, 0));

        var resultado = await CriarService().CriarAsync(usuarioId, dto);

        Assert.Equal(ProdutoStatus.Disponivel, resultado.Status);

        await _repository.Received(1).AdicionarAsync(
            Arg.Is<Alimento>(a =>
                a.Nome == "Coxinha" &&
                a.QuantidadeDisponivel == 3 &&
                a.Status == ProdutoStatus.Disponivel &&
                a.VendedorId == vendedorId));
    }

    [Fact]
    public async Task Criar_QuantidadeZero_DefineStatusEsgotado()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();

        _repository
            .ObterVendedorIdPorUsuarioAsync(usuarioId)
            .Returns(vendedorId);

        _repository
            .ObterPorIdComVendedorAsync(Arg.Any<Guid>())
            .Returns(new Alimento
            {
                Nome = "Suco",
                Preco = 4,
                QuantidadeDisponivel = 0,
                Categoria = "Bebida",
                Status = ProdutoStatus.Esgotado,
                VendedorId = vendedorId,
                Vendedor = new Vendedor { Id = vendedorId }
            });

        var dto = new CriarAlimentoDto(
            "Suco",
            null,
            4,
            0,
            "Bebida",
            null,
            new TimeSpan(12, 0, 0));

        await CriarService().CriarAsync(usuarioId, dto);

        await _repository.Received(1).AdicionarAsync(
            Arg.Is<Alimento>(a =>
                a.QuantidadeDisponivel == 0 &&
                a.Status == ProdutoStatus.Esgotado));
    }

    [Fact]
    public async Task Criar_QuantidadeNegativa_LancaExcecao()
    {
        var dto = new CriarAlimentoDto(
            "Coxinha",
            null,
            5,
            -1,
            "Salgado",
            null,
            new TimeSpan(18, 0, 0));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CriarService().CriarAsync(Guid.NewGuid(), dto));

        await _repository.DidNotReceive()
            .AdicionarAsync(Arg.Any<Alimento>());
    }

    [Fact]
    public async Task Criar_HorarioInvalido_LancaExcecao()
    {
        var dto = new CriarAlimentoDto(
            "Coxinha",
            null,
            5,
            2,
            "Salgado",
            null,
            TimeSpan.FromDays(1));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            CriarService().CriarAsync(Guid.NewGuid(), dto));

        await _repository.DidNotReceive()
            .AdicionarAsync(Arg.Any<Alimento>());
    }

    // verifica que um usuário autenticado, mas sem perfil de vendedor, não pode publicar alimento
    [Fact]
    public async Task Criar_UsuarioSemPerfilDeVendedor_LancaExcecao()
    {
        var usuarioId = Guid.NewGuid();

        _repository
            .ObterVendedorIdPorUsuarioAsync(usuarioId)
            .Returns((Guid?)null);

        var dto = new CriarAlimentoDto(
            "Coxinha",
            null,
            5,
            2,
            "Salgado",
            null,
            new TimeSpan(18, 0, 0));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CriarService().CriarAsync(usuarioId, dto));

        await _repository.DidNotReceive()
            .AdicionarAsync(Arg.Any<Alimento>());
    }

    // Mesmo que o usuário esteja autenticado, ele não pode editar um alimento 
    // que não pertence ao dele próprio
    [Fact]
    public async Task Atualizar_AlimentoDeOutroVendedor_NaoAtualiza()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var alimentoId = Guid.NewGuid();

        _repository
            .ObterVendedorIdPorUsuarioAsync(usuarioId)
            .Returns(vendedorId);

        _repository
            .ObterDoVendedorAsync(alimentoId, vendedorId)
            .Returns((Alimento?)null);

        var dto = new AtualizarAlimentoDto(
            "Novo nome",
            null,
            6,
            2,
            "Salgado",
            null,
            new TimeSpan(18, 0, 0));

        var resultado = await CriarService()
            .AtualizarAsync(usuarioId, alimentoId, dto);

        Assert.False(resultado);

        await _repository.DidNotReceive()
            .AtualizarAsync(Arg.Any<Alimento>());
    }

    [Fact]
    public async Task Inativar_AlimentoDoVendedor_ChamaInativacao()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var alimentoId = Guid.NewGuid();

        _repository
            .ObterVendedorIdPorUsuarioAsync(usuarioId)
            .Returns(vendedorId);

        _repository
            .ObterDoVendedorAsync(alimentoId, vendedorId)
            .Returns(new Alimento
            {
                Id = alimentoId,
                VendedorId = vendedorId,
                Nome = "Bolo",
                Status = ProdutoStatus.Disponivel
            });

        _repository
            .InativarAsync(alimentoId)
            .Returns(true);

        var resultado = await CriarService()
            .InativarAsync(usuarioId, alimentoId);

        Assert.True(resultado);

        await _repository.Received(1)
            .InativarAsync(alimentoId);
    }

    // um alimento inativo com estoque = 0 não pode ser reativado
    [Fact]
    public async Task Reativar_SemEstoque_NaoReativa()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var alimentoId = Guid.NewGuid();

        _repository
            .ObterVendedorIdPorUsuarioAsync(usuarioId)
            .Returns(vendedorId);

        _repository
            .ObterDoVendedorAsync(alimentoId, vendedorId)
            .Returns(new Alimento
            {
                Id = alimentoId,
                VendedorId = vendedorId,
                QuantidadeDisponivel = 0,
                Status = ProdutoStatus.Inativo
            });

        var resultado = await CriarService()
            .ReativarAsync(usuarioId, alimentoId);

        Assert.False(resultado);

        await _repository.DidNotReceive()
            .ReativarAsync(Arg.Any<Guid>());
    }
}
