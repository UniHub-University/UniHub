using System;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using UniHub.Application.DTOs.Alimentacao;
using UniHub.Application.Interfaces;
using UniHub.Application.Interfaces.Alimentacao;
using UniHub.Application.Services.Alimentacao;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Application;

// Robustez (issue #104, H2): a remocao da imagem antiga no Cloudinary e um
// efeito colateral best-effort. Se o storage falhar (ex.: Cloudinary fora do
// ar), a edicao do produto NAO pode quebrar. Aqui o IImageStorageService e
// mockado para LANCAR na remocao, e o teste verifica que AtualizarAsync ainda
// conclui com sucesso e persiste a nova URL. (AGENTS secao 7.1)
[Trait("Categoria", "Unit")]
public class AlimentoServiceRobustezImagemTests
{
    private readonly IAlimentoRepository _repo = Substitute.For<IAlimentoRepository>();
    private readonly IImageStorageService _storage = Substitute.For<IImageStorageService>();

    private AlimentoService CriarService() => new(_repo, _storage);

    [Fact]
    public async Task Atualizar_QuandoRemocaoDaImagemAntigaFalha_AindaAtualizaComSucesso()
    {
        var usuarioId = Guid.NewGuid();
        var vendedorId = Guid.NewGuid();
        var alimento = new Alimento
        {
            Id = Guid.NewGuid(),
            VendedorId = vendedorId,
            Nome = "Coxinha",
            Preco = 5m,
            QuantidadeDisponivel = 3,
            Categoria = "Salgado",
            Status = ProdutoStatus.Disponivel,
            ImagemUrl = "https://res.cloudinary.com/demo/image/upload/antiga.jpg",
            HorarioDisponibilidade = TimeSpan.FromHours(12)
        };

        // O service resolve o vendedor pelo usuario e busca o alimento do vendedor.
        _repo.ObterVendedorIdPorUsuarioAsync(usuarioId).Returns(vendedorId);
        _repo.ObterDoVendedorAsync(alimento.Id, vendedorId).Returns(alimento);

        // ExtrairPublicId devolve um id valido (URL antiga e do provedor)...
        _storage.ExtrairPublicId(Arg.Any<string>()).Returns("antiga");
        // ...mas o DeleteAsync LANCA (falha de infraestrutura do Cloudinary).
        _storage.DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Throws(new Exception("Cloudinary indisponivel"));

        // Troca a imagem (dispara a remocao da antiga).
        var dto = new AtualizarAlimentoDto(
            "Coxinha",
            null,
            "https://res.cloudinary.com/demo/image/upload/nova.jpg",
            5m,
            3,
            "Salgado",
            null,
            TimeSpan.FromHours(12));

        // Nao deve lancar: a falha da remocao e best-effort.
        var ok = await CriarService().AtualizarAsync(usuarioId, alimento.Id, dto);

        Assert.True(ok);
        // A nova URL foi persistida mesmo com a remocao da antiga falhando.
        Assert.Equal("https://res.cloudinary.com/demo/image/upload/nova.jpg", alimento.ImagemUrl);
        await _repo.Received(1).AtualizarAsync(alimento);
    }
}
