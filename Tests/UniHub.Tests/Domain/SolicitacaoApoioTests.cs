using System;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Domain;

[Trait("Categoria", "Unit")]
public class SolicitacaoApoioTests
{
    // ---------- MarcarEmAndamento ----------
    [Fact]
    public void MarcarEmAndamento_QuandoAberta_MudaStatusParaEmAndamento()
    {
        var solicitacao = new SolicitacaoApoio(); // nasce Aberta
        solicitacao.MarcarEmAndamento();
        Assert.Equal(StatusSolicitacao.EmAndamento, solicitacao.Status);
    }

    [Fact]
    public void MarcarEmAndamento_QuandoNaoEstaAberta_LancaExcecao()
    {
        var solicitacao = new SolicitacaoApoio();
        solicitacao.MarcarEmAndamento(); // agora EmAndamento (não mais Aberta)
        Assert.Throws<InvalidOperationException>(() => solicitacao.MarcarEmAndamento());
    }

    // ---------- Concluir ----------
    [Fact]
    public void Concluir_QuandoEmAndamento_MudaStatusParaConcluida()
    {
        var solicitacao = new SolicitacaoApoio();
        solicitacao.MarcarEmAndamento(); // pré-requisito: precisa estar EmAndamento
        solicitacao.Concluir();
        Assert.Equal(StatusSolicitacao.Concluida, solicitacao.Status);
    }

    [Fact]
    public void Concluir_QuandoAberta_LancaExcecao()
    {
        var solicitacao = new SolicitacaoApoio(); // Aberta (não EmAndamento)
        Assert.Throws<InvalidOperationException>(() => solicitacao.Concluir());
    }

    // ---------- Cancelar ----------
    [Fact]
    public void Cancelar_QuandoAberta_MudaStatusParaCancelada()
    {
        var solicitacao = new SolicitacaoApoio();
        solicitacao.Cancelar();
        Assert.Equal(StatusSolicitacao.Cancelada, solicitacao.Status);
    }

    [Fact]
    public void Cancelar_QuandoConcluida_LancaExcecao()
    {
        // Arrange — leva até Concluida (Aberta -> EmAndamento -> Concluida)
        var solicitacao = new SolicitacaoApoio();
        solicitacao.MarcarEmAndamento();
        solicitacao.Concluir();

        // Act + Assert — cancelar uma concluída deve lançar
        Assert.Throws<InvalidOperationException>(() => solicitacao.Cancelar());
    }
}
