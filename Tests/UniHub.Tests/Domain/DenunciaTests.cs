using System;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Domain;

[Trait("Categoria", "Unit")]
public class DenunciaTests
{
    // ---------- A partir de Aberta (estado inicial) ----------
    [Theory]
    [InlineData(DenunciaStatus.EmAnalise)]
    [InlineData(DenunciaStatus.Resolvida)]
    [InlineData(DenunciaStatus.Rejeitada)]
    [InlineData(DenunciaStatus.Aberta)] // mesmo status também é um valor válido
    public void AtualizarStatus_QuandoAberta_AceitaQualquerStatusValido(DenunciaStatus novo)
    {
        var denuncia = new Denuncia(); // nasce Aberta

        denuncia.AtualizarStatus(novo);

        Assert.Equal(novo, denuncia.Status);
    }

    // ---------- A partir de EmAnalise ----------
    [Theory]
    [InlineData(DenunciaStatus.Aberta)]
    [InlineData(DenunciaStatus.Resolvida)]
    [InlineData(DenunciaStatus.Rejeitada)]
    public void AtualizarStatus_QuandoEmAnalise_AceitaQualquerStatusValido(DenunciaStatus novo)
    {
        var denuncia = new Denuncia();
        denuncia.AtualizarStatus(DenunciaStatus.EmAnalise);

        denuncia.AtualizarStatus(novo);

        Assert.Equal(novo, denuncia.Status);
    }

    // ---------- Estados finais não mudam ----------
    [Theory]
    [InlineData(DenunciaStatus.Aberta)]
    [InlineData(DenunciaStatus.EmAnalise)]
    [InlineData(DenunciaStatus.Rejeitada)]
    [InlineData(DenunciaStatus.Resolvida)]
    public void AtualizarStatus_QuandoResolvida_NaoPermiteNenhumaAlteracao(DenunciaStatus tentativa)
    {
        var denuncia = new Denuncia();
        denuncia.AtualizarStatus(DenunciaStatus.Resolvida); // estado final

        Assert.Throws<InvalidOperationException>(() => denuncia.AtualizarStatus(tentativa));
        Assert.Equal(DenunciaStatus.Resolvida, denuncia.Status); // permanece inalterada
    }

    [Theory]
    [InlineData(DenunciaStatus.Aberta)]
    [InlineData(DenunciaStatus.EmAnalise)]
    [InlineData(DenunciaStatus.Resolvida)]
    [InlineData(DenunciaStatus.Rejeitada)]
    public void AtualizarStatus_QuandoRejeitada_NaoPermiteNenhumaAlteracao(DenunciaStatus tentativa)
    {
        var denuncia = new Denuncia();
        denuncia.AtualizarStatus(DenunciaStatus.Rejeitada); // estado final

        Assert.Throws<InvalidOperationException>(() => denuncia.AtualizarStatus(tentativa));
        Assert.Equal(DenunciaStatus.Rejeitada, denuncia.Status);
    }

    // ---------- Valor fora do enum ----------
    [Fact]
    public void AtualizarStatus_ComValorForaDoEnum_LancaArgumentException()
    {
        var denuncia = new Denuncia();

        Assert.Throws<ArgumentException>(() => denuncia.AtualizarStatus((DenunciaStatus)99));
    }
}
