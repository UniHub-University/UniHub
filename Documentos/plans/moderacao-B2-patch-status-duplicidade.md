# Plano — Moderação B2 (Pessoa B): transição de status + duplicidade

## Objetivo
Entregar a parte da Pessoa B do ciclo de moderação (issue #114/B2): método de
domínio para transição de status da denúncia, o `PATCH` de status (só Admin), a
checagem de denúncia duplicada na criação, e os testes unitários das transições.

> A parte da Pessoa A (GET admin paginado, `AdminDenunciaDto`, `ListarParaAdminAsync`,
> `DenunciasAdminController`) **já foi mergeada na main** por outro colega (PR #119).
> Este plano cobre só a Pessoa B.

## Escopo
- **Inclui:**
  - `Denuncia.AtualizarStatus(DenunciaStatus)` + `Status` com setter privado (domínio).
  - DTO `AtualizarStatusDenunciaDto`.
  - `PATCH /api/admin/denuncias/{id}/status` (role Admin).
  - `ExisteDenunciaEmAndamentoAsync` no repositório + checagem no `DenunciasController` (409).
  - Método para obter denúncia rastreada (para persistir o PATCH).
  - Testes unitários de transição em `Tests/UniHub.Tests/Domain/DenunciaTests.cs`.
- **NÃO inclui:**
  - Migration (a issue pede sem migration; só coordenar com o guardião se precisar).
  - Reescrever a parte da Pessoa A.

## Regras acordadas (do documento)
- Status: Aberta=0, EmAnalise=1, Resolvida=2, Rejeitada=3.
- `Aberta` e `EmAnalise` → podem ir para qualquer status válido.
- `Resolvida` e `Rejeitada` são finais → qualquer tentativa de mudança lança
  `InvalidOperationException` (vira 409 no PATCH).
- Status fora do enum → 400.
- Duplicidade: bloquear nova denúncia se já existe uma do mesmo denunciante, mesmo
  denunciado e mesmo `ProdutoId` (comparação por igualdade, incluindo nulo) em estado
  `Aberta` ou `EmAnalise`. Duplicata → 409. `EmAnalise` conta como ativa.

## Decisões de design
- **Domínio:** `Status` passa a ter `private set` (EF Core aceita). `AtualizarStatus`
  valida `Enum.IsDefined` e a regra de estados finais.
- **Onde fica o PATCH:** o `AdminController` já existe com `[Route("api/admin")]` e
  `[Authorize(Roles="Admin")]`. A rota exigida é `api/admin/denuncias/{id}/status`.
  Decisão: adicionar a action `[HttpPatch("denuncias/{id:guid}/status")]` no
  `AdminController` existente (a rota base já é `api/admin`). Precisa injetar o
  `IDenunciaRepository` nesse controller. (Alternativa seria um controller novo; fico
  no existente por já ter a role configurada, e aviso o colega.)
- **Repositório:** adicionar `ExisteDenunciaEmAndamentoAsync` (AnyAsync) e
  `ObterRastreadaAsync` (busca a entidade tracked para o SaveChanges do PATCH).
  Preservar o `ListarParaAdminAsync` e o `CriarAsync` que já existem.
- **Serialização:** enums como número (padrão do projeto). PATCH recebe `{ "status": 2 }`.
- **Concorrência:** `AnyAsync` é a proteção pedida; unicidade rígida exigiria
  constraint/migration, que NÃO entra aqui (coordenar com o guardião depois).

## Passos
1. Domínio: `Denuncia.cs` — setter privado + `AtualizarStatus`.
2. DTO: `AtualizarStatusDenunciaDto.cs`.
3. Interface: `IDenunciaRepository` — `ExisteDenunciaEmAndamentoAsync`, `ObterRastreadaAsync`.
4. Repositório: `DenunciaRepository` — implementar os dois (sem remover nada).
5. API: `AdminController` — PATCH de status. `DenunciasController` — checagem de duplicata.
6. Testes: `DenunciaTests.cs` — transições permitidas e recusadas.
7. Validar: build + testes unitários.

## Critérios de aceite
- [ ] `AtualizarStatus`: Aberta/EmAnalise → qualquer válido; final → InvalidOperationException.
- [ ] Status fora do enum → `ArgumentException`/400.
- [ ] `PATCH /api/admin/denuncias/{id}/status` exige Admin; 204/404/400/409.
- [ ] Duplicata (mesmo denunciante+denunciado+ProdutoId, Aberta/EmAnalise) → 409.
- [ ] Testes de transição passam; `dotnet build` e suíte unitária limpos.

## Pendências
- Avisar a Pessoa A (mppaschoalotto) que o PATCH entra no `AdminController` e que a
  interface/repositório de denúncia ganham métodos novos (consolidar sem apagar nada).
- Número da issue no board (#114 citado no doc).
