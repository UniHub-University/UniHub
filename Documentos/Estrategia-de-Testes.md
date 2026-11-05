# Estratégia de Testes — UniHub

> Documento de decisões. Explica **como** testamos o UniHub e, principalmente,
> **por que** testamos desse jeito. Serve de referência para quem for escrever
> um teste novo e de registro das escolhas de engenharia feitas pelo time.

## 1. Objetivo e princípio

O UniHub é um marketplace acadêmico com regras de negócio sensíveis a corridas
(estoque, reserva, expiração) e a um módulo com implicações de LGPD (Ação
Solidária). A estratégia de testes foi desenhada em torno de uma pergunta:

> *"Onde um bug causaria dano real — venda além do estoque, reserva que não
> expira, identidade revelada cedo demais?"*

Testamos **com profundidade onde o custo do erro é alto** e com leveza onde o
código é trivial. Não perseguimos cobertura por cobertura: perseguimos confiança
nas regras que sustentam o produto.

## 2. Ferramentas (o que usamos e por quê)

| Ferramenta | Versão | Papel |
|---|---|---|
| **xUnit** | 2.9.x | Framework de teste. Padrão de fato no ecossistema .NET moderno. |
| **NSubstitute** | 6.2.x | Mocks/fakes para isolar a camada de Application das dependências (repositórios). Sintaxe mais limpa que Moq para o nosso uso. |
| **Microsoft.NET.Test.Sdk** | 17.14.x | Runner/host dos testes. |
| **coverlet.collector** | 6.0.x | Coleta de cobertura (quando quisermos medir). |
| **Npgsql / EF Core** | 10.0.12 | Testes de integração batem num Postgres real (Neon), não em InMemory. |

**Decisão — EF Core fixado em 10.0.12 no projeto de teste:** alinhamos a versão
do EF com a do Backend explicitamente no `.csproj` para eliminar o conflito
`MSB3277` (versões divergentes da mesma dependência). Está comentado no csproj.

**Decisão — não usamos o provider InMemory do EF.** Ele não reproduz o
comportamento transacional nem o `ExecuteUpdateAsync` condicional do Postgres —
justamente o mecanismo que precisamos provar. Integração roda contra Postgres de
verdade (ver seção 5).

## 3. A pirâmide de testes do projeto

```
        ┌─────────────────────────┐
        │  Integração (Postgres)  │  poucos, caros, alto valor
        ├─────────────────────────┤
        │   Application (service) │  muitos, rápidos, com mocks
        ├─────────────────────────┤
        │     Domínio (entidade)  │  base: regras puras, sem I/O
        └─────────────────────────┘
```

Organização física (`Tests/UniHub.Tests/`):
- `Domain/` — testes de entidade pura (sem banco, sem mock).
- `Application/` — testes de service com repositórios mockados (NSubstitute).
- `Integracao/` — testes que tocam o Postgres real.

### 3.1 Testes de domínio — regras puras
Testam o comportamento das entidades isoladamente: construção, invariantes e
transições de estado. Sem I/O, sem mock, instantâneos.

```csharp
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
    public void RetomarAtuacao_DeixaDisponivelNovamente()
    {
        var voluntario = new Voluntario();
        voluntario.PausarAtuacao();
        voluntario.RetomarAtuacao();
        Assert.True(voluntario.Disponivel);
    }
}
```

**Decisão — regra de negócio mora na entidade, e é lá que o teste a prende.**
Métodos como `Pedido.Cancelar()`, `Pedido.Expirar()`, `Voluntario.PausarAtuacao()`
validam o próprio estado (ex.: só cancela se `Tipo == Reserva && Status ==
Reservado`). Testar a entidade garante que a invariante vale independentemente de
quem a chame.

### 3.2 Testes de Application (service) — orquestração com mocks
Testam a lógica de orquestração do service isolando os repositórios com
NSubstitute. Rápidos e determinísticos.

```csharp
[Trait("Categoria", "Unit")]
public class PedidoServiceExpiracaoTests
{
    private readonly IProdutoRepository _produtoRepo = Substitute.For<IProdutoRepository>();
    private readonly IPedidoRepository _pedidoRepo  = Substitute.For<IPedidoRepository>();

    [Fact]
    public async Task ExpirarReservasVencidas_QuandoHaReservaVencida_ExpiraEDevolveEstoque()
    {
        var produtoId = Guid.NewGuid();
        var reserva = ReservaVencida((produtoId, 3));
        _pedidoRepo.GetReservasVencidasAsync(Arg.Any<DateTime>())
                   .Returns(new List<Pedido> { reserva });
        _pedidoRepo.TryExpirarAsync(reserva.Id).Returns(true); // "ganhou" a corrida

        var expiradas = await CriarService().ExpirarReservasVencidasAsync();

        Assert.Equal(1, expiradas);
        await _produtoRepo.Received(1).IncrementarEstoqueAsync(produtoId, 3);
    }
}
```

**Decisão — padrão de concorrência testado pelo valor de retorno do `Try*`.**
Os métodos de repositório que resolvem corridas (`TryConcluirAsync`,
`TryExpirarAsync`, `TryCancelarAsync`) usam `ExecuteUpdateAsync` com WHERE
condicional e retornam **se a linha foi afetada** (`true` = esta chamada ganhou a
corrida). O service só devolve estoque quando ganhou. Mockamos esse retorno para
testar os dois caminhos:
- `Try* == true` → devolve estoque exatamente uma vez (`Received(1)`);
- `Try* == false` → **não** devolve estoque (idempotência: outro já tratou).

É assim que provamos, em testes rápidos, que não há dupla-devolução de estoque.

## 4. Convenções de escrita

- **Nome do teste:** `Metodo_Cenario_ResultadoEsperado`
  (ex.: `ExpirarReservasVencidas_QuandoOutroJaExpirou_NaoDevolveEstoque`).
  O nome conta a história; a leitura da lista de testes vira a especificação.
- **Estrutura Arrange / Act / Assert**, com comentários quando o *porquê* não é óbvio.
- **Factories locais** (ex.: `ReservaVencida(...)`) para montar cenários sem
  poluir cada teste com setup repetido.
- **Asserções específicas:** preferir `Received(1)` / valores exatos a asserts
  genéricos — um teste deve falhar pelo motivo certo.

## 5. Testes de integração — Postgres real, não InMemory

Os testes de integração batem na **branch `development` do Neon** (Postgres
gerenciado), com a connection string vinda da env var `UNIHUB_TEST_DB`.

**Decisão — padrão transação + rollback.** Cada teste abre uma transação e faz
**ROLLBACK** no fim. Nada do que o teste escreve fica persistido: o banco
permanece limpo e os testes são repetíveis. É o que a `IntegracaoTestBase`
fornece via `CriarContexto()`.

```csharp
public abstract class IntegracaoTestBase
{
    protected AppDbContext CriarContexto()
    {
        var connString = Environment.GetEnvironmentVariable("UNIHUB_TEST_DB")
            ?? throw new InvalidOperationException(
                "Defina UNIHUB_TEST_DB com a connection string da branch development do Neon.");

        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(connString)
            .Options;

        return new AppDbContext(opcoes);
    }
}
```

**Por que Postgres de verdade:** só ele reproduz o `ExecuteUpdateAsync`
condicional, o comportamento transacional e os tipos reais. É exatamente o
mecanismo crítico do projeto — testá-lo em InMemory seria testar outra coisa.

### 5.1 A exceção consciente: teste de concorrência

O teste do requisito técnico central — **nunca vender além do estoque sob
concorrência** — é a única exceção ao padrão de rollback, e é uma exceção
deliberada.

```csharp
// Estoque = 1, 10 compradores simultâneos → esperado: 1 sucesso, 9 falhas, estoque final 0.
var tarefas = Enumerable.Range(0, compradores).Select(async _ =>
{
    await using var ctx = CriarContexto();          // conexão independente por Task
    var repo = new ProdutoRepository(ctx);
    return await repo.TryDecrementarEstoqueAsync(produtoId, 1);
});
bool[] resultados = await Task.WhenAll(tarefas);
Assert.Equal(sucessosEsperados, resultados.Count(r => r));
```

Concorrência de verdade exige **conexões separadas rodando em paralelo** (cada
Task com seu próprio `AppDbContext`) e **COMMIT real** — então não dá para usar
transação+rollback, porque:
- um único `DbContext` serializaria as operações no cliente (sem disputa real);
- transações separadas não enxergariam o produto criado por outra;
- o UPDATE precisa commitar para a corrida existir.

**Consequência (registrada):** esse teste **grava de verdade** no banco de
development e faz a **própria limpeza no `finally`** (IDs únicos criados no seed,
tudo apagado no fim). É inevitável para provar atomicidade. Toda essa justificativa
está documentada no cabeçalho do próprio arquivo
(`ConcorrenciaEstoqueIntegracaoTests.cs`), para que ninguém "conserte" o teste
tentando aplicar o padrão de rollback.

## 6. Categorias e execução no CI

Todo teste recebe **uma** categoria via `[Trait("Categoria", "...")]`:

| Categoria | O que é | Roda no CI? |
|---|---|---|
| `Unit` | Domínio + Application (sem I/O, com mocks) | ✓ sim |
| `Integracao` | Toca o Postgres (Neon) | ✗ não — roda localmente |

**Decisão — o CI roda só `Categoria=Unit`.** Os testes de integração dependem de
uma connection string de banco gerenciado (`UNIHUB_TEST_DB`) que não expomos no
pipeline público. Eles são executados **localmente** por quem desenvolve a feature.

Comandos:

```bash
# CI (e desenvolvimento rápido): só unitários
dotnet test Tests/UniHub.Tests/UniHub.Tests.csproj --filter "Categoria=Unit"

# Localmente, com o banco configurado: só integração
export UNIHUB_TEST_DB="<connection string da branch development do Neon>"
dotnet test Tests/UniHub.Tests/UniHub.Tests.csproj --filter "Categoria=Integracao"
```

O pipeline (`.github/workflows/ci.yml`) tem três jobs independentes:
1. **Build (.NET 10)** — restaura, compila em Release e **verifica migrations
   pendentes** (`dotnet ef migrations has-pending-model-changes`): se o modelo
   mudou sem migration, o build falha.
2. **Testes unitários** — `dotnet test --filter "Categoria=Unit"`.
3. **Build da imagem Docker** — garante que o `Dockerfile` do Backend sobe.

## 7. O que testamos (e o que deliberadamente não testamos)

**Cobrimos com prioridade:**
- Regras de transição de estado do `Pedido` (reservar, confirmar, concluir,
  cancelar, expirar) — domínio + service.
- Devolução de estoque idempotente nas corridas (`Try*` ganha/perde).
- Atomicidade do decremento de estoque sob concorrência real (integração).
- Regras do módulo de voluntário (pausar/retomar) e de Ação Solidária
  (correspondência, doação, solicitação) no domínio.
- Validadores de domínio (ex.: `ChavePixValidator`).

**Deliberadamente fora do escopo de testes automatizados (por ora):**
- **Frontend (React):** validação é manual/exploratória nesta fase do projeto. O
  custo de montar E2E não se paga antes da entrega final. Fica como melhoria futura.
- **Controllers (HTTP):** a lógica mora nos services (testados); os controllers
  são finos. Não escrevemos testes de controller isolados para não testar o framework.
- **Mapeamentos triviais de DTO:** não testamos getters/setters nem mapeamentos
  sem lógica.

Essa fronteira é uma escolha de custo/benefício consciente, não uma lacuna
esquecida — e está registrada aqui justamente para ser revisada quando o contexto
mudar (ex.: pré-entrega final, vale avaliar um smoke E2E do fluxo de compra).

## 8. Como adicionar um teste novo (checklist)

1. Escolha a camada: regra pura → `Domain/`; orquestração → `Application/`
   (mocke os repositórios); precisa do banco → `Integracao/`.
2. Marque `[Trait("Categoria", "Unit")]` ou `[Trait("Categoria", "Integracao")]`.
3. Nomeie no padrão `Metodo_Cenario_ResultadoEsperado`.
4. Se for integração comum, herde de `IntegracaoTestBase` (transação+rollback).
   **Só** fuja desse padrão se precisar de concorrência real — e documente o porquê
   no cabeçalho do arquivo, como no teste de estoque.
5. Rode `--filter "Categoria=Unit"` antes de abrir o PR; se mexeu em integração,
   rode-a localmente com `UNIHUB_TEST_DB` setada.
