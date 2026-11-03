# UniHub — Backend

API REST do UniHub: o ecossistema digital que centraliza comércio (Alimentação e
Bazar Acadêmico) e ação social (Ação Solidária) dentro do campus da UNIFESP,
restrito a usuários autenticados com e-mail institucional (`@unifesp.br`).

> Para as convenções de código, fluxo de PRs, migrations e lições aprendidas, leia
> o [`AGENTS.md`](../AGENTS.md) na raiz — é a fonte de verdade do time.

## Como o backend funciona

API em **.NET 10** (ASP.NET Core), organizada em camadas (Clean Architecture
simplificada) para separar regra de negócio de detalhes de infraestrutura:

```
Domain          → entidades puras (Produto, Usuario, Pedido, Correspondencia, ...)
Application     → regras de negócio (Services), interfaces de repositório, DTOs
Infrastructure  → EF Core, AppDbContext, repositórios, Cloudinary
API             → Controllers, autenticação JWT, BackgroundServices, composição (DI)
```

Cada camada só conhece a de baixo (API → Application → Infrastructure → Domain),
nunca o contrário.

### Autenticação

Todo acesso exige um usuário autenticado com e-mail institucional. O login via
Google OAuth valida o domínio e a API emite um **JWT**, validado em cada requisição.
Endpoints marcados com `[Authorize]` rejeitam chamadas sem token válido.

> ⚠️ O usuário autenticado é SEMPRE extraído do claim `"sub"`
> (`User.FindFirst("sub")`), nunca de `ClaimTypes.NameIdentifier` — o projeto usa
> `MapInboundClaims = false`. Ver `AGENTS.md` §4.

### O maior desafio técnico: concorrência no estoque

Durante o intervalo, dezenas de alunos podem tentar comprar o mesmo item ao mesmo
tempo. Ler o estoque, verificar em C# e salvar depois criaria uma janela em que dois
pedidos "veem" a mesma última unidade.

A solução **não** usa locks em memória (que não funcionam com múltiplas instâncias
atrás de um load balancer). O decremento é um **UPDATE condicional atômico** no banco:

```sql
UPDATE "Produtos"
SET "QuantidadeDisponivel" = "QuantidadeDisponivel" - @quantidade
WHERE "Id" = @id AND "QuantidadeDisponivel" >= @quantidade
```

Se duas requisições disputam a última unidade, apenas uma altera uma linha; a outra
recebe zero linhas afetadas e sabe, de forma confiável, que perdeu a corrida. O mesmo
padrão rege o aceite de solicitações de apoio (`WHERE Status = Aberta`), a baixa de
pedido pelo vendedor (`WHERE Status = Reservado`) e a expiração de reservas. Em todos,
o efeito colateral (ex.: devolver estoque) só é aplicado por quem ganha a corrida.

### Módulos

- **Alimentação:** vendedores publicam alimentos com estoque; alunos compram com
  garantia de consistência mesmo sob alta concorrência. Exclusão de alimento é lógica
  (preserva histórico).
- **Bazar Acadêmico:** marketplace de compra/venda entre alunos (livros, calculadoras,
  equipamentos), reaproveitando a modelagem de produto/estoque. Suporta **compra** e
  **reserva temporária** (TTL de 48h) que expira e devolve o estoque automaticamente —
  via `BackgroundService` + checagem *lazy* nos acessos.
- **Ação Solidária:** solicitações são pseudonimizadas por um `CodigoPublico`. A
  identidade real do solicitante só é revelada após autorização explícita
  (`Correspondencia.AutorizarRevelacaoIdentidade()`), nunca no fluxo de listagem/match.
- **Moderação:** denúncias de usuários/produtos, gerenciadas pelo Admin.

## Banco de dados: Neon (Postgres serverless)

O projeto usa **Neon** (Postgres serverless gerenciado), com EF Core 10 + provider
**Npgsql**. Motivos:

- **Scale-to-zero:** o banco "dorme" quando ocioso e volta em segundos — sem custo de
  compute parado.
- **Branching instantâneo:** cada squad/integrante cria uma branch isolada do banco
  para testar sem afetar os demais (é o que os testes de integração usam, via a env
  var `UNIHUB_TEST_DB`).
- **Postgres padrão:** sem vendor lock-in.

**Ao usar:**
- Use sempre a *connection string com pooling* e **SSL** do Neon.
- O primeiro acesso após ociosidade tem latência extra (cold start) — aquecer o banco
  com uma query simples antes de medir concorrência.

### Migrations

> ⚠️ Centralizadas pelo **Guardião de Migrations** (um por sprint). Não rode
> `dotnet ef migrations add` por conta própria. Detalhes em `AGENTS.md` §6.

- Toda mudança de entidade/`AppDbContext` que afete o schema gera migration.
- Testar sempre em uma branch do Neon antes da principal.
- FKs usam `DeleteBehavior.Restrict` — prefira **exclusão lógica** (mudar `Status`) a
  `DELETE` físico em entidades com dependentes.
- A API **não** aplica migrations no startup (aplicação é manual).

## Segurança e configuração

- **Segredos nunca versionados.** Localmente: `dotnet user-secrets`. Em produção:
  variáveis de ambiente no Render (formato `Secao__Chave`).
- Segredos: connection string do Neon, `JwtSettings:SecureKey`, credenciais Cloudinary.
- `EnableSensitiveDataLogging` só em Development (LGPD).

## Estrutura de pastas

```
Backend/
├── Program.cs                       # composição, DI, auth, CORS, hosted services
├── Dockerfile                       # imagem de produção (deploy no Render)
├── Migrations/                      # migrations do EF Core (Guardião)
└── src/
    ├── UniHub.Domain/Entities/      # entidades e enums
    ├── UniHub.Application/          # Services, Interfaces, DTOs (por módulo)
    ├── UniHub.Infrastructure/       # AppDbContext, repositórios, Cloudinary
    └── Unihub.API/
        ├── Controllers/             # endpoints REST
        └── BackgroundServices/      # jobs (ex.: expiração de reservas)
```

## Como rodar localmente

```bash
cd Backend

# 1. Configurar os segredos (uma vez)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<neon-com-pooling-e-ssl>"
dotnet user-secrets set "JwtSettings:SecureKey" "<chave-32+-caracteres>"
dotnet user-secrets set "Cloudinary:CloudName" "<cloud-name>"
dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>"
dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>"

# 2. Restaurar, aplicar migrations e rodar
dotnet restore
dotnet ef database update   # idealmente só o Guardião de Migrations
dotnet run
```

Em Development, o Swagger fica na URL informada pelo `dotnet run`
(ex.: `http://localhost:5206/swagger`).

## Testes

```bash
# unidade (rápido, sem banco — é o que o CI roda)
dotnet test --filter "Categoria=Unit"

# integração/concorrência (exige a env var UNIHUB_TEST_DB apontando para uma
# branch de teste do Neon; cada teste roda em transação + ROLLBACK)
dotnet test --filter "Categoria=Integracao"
```

A suíte cobre domínio, services (com NSubstitute), integração e o teste de
concorrência de estoque (o diferencial técnico do projeto).

## Deploy

Deploy no **Render** a partir do `Backend/Dockerfile`, banco no **Neon**. Endpoint
`/health` para health check e warm-up do cold start. Runbook completo em
[`../DEPLOY.md`](../DEPLOY.md).
