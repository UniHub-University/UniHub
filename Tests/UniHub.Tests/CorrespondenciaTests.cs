using System;
using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Domain;

// Agrupa os testes da entidade Correspondencia.
// Convenção de nome do teste: Metodo_Cenario_ResultadoEsperado
public class CorrespondenciaTests
{
    // ---------- Caminho feliz ----------
    [Fact]
    public void Aceitar_QuandoSugerida_MudaStatusParaAceitaPeloVoluntario()
    {
        // Arrange — uma correspondência recém-criada nasce como "Sugerida"
        var correspondencia = new Correspondencia();

        // Act — o voluntário aceita
        correspondencia.Aceitar();

        // Assert — o status mudou e o momento do aceite foi registrado
        Assert.Equal(StatusCorrespondencia.AceitaPeloVoluntario, correspondencia.Status);
        Assert.NotNull(correspondencia.AceitaEm);
    }

    // ---------- Caminho de erro ----------
    [Fact]
    public void Aceitar_QuandoJaNaoEstaSugerida_LancaExcecao()
    {
        // Arrange — uma correspondência que já foi aceita (não está mais "Sugerida")
        var correspondencia = new Correspondencia();
        correspondencia.Aceitar(); // agora está "AceitaPeloVoluntario"

        // Act + Assert — tentar aceitar de novo deve lançar InvalidOperationException
        Assert.Throws<InvalidOperationException>(() => correspondencia.Aceitar());
    }

    // ---------- Caminho feliz ----------
    [Fact]
    public void Cancelar_QuandoSugerida_MudaStatusParaCancelada()
    {
        // Arrange — nasce "Sugerida"
        var correspondencia = new Correspondencia();

        // Act
        correspondencia.Cancelar();

        // Assert — só o status muda (o método não registra data de cancelamento)
        Assert.Equal(StatusCorrespondencia.Cancelada, correspondencia.Status);
    }

    // ---------- Caminho de erro ----------
    [Fact]
    public void Cancelar_QuandoConcluida_LancaExcecao()
    {
        // Arrange — precisamos levar a correspondência até "Concluida".
        // Isso exige: ter uma Solicitacao em andamento, aceitar e concluir.
        var solicitacao = new SolicitacaoApoio();
        solicitacao.MarcarEmAndamento(); // Aberta -> EmAndamento (pré-requisito do Concluir)

        var correspondencia = new Correspondencia { Solicitacao = solicitacao };
        correspondencia.Aceitar();   // Sugerida -> AceitaPeloVoluntario
        correspondencia.Concluir();  // AceitaPeloVoluntario -> Concluida

        // Act + Assert — agora sim, cancelar uma Concluida deve lançar
        Assert.Throws<InvalidOperationException>(() => correspondencia.Cancelar());
    }

    // ---------- Caminho feliz ----------
    [Fact]
    public void Concluir_QuandoAceita_MudaStatusParaConcluida()
    {
        // Arrange - Precisamos de uma correspondência que esteja "EmAndamento"
        var solicitacao = new SolicitacaoApoio();
        solicitacao.MarcarEmAndamento();

        var correspondencia = new Correspondencia { Solicitacao = solicitacao};
        correspondencia.Aceitar(); // Sugerida -> AceitaPeloVoluntario
        correspondencia.Concluir(); // AceitaPeloVoluntario -> Concluida

        Assert.Equal(StatusCorrespondencia.Concluida, correspondencia.Status);
    }

    // ---------- Caminho de erro 1 ----------
    [Fact]
    public void Concluir_QuandoNaoAceita_LancaExcecao()
    {
        // Arrange -- uma correspondência que ainda está "Sugerida"
        var correspondencia = new Correspondencia();

        // Act + Assert -- tentar concluir sem aceitar deve lançar
        Assert.Throws<InvalidOperationException>(() => correspondencia.Concluir());
    }

    // ---------- Caminho de erro 2 ----------
    [Fact]
    public void Concluir_QuandoSolicitacaoNaoEstaEmAndamento_LancaExcecao()
    {
        // Arrange -- uma correspondência que está "EmAndamento" mas cuja solicitação não está "EmAndamento"
        var solicitacao = new SolicitacaoApoio();
        var correspondencia = new Correspondencia { Solicitacao = solicitacao };
        correspondencia.Aceitar(); // Sugerida -> AceitaPeloVoluntario

        // Act + Assert -- tentar concluir deve lançar, pois a solicitação não está "EmAndamento"
        Assert.Throws<InvalidOperationException>(() => correspondencia.Concluir());
    }

    // ---------- Comportamento documentado: concluir sem Solicitacao carregada ----------
    // ATENÇÃO (ponto de design a revisar com o squad): hoje, se a Solicitacao não
    // estiver carregada (null), o Concluir() da correspondência NÃO sincroniza a
    // solicitação (o `Solicitacao?.Concluir()` é pulado), mas a correspondência
    // vira Concluida mesmo assim. Este teste documenta esse comportamento atual.
    [Fact]
    public void Concluir_QuandoAceitaSemSolicitacaoCarregada_ConcluiCorrespondencia()
    {
        // Arrange — correspondência aceita, mas SEM Solicitacao (null)
        var correspondencia = new Correspondencia();
        correspondencia.Aceitar();  // Sugerida -> AceitaPeloVoluntario

        // Act — conclui sem Solicitacao carregada (o `?.` pula a chamada à solicitação)
        correspondencia.Concluir();

        // Assert — a correspondência conclui mesmo sem sincronizar a solicitação
        Assert.Equal(StatusCorrespondencia.Concluida, correspondencia.Status);
    }

    // ---------- Caminho Feliz ----------
    [Fact]
    public void RevelarIdentidade_QuandoAceita_MudaStatusParaRevelada()
    {
        // Arrange - Uma correspondência aceita
        var correspondencia = new Correspondencia();
        correspondencia.Aceitar(); // Sugerida -> AceitaPeloVoluntario

        // Act - Revela a identidade
        correspondencia.AutorizarRevelacaoIdentidade();

        // Assert - O status muda para Revelada
        Assert.True(correspondencia.IdentidadeRevelada);
        Assert.NotNull(correspondencia.IdentidadeReveladaEm);
    }

    // ---------- Caminho de erro ----------
    [Fact]
    public void RevelarIdentidade_QuandoNaoAceita_LancaExcecao()
    {
        // Arrange - Uma correspondência que ainda está Sugerida
        var correspondencia = new Correspondencia();

        // Act + Assert - Tentar revelar Identidade sem aceitar deve lançar execeção
        Assert.Throws<InvalidOperationException>(() => correspondencia.AutorizarRevelacaoIdentidade());
    }

}
