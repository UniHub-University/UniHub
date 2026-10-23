using System;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Domain;

public class DoacaoTests
{
    // ---------- ConfirmarEntrega ----------
    [Fact]
    public void ConfirmarEntrega_QuandoPendente_MudaStatusParaConcluida()
    {
        var doacao = new Doacao(); // nasce Pendente
        doacao.ConfirmarEntrega();
        Assert.Equal(StatusDoacao.Concluida, doacao.Status);
    }

    [Fact]
    public void ConfirmarEntrega_QuandoNaoEstaPendente_LancaExcecao()
    {
        var doacao = new Doacao();
        doacao.ConfirmarEntrega(); // agora Concluida (não mais Pendente)
        Assert.Throws<InvalidOperationException>(() => doacao.ConfirmarEntrega());
    }

    // ---------- Cancelar ----------
    [Fact]
    public void Cancelar_QuandoPendente_MudaStatusParaCancelada()
    {
        var doacao = new Doacao();
        doacao.Cancelar();
        Assert.Equal(StatusDoacao.Cancelada, doacao.Status);
    }

    [Fact]
    public void Cancelar_QuandoConcluida_LancaExcecao()
    {
        // Arrange — leva até Concluida (Pendente -> ConfirmarEntrega)
        var doacao = new Doacao();
        doacao.ConfirmarEntrega();

        // Act + Assert — cancelar uma já entregue/concluída deve lançar
        Assert.Throws<InvalidOperationException>(() => doacao.Cancelar());
    }
}
