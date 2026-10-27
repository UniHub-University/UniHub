using System;
using System.Text.RegularExpressions;

namespace UniHub.Application.Validacoes;

/// <summary>
/// Valida o formato de uma chave PIX. Aceita os 5 formatos oficiais:
/// CPF, CNPJ, telefone (E.164), e-mail e chave aleatória (EVP/UUID).
/// Extraído de VendedorService para ficar testável de forma isolada.
/// </summary>
public static class ChavePixValidator
{
    public static bool EhValida(string? chavePix)
    {
        if (string.IsNullOrWhiteSpace(chavePix)) return false;

        // Limpa a chave de possíveis formatações (pontos, traços, barras, parênteses).
        // Mantém letras, números, @, + e - (úteis para email, telefone e EVP).
        var chaveLimpa = Regex.Replace(chavePix, @"[^\w\-\.@+]", "");

        // 1. CPF (11 dígitos numéricos)
        if (Regex.IsMatch(chaveLimpa, @"^\d{11}$")) return true;

        // 2. CNPJ (14 dígitos numéricos)
        if (Regex.IsMatch(chaveLimpa, @"^\d{14}$")) return true;

        // 3. Telefone (E.164: começa opcionalmente com + seguido de 10 a 14 dígitos)
        if (Regex.IsMatch(chaveLimpa, @"^\+?[1-9]\d{9,13}$")) return true;

        // 4. E-mail (validação básica de formato)
        if (Regex.IsMatch(chaveLimpa, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) return true;

        // 5. Chave Aleatória / EVP (formato UUID)
        if (Guid.TryParse(chavePix, out _)) return true;

        return false;
    }
}
