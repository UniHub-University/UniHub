using UniHub.Application.Validacoes;
using Xunit;

namespace UniHub.Tests.Validacoes;

[Trait("Categoria", "Unit")]
public class ChavePixValidatorTests
{
    // ---------- Chaves VÁLIDAS (um teste, vários dados) ----------
    [Theory]
    [InlineData("12345678901")]                               // CPF (11 dígitos)
    [InlineData("12345678000199")]                            // CNPJ (14 dígitos)
    [InlineData("+5511999998888")]                            // Telefone E.164
    [InlineData("11999998888")]                               // Telefone sem +
    [InlineData("vendedor@unifesp.br")]                       // E-mail
    [InlineData("a1b2c3d4-e5f6-7890-abcd-ef1234567890")]      // Chave aleatória (UUID)
    public void EhValida_QuandoFormatoReconhecido_RetornaTrue(string chave)
    {
        Assert.True(ChavePixValidator.EhValida(chave));
    }

    // ---------- Chaves INVÁLIDAS ----------
    [Theory]
    [InlineData("")]                 // vazio
    [InlineData("   ")]              // só espaços
    [InlineData(null)]               // null
    [InlineData("abc")]              // texto curto sem formato
    [InlineData("123")]              // poucos dígitos (não é CPF/CNPJ/telefone)
    [InlineData("email-sem-arroba")] // não é e-mail válido
    public void EhValida_QuandoFormatoInvalido_RetornaFalse(string? chave)
    {
        Assert.False(ChavePixValidator.EhValida(chave));
    }
}
