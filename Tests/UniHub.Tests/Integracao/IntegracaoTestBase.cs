using System;
using Microsoft.EntityFrameworkCore;
using UniHub.Infrastructure.Data;

namespace UniHub.Tests.Integracao;

/// <summary>
/// Base para testes de integração. Cada teste cria um AppDbContext conectado
/// à branch development do Neon (connection string lida da env var UNIHUB_TEST_DB).
///
/// IMPORTANTE: os testes abrem uma transação e fazem ROLLBACK no fim, então
/// nada do que o teste escreve fica persistido no banco — ele continua limpo
/// e os testes são repetíveis.
/// </summary>
public abstract class IntegracaoTestBase
{
    protected AppDbContext CriarContexto()
    {
        var connString = Environment.GetEnvironmentVariable("UNIHUB_TEST_DB")
            ?? throw new InvalidOperationException(
                "Defina a env var UNIHUB_TEST_DB com a connection string da branch development do Neon.");

        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connString)
            .Options;

        return new AppDbContext(opcoes);
    }
}
