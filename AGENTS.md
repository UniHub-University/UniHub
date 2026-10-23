# AGENTS.md — Guia para Assistentes de IA e Desenvolvedores

Este documento define como qualquer agente de IA (ou desenvolvedor) deve atuar no
repositório **UniHub**. Ele consolida as convenções, a arquitetura e as lições
aprendidas ao longo das sprints. **Leia antes de escrever qualquer código ou abrir
um PR.**

---

## 0. Princípios fundamentais (ler antes de tudo)

Estes princípios reduzem os erros mais comuns de IA e de desenvolvimento neste
projeto. Aplique-os em toda tarefa.

### 0.1. Pense antes de codar
Não presuma. Não esconda confusão. Exponha os trade-offs.
- Declare suposições explicitamente. Se estiver incerto, **pergunte**.
- Se há mais de uma interpretação, apresente-as — **não escolha em silêncio**.
- Se existe uma abordagem mais simples, diga. Questione quando fizer sentido.
- Se algo está confuso, **pare**. Aponte o que não está claro e pergunte.

### 0.2. Simplicidade primeiro
O mínimo de código que resolve o problema. Nada especulativo.
- Nenhuma funcionalidade além do que foi pedido.
- Prefira reaproveitar os padrões que já existem no projeto a inventar novos.
- Não adicione abstração "para o futuro" sem necessidade concreta.

### 0.3. Valide antes de entregar
Neste projeto, erros recorrentes só aparecem em build/runtime — não na leitura.
- **Sempre** rode `dotnet build` antes de abrir ou atualizar um PR.
- Confirme que os arquivos citados foram realmente commitados.
- Antes de dizer "pronto", verifique contra o que foi de fato pedido.

### 0.4. Respeite o que já funciona
- Não "padronize" ou refatore código fora do escopo da tarefa sem alinhar
  (ver Débitos técnicos conhecidos — seção 12).
- Mudanças em `AppDbContext`, `Program.cs` e migrations são sensíveis:
  coordene (ver seções 6 e 9) antes de agir.

---

## 0.5. Fluxo obrigatório: planejar antes de implementar

Antes de escrever código para qualquer task/user story, crie e refine um
**plano em markdown**. Isto não é opcional — um plano ruim gera retrabalho caro;
um plano revisado é barato de corrigir.

### Passo a passo
1. **Crie o arquivo** `Documentos/plans/<id-ou-nome-da-task>.md` (ex.:
   `Documentos/plans/BRV-128-campanhas-sazonais.md`), usando o template abaixo.
2. **Refine o plano em conversa** com a IA (ou com o squad) até ficar claro:
   questione suposições, levante trade-offs, confirme o escopo. **Não escolha
   em silêncio** (ver princípio 0.1).
3. **Só então implemente**, seguindo o plano aprovado.
4. **Mantenha o `.md` atualizado** se o plano mudar durante a implementação.
5. Ao concluir, o `.md` fica arquivado em `Documentos/plans/` como registro da decisão
   (útil se outra pessoa/IA assumir a task depois).

### Quando aplicar
- **Sempre** para features e user stories (qualquer coisa com lógica de negócio nova).
- **Pode pular** para correções triviais (typo, ajuste de texto, bump de versão) —
  bom senso.

### Template do plano (`Documentos/plans/<task>.md`)

```markdown
# Plano — <ID/Nome da task>

## Objetivo
<O que esta task entrega e por quê. 1-3 frases.>

## Escopo
- **Inclui:** <o que será feito>
- **NÃO inclui:** <o que fica de fora, para evitar escopo inchado>

## Decisões de design
- **Entidades/Domain:** <entidades novas ou alteradas>
- **Endpoints/API:** <rotas novas, verbos, contratos>
- **Banco/Migrations:** <muda schema? precisa de migration? coordenar com o Guardião>
- **Impacto em concorrência/LGPD:** <toca estoque? expõe dados sensíveis?>
- **Trade-offs considerados:** <alternativas avaliadas e por que a escolhida>

## Passos de implementação
1. <camada Domain>
2. <camada Application: interface, service, DTOs>
3. <camada Infrastructure: repositório>
4. <camada API: controller, DI no Program.cs>
5. <frontend, se aplicável>

## Critérios de aceite
- [ ] <condição 1 — comportamento verificável>
- [ ] <condição 2>
- [ ] `dotnet build` limpo
- [ ] (se mexe em schema) migration testada em branch do Neon

## Pendências / dúvidas em aberto
<Perguntas que precisam de decisão humana antes/durante a implementação.>
```

> Este fluxo complementa o planejamento de sprint (board/YouTrack): o board diz
> **o que** fazer e quando; o `.md` de plano diz **como** fazer aquela task
> específica, com o nível de detalhe técnico que a implementação exige.

---

## 1. O que é o projeto

UniHub é um ecossistema digital universitário (UNIFESP) com três módulos:
**Alimentação**, **Bazar Acadêmico** e **Ação Solidária**, além de Moderação.
Acesso restrito a e-mails `@unifesp.br`. Trabalho acadêmico de Engenharia de
Software, equipe de ~7 pessoas, Scrum com sprints de 2 semanas.

## 2. Stack

- **Backend:** C# / **.NET 10** (ASP.NET Core Web API)
- **ORM:** EF Core 10 com provider **Npgsql** (`Npgsql.EntityFrameworkCore.PostgreSQL`)
- **Banco:** **Neon** (Postgres serverless) — decisão final, NÃO migrar para RDS
- **Auth:** Google OAuth (login institucional) + JWT emitido pela API
- **Imagens:** Cloudinary
- **Docs de API:** Swagger/Swashbuckle
- **Frontend:** React + Vite
- **Deploy:** Render (container Docker), HTTPS automático em `*.onrender.com`

## 3. Arquitetura (Clean Architecture simplificada)

Projeto único, camadas separadas por pastas. Cada camada só conhece a de baixo:

```
API  →  Application  →  Infrastructure  →  Domain
```

- **Domain** (`UniHub.Domain/Entities`): entidades puras, regras e invariantes de
  negócio. Sem dependência de nada.
- **Application** (`UniHub.Application`): Services (regra de negócio), Interfaces
  (contratos de repositório), DTOs. Organizados por módulo em subpastas.
- **Infrastructure** (`UniHub.Infrastructure`): `AppDbContext`, repositórios
  concretos (EF Core), integrações externas (Cloudinary).
- **API** (`Unihub.API/Controllers`): Controllers, autenticação, composição via DI.

**Regra de dependência:** Domain não depende de nada; Application depende de Domain;
Infrastructure implementa interfaces da Application; API monta tudo via injeção.

## 4. Convenções de código

- **Namespace file-scoped** (`namespace X;`), não com chaves. (Alguns arquivos
  antigos ainda usam chaves — padronizar ao editar, não em massa.)
- **Nomes de domínio em português:** `Usuario`, `Pedido`, `QuantidadeDisponivel`,
  `SolicitacaoApoio`.
- **Extração do usuário autenticado SEMPRE via claim `"sub"`:**
  ```csharp
  var usuarioId = Guid.Parse(User.FindFirst("sub")?.Value ?? Guid.Empty.ToString());
  ```
  ⚠️ **NUNCA** usar `User.FindFirst(ClaimTypes.NameIdentifier)` — o projeto usa
  `MapInboundClaims = false`, então o `sub` NÃO é remapeado e isso retorna `null`.
  (Este bug já apareceu em múltiplos PRs — é o erro nº 1 do projeto.)
- **DTOs organizados por módulo** em subpastas de `DTOs/` (ex.: `DTOs/AcaoSolidaria`).
- **Regras de estado vivem na entidade** (métodos como `Aceitar()`, `Cancelar()`,
  `SuspenderConta()`), nunca como setter solto no controller. O controller chama o
  método e trata a `InvalidOperationException` com um status HTTP adequado (geralmente 409).
- **Autorização por posse:** o dono de um recurso é SEMPRE derivado do token, nunca
  de um id vindo do corpo/query da requisição (evita IDOR).
- **Validação de enum vindo da API:** usar `Enum.IsDefined(...)` / `Enum.TryParse(...)`
  — não assumir que o inteiro recebido é um valor válido do enum.

## 5. Segurança e configuração

- **Segredos NUNCA no `appsettings.json` versionado.** Usar placeholders
  (`SET_VIA_ENV_OR_USER_SECRETS`). Localmente: `dotnet user-secrets`. Em produção:
  variáveis de ambiente no Render (formato `Secao__Chave`, com duplo underline).
- Segredos do projeto: connection string do Neon, `JwtSettings:SecureKey`,
  credenciais do Cloudinary.
- **`EnableSensitiveDataLogging` só em Development** — nunca em produção (LGPD).
- CORS em produção lê as origens de `Cors:AllowedOrigins` (env var).

## 6. Banco de dados e MIGRATIONS (crítico)

> ⚠️ **O maior risco histórico do projeto.** Siga à risca.

- **Guardião de Migrations:** UMA pessoa por sprint centraliza a geração e aplicação
  de migrations. Outros squads NÃO rodam `dotnet ef migrations add` por conta própria.
- Toda mudança em entidade ou no `AppDbContext` que afete o schema gera migration —
  coordene com o Guardião.
- **Sempre testar a migration em uma branch do Neon antes da principal.**
- Ao gerar uma migration, **revise o arquivo**: deve conter só o esperado
  (`CreateTable`, `AddColumn`...). Desconfie de `RenameTable`/`AlterColumn`/`DropTable`
  inesperados — geralmente indicam snapshot dessincronizado.
- Conversões de tipo de coluna (ex.: `text` → `integer` ao trocar string por enum)
  exigem cláusula `USING` manual no Postgres, senão a migration falha com
  "cannot be cast automatically".
- FKs usam `OnDelete(DeleteBehavior.Restrict)` por padrão neste projeto (evita
  cascata indesejada e o erro de "múltiplos caminhos de cascata").
- **A API NÃO aplica migrations no startup** — a aplicação é manual e controlada.

## 7. Requisito técnico central: concorrência de estoque

O decremento de estoque usa **UPDATE condicional atômico** no banco, NÃO lock em
memória nem `RowVersion`:

```sql
UPDATE "Produtos"
SET "QuantidadeDisponivel" = "QuantidadeDisponivel" - @qtd
WHERE "Id" = @id AND "QuantidadeDisponivel" >= @qtd
```

Verifica `rowsAffected == 1`. O mesmo padrão vale para o aceite de solicitações de
apoio (`TryMarcarEmAndamentoAsync`: `UPDATE ... WHERE Status = Aberta`). `RowVersion`
foi avaliado e **descartado** (gerava falso conflito). Mantenha esse padrão.

## 8. Privacidade / LGPD (módulo Ação Solidária)

- A identidade do solicitante é protegida por `CodigoPublico` pseudonimizado.
- **NUNCA** fazer `.Include(s => s.Solicitante)` num endpoint de listagem/consulta
  sem passar por autorização explícita.
- A identidade real só é revelada após `Correspondencia.AutorizarRevelacaoIdentidade()`
  — chamado explicitamente pelo solicitante, nunca automático.
- DTOs de resposta do módulo expõem só o código público e campos não-sensíveis.

## 9. Fluxo de trabalho Git / PRs

- **Pull Requests obrigatórios**, com revisão antes de merge na `main`. Nunca commitar
  direto na `main`.
- **Branches de feature** por tarefa; nomes descritivos (`feat/...`, `fix/...`).
- **Mensagens de commit** no padrão convencional: `feat:`, `fix:`, `chore:`, `docs:`,
  `test:`.
- **Antes de abrir/atualizar PR:** rodar `dotnet build` localmente. Build quebrado é
  recorrente e o CI pega, mas valide antes.
- **Atualizar branch defasada:** usar `git fetch` + `git rebase origin/main`. Após
  rebase, push com `git push --force-with-lease` (NUNCA `git pull` depois de rebase;
  NUNCA `--force` na `main`).
- **Branch muito defasada com código já mergeado:** não rebasear 20+ commits — preferir
  recriar a branch a partir da `main` e trazer só os commits próprios via `git cherry-pick`
  (preserva autoria, importante para a avaliação por commits).
- **Commits preservam autoria** — a avaliação conta commits por pessoa. Ao reintegrar
  trabalho de outra pessoa, use `cherry-pick` (não `git checkout` de arquivos, que
  perde o crédito).
- **PRs pequenos**, especialmente em `AppDbContext.cs` e `Program.cs` (pontos de
  conflito entre squads).
- **Arquivos de CI/pipeline** (`.github/workflows/`) só entram via PR, nunca direto na main.

## 10. Checklist de Code Review (riscos recorrentes)

Ao revisar um PR, verificar:
- [ ] Claim `"sub"` (não `ClaimTypes.NameIdentifier`).
- [ ] DI registrado no `Program.cs` para todo service/repository novo (erro só aparece em runtime).
- [ ] Sem código duplicado por merge (construtor/método/variável repetidos).
- [ ] Autorização por posse via token (sem IDOR).
- [ ] LGPD: nenhum vazamento de identidade na Ação Solidária.
- [ ] Migration coordenada com o Guardião, sem operações inesperadas.
- [ ] `Enum.TryParse`/`IsDefined` para enums vindos da API (evita 500).
- [ ] `dotnet build` limpo (0 erros, idealmente 0 warnings).
- [ ] Arquivos citados no PR realmente commitados.

## 11. Deploy

- Deploy no **Render** a partir de `Backend/Dockerfile`, banco no **Neon**.
- Variáveis de ambiente (segredos) no painel do Render, formato `Secao__Chave`.
- Endpoint `/health` para health check e warm-up do cold start.
- Plano free do Render dorme após 15 min — cold start de ~50s; aquecer antes de demos.
- Runbook completo em `DEPLOY.md`.

## 12. Débitos técnicos conhecidos (não "corrigir" sem alinhar)

- Nomenclatura de pasta inconsistente para Ação Solidária: existem `AcaoSolidaria` E
  `Solidariedade`. Preferir **`AcaoSolidaria`** em código novo.
- Localização de repositórios inconsistente (`Infrastructure/Repositories/` vs
  `Infrastructure/Vendas/`). Preferir `Repositories/<Modulo>/`.
- Inconsistência `Dto` vs `DTO` em nomes de arquivo/record.
- Enum `DiaSemana` definido em `Application.DTOs` em vez de `Domain` (direção de
  dependência invertida).
- Sem paginação nas listagens; sem testes automatizados (planejado para Sprint 4).

## 13. Como rodar localmente

```bash
cd Backend
# configurar segredos (uma vez)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<neon-com-pooling-e-ssl>"
dotnet user-secrets set "JwtSettings:SecureKey" "<chave-32+-caracteres>"
dotnet user-secrets set "Cloudinary:CloudName" "<...>"   # ApiKey, ApiSecret idem

dotnet restore
dotnet ef database update   # só o Guardião de Migrations, idealmente
dotnet run                  # Swagger em /swagger (Development)
```

## 14. Frontend (React + Vite)

O frontend consome a API REST. Os padrões abaixo foram estabelecidos na primeira
tela (login, BRV-116) e **devem ser seguidos** pelas próximas telas, para o front
crescer consistente.

### Stack e libs
- **React 19 + Vite**, JavaScript (não TypeScript).
- **Roteamento:** `react-router-dom`.
- **Login Google:** `@react-oauth/google`.
- **Ícones:** `react-icons`.
- **Estado de autenticação:** Context API (`AuthContext`), sem Redux.

### Estrutura de pastas (`Frontend/src/`)
```
src/
├── auth/            ← autenticação: AuthContext, RotaProtegida, authStorage
├── config/          ← leitura centralizada de env (env.js)
├── services/        ← wrapper de chamadas à API (api.js)
├── pages/           ← telas (uma pasta/arquivo por tela: .jsx + .css)
├── assets/          ← imagens/logos
├── App.jsx          ← rotas
├── main.jsx         ← providers (GoogleOAuth, Router, Auth)
└── index.css        ← tema global
```

### Regras essenciais

- **Toda chamada à API passa pelo wrapper `services/api.js`.** Não usar `fetch`
  direto nas telas. O wrapper injeta `Authorization: Bearer <jwt>` automaticamente,
  prefixa a `VITE_API_BASE_URL`, trata 204 e padroniza erros via `ApiError`.
  - Rotas públicas (ex.: login): passar `{ autenticado: false }`.
- **Variáveis de ambiente só via `config/env.js`.** Nunca ler `import.meta.env`
  espalhado pelo código. Variáveis do Vite exigem o prefixo `VITE_`:
  - `VITE_API_BASE_URL` — URL da API (local `http://localhost:5206`, prod = Render).
  - `VITE_GOOGLE_CLIENT_ID` — Client ID do Google OAuth.
- **Autenticação via `useAuth()`** (do `AuthContext`). Expõe `usuario`,
  `estaAutenticado`, `login()`, `logout()`. A persistência fica isolada em
  `authStorage.js` (hoje `localStorage`; se mudar para cookie, só esse arquivo muda).
- **Rotas que exigem login** são envolvidas por `<RotaProtegida>` (redireciona
  para `/` quem não está autenticado).
- **Segredos / config local:** `.env.local` NÃO é versionado (coberto pelo
  `.gitignore`). Só o `.env.example` entra no repo. O `VITE_GOOGLE_CLIENT_ID`,
  apesar de ficar exposto no browser por natureza, mora em env para não travar
  em um ambiente só.
- **Contrato de login:** o front envia `POST /api/auth/google-login` com
  `{ idToken }` e recebe `{ Mensagem, Usuario, Token }`. O código lê tanto
  `Token`/`Usuario` quanto `token`/`usuario` (resiste a serialização camelCase).
- **Identidade visual:** paleta verde `#2A4515` / tan `#B19563` / creme `#FEF6C7`,
  fontes Playfair Display + Inter. Reusar o tema de `index.css`.

### Fluxo de branches do Frontend (branch de integração por sprint)

Para evitar muitos merges pequenos direto na `main`, o squad de frontend usa uma
**branch de integração por sprint**: `feat/frontend-sprint<N>`.

- **Branches-guia** (fundação: auth, api, estrutura) podem ir **direto para a `main`**
  primeiro, para servir de base a todos.
- As **demais features de front da sprint** têm como base a `feat/frontend-sprint<N>`
  (PRs miram nela, não na `main`).
- No **fim da sprint**, a `feat/frontend-sprint<N>` é unificada na `main` num PR único.
- ⚠️ Manter a branch de integração **sincronizada** com a `main` (rebase/merge
  periódico) para não divergir das branches-guia que foram direto para a main.

### Validação (adaptação do checklist para front)
- `npm run lint` limpo.
- Fluxo validado manualmente no `npm run dev`.
- Nenhum segredo versionado (`.env.local` ignorado).
- Branch rebaseada sobre a base correta antes do PR.

## 15. Glossário de domínio

Termos do negócio (todos em português no código). Útil para entender rapidamente
cada entidade, especialmente no módulo de Ação Solidária, que tem termos não óbvios.

### Usuários e papéis
- **Usuario** — entidade central; autentica via Google (`@unifesp.br`). Tem `Role`
  (`UserRole`: `Aluno=0`, `Voluntario=1`, `Admin=2`) e `IsActive` (conta suspensa
  pela Moderação fica `false`).
- **Vendedor** — perfil opcional de um `Usuario` (relação 1:0..1). Quem publica
  produtos. Tem `ChavePix`, dados da loja e logística.
- **Voluntario** — perfil opcional de um `Usuario` (relação 1:0..1). Quem atende
  solicitações na Ação Solidária. Tem `AreasDeAtuacao` (lista de `CategoriaApoio`)
  e `Disponivel` (pausar/retomar atuação).
  > Obs.: ser Vendedor/Voluntário é representado pela existência do perfil, NÃO por
  > um valor de `UserRole` — evita inconsistência entre o enum e o perfil.

### Comércio (Alimentação e Bazar)
- **Produto** — classe **abstrata** (Table-Per-Hierarchy no EF). Subtipos: `Alimento`
  (Alimentação) e `ItemBazar` (Bazar). `QuantidadeDisponivel` é o campo sensível à
  concorrência (ver seção 7). Tem `ProdutoStatus` (`Disponivel`, `Esgotado`, `Inativo`).
- **Pedido / ItemPedido** — a transação de compra. `Pedido` tem `PedidoStatus`
  (começa em `Reservado`). `ItemPedido` guarda o produto, a quantidade e o
  `PrecoUnitarioNaCompra` (preço "congelado" no momento da compra).

### Ação Solidária (o módulo mais sensível — ver LGPD, seção 8)
- **SolicitacaoApoio** — pedido de ajuda feito por um aluno (o *solicitante*). Tem
  `CodigoPublico` pseudonimizado (ex.: `SOL-A1B2C3`), uma `CategoriaApoio` e
  `StatusSolicitacao` (`Aberta`, `EmAndamento`, `Concluida`, `Cancelada`).
- **Voluntario** — ver acima; é quem pode atender uma solicitação.
- **Correspondencia** — o **"match"** entre um Voluntário e uma Solicitação. Tem
  `StatusCorrespondencia` (`Sugerida` → `AceitaPeloVoluntario` → `Concluida`/`Cancelada`)
  e o flag `IdentidadeRevelada`. É aqui que mora a regra de privacidade: o método
  `AutorizarRevelacaoIdentidade()` só é chamado pelo próprio solicitante, nunca
  automaticamente.
- **Doacao** — registro de uma doação concreta feita em resposta a uma solicitação
  (pode haver mais de uma por solicitação). Tem `StatusDoacao`
  (`Pendente`, `Concluida`, `Cancelada`).
- **CategoriaApoio** — enum das categorias de apoio (ex.: `AulasDeReforco`,
  `DoacaoDeRoupas`, `DoacaoDeAlimentos`, `MaterialAcademico`, `Outros`). Usado tanto
  na Solicitação quanto nas áreas de atuação do Voluntário (para o "match").

### Moderação
- **Denuncia** — reporte feito por um usuário sobre outro usuário (e opcionalmente um
  produto). Tem `DenunciaMotivo` e `DenunciaStatus` (`Aberta`, `EmAnalise`,
  `Resolvida`, `Rejeitada`). O Admin gerencia (painel de moderação planejado).

### Conceitos transversais
- **CodigoPublico** — pseudônimo público e imutável, gerado uma vez
  (`SolicitacaoApoio` e `Voluntario`). Permite exibir/referenciar sem revelar a
  identidade real — base do *privacy by design*.
- **Guardião de Migrations** — papel rotativo por sprint: a pessoa que centraliza
  toda geração/aplicação de migration (ver seção 6).

---

## 16. Mantenha este documento vivo

Este `AGENTS.md` é a fonte de verdade compartilhada do time — qualquer IA (Kiro,
Codex, Cursor, etc.) e qualquer desenvolvingedor o lê para se alinhar. Ele só é
útil se estiver atualizado:
- **Toda convenção nova entra aqui no mesmo PR** que a introduz.
- Decisões voláteis (sprint atual, prioridades) ficam nos documentos de planejamento,
  não aqui.
- Trate-o como código: revisado e versionado.

---

**Resumo em uma frase:** planeje antes de codar, respeite as camadas, use o claim
`"sub"`, nunca versione segredos, coordene migrations com o Guardião, proteja a
identidade na Ação Solidária, e valide com `dotnet build` antes de todo PR.
