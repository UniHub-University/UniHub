using UniHub.Domain.Entities;
using Xunit;

namespace UniHub.Tests.Domain;

[Trait("Categoria", "Unit")]
public class VoluntarioTests
{
    [Fact]
    public void Voluntario_QuandoCriado_NasceDisponivel()
    {
        var voluntario = new Voluntario();
        Assert.True(voluntario.Disponivel);
    }

    [Fact]
    public void PausarAtuacao_DeixaIndisponivel()
    {
        var voluntario = new Voluntario();
        voluntario.PausarAtuacao();
        Assert.False(voluntario.Disponivel);
    }

    [Fact]
    public void RetomarAtuacao_DeixaDisponivelNovamente()
    {
        var voluntario = new Voluntario();
        voluntario.PausarAtuacao();   // primeiro pausa
        voluntario.RetomarAtuacao();  // depois retoma
        Assert.True(voluntario.Disponivel);
    }
}
