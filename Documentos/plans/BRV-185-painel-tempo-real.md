# Plano — BRV-185 (Frente 2, Sprint 5): Painel do Vendedor em Tempo Real (SignalR)

> Este `.md` registra o COMO da Frente 2. O board (YouTrack) continua sendo a
> fonte do O QUE/quando. A frente é dividida em trilhas A (Backend/Hub) e B
> (Frontend/Consumo); este documento foca a **trilha A** e marca o progresso de
> cada task. A task em andamento aqui é a **A1**.

## Objetivo
Entregar um painel do vendedor que reflita, ao vivo e sem refresh, a
disponibilidade dos seus produtos (estoque muda ou chega novo pedido). É o
diferencial técnico do pitch da entrega final. SignalR é o incremento sobre o
estado inicial via REST — não é pré-requisito para os 3 módulos demonstráveis
(corte seguro do Planejamento_Sprint5).

## Escopo
- **Inclui (trilha A — Backend/Hub):**
  - `PainelHub` (SignalR) com `[Authorize]`, agrupando conexões por vendedor
    em `vendedor:{VendedorId}`.
  - Dois eventos: `EstoqueAtualizado` e `NovoPedido` (A3/A4).
  - Interface `INotificadorPainel` (Application) + implementação
    `NotificadorPainelSignalR` (API) via `IHubContext<PainelHub>` (A3).
  - Integração no `PedidoService.CreatePedidoAsync` sem derrubar a venda se o
    hub falhar (A4).
  - Infra SignalR no `Program.cs`: `AddSignalR()`/`MapHub` (A1); JWT via
    `access_token` na query string + `AllowCredentials()` no CORS (A2).
- **NÃO inclui:**
  - Endpoint de "meus produtos" — já existe `GET /api/perfil/vendedor/produtos`
    (BRV-164). O painel reusa para o estado inicial.
  - Mudança de schema / migration (nenhuma entidade nova).
  - Alterar a regra de concorrência de estoque (AGENTS §7 — mantida intacta).
  - Tempo real em outras telas (Bazar público, Alimentação).
  - Tela completa de pedidos recebidos (só o evento `NovoPedido` → toast/contador).

## Decisões de design

### Contrato dos eventos (T0 — fechado com a trilha B)
- Path do hub: **`/hubs/painel`**.
- Nomes dos eventos: **`EstoqueAtualizado`** e **`NovoPedido`**.
- Grupo por vendedor: **`vendedor:{VendedorId}`** (helper `PainelHub.NomeGrupoVendedor`).
- Serialização em **camelCase** (padrão do projeto).
- Payloads (DTOs em Application — a criar na A3):
  - `EstoqueAtualizadoDto { Guid ProdutoId, int QuantidadeDisponivel, string Status }`
  - `NovoPedidoDto { Guid PedidoId, Guid ProdutoId, string NomeProduto, int Quantidade, DateTime QuandoUtc }`

### Entidades/Domain
Nenhuma entidade nova. O evento carrega dados já existentes (`Produto`, `Pedido`).

### Endpoints/API
- Hub mapeado em `/hubs/painel` (não é rota REST; é endpoint de WebSocket/SignalR).
- Estado inicial do painel reusa `GET /api/perfil/vendedor/produtos` (BRV-164).

### Banco/Migrations
Não muda schema. Não precisa de migration. Nada a coordenar com o Guardião.

### Impacto em concorrência/LGPD
- Concorrência: a regra de estoque (AGENTS §7) **não muda**. O evento é só
  notificação; a nova quantidade será calculada no service a partir do valor
  anterior/decrementado (confirmar sob concorrência na integração — ver A4).
- Isolamento por grupo: um vendedor só recebe eventos dos seus produtos. Usuário
  autenticado que não é vendedor conecta mas fica sem grupo (não recebe nada).
  Não há broadcast global.

### Trade-offs considerados (A1)
- **Resolver `Vendedor.Id` a partir do usuário:** criei a interface de consulta
  `IVendedorConsultaRepository` (Application) + `VendedorConsultaRepository`
  (Infrastructure), em vez de **(a)** injetar `AppDbContext` direto no hub (mistura
  lógica de EF Core na camada API e dificulta teste) ou **(b)** reusar o
  `VendedorService` (carrega dependências que o hub não precisa, ex.:
  `IImageStorageService`). A interface enxuta preserva a direção de dependência da
  Clean Architecture (AGENTS §3) e é **reuso concreto**: A3/A4 precisam do mesmo
  mapeamento usuário→vendedor para emitir eventos (não é abstração especulativa —
  AGENTS §0.2).
- **Grupo por vendedor (vs. broadcast global):** só o dono recebe eventos dos seus
  produtos — menos tráfego e alinhado à privacidade.
- **Helper `NomeGrupoVendedor(Guid)` estático no hub:** A3/A4 montam o mesmo nome
  de grupo ao emitir, evitando string mágica repetida/divergente.
- **`Context.Abort()` sem `sub` válido:** mesmo com `[Authorize]`, se o token não
  tiver `sub` parseável a conexão é abortada em vez de seguir sem vendedor.

## Passos de implementação (trilha A)
1. **A1 — Hub + infra base (ESTA task):**
   - Domain/Infra: `IVendedorConsultaRepository` (Application) +
     `VendedorConsultaRepository` (Infrastructure, `AsNoTracking`, projeta `Guid?`).
   - API: `PainelHub : Hub` com `[Authorize]`; `OnConnectedAsync` resolve o
     usuário via claim `"sub"` (AGENTS §4, nunca `ClaimTypes.NameIdentifier`),
     resolve o vendedor e adiciona a conexão ao grupo `vendedor:{id}`.
   - `Program.cs`: `AddSignalR()`, DI de `IVendedorConsultaRepository`,
     `MapHub<PainelHub>("/hubs/painel")`.
2. **A2:** JWT `OnMessageReceived` lendo `access_token` da query string para
   paths `/hubs/` + `AllowCredentials()` no CORS. PR isolado do `Program.cs`
   (AGENTS §9).
3. **A3:** `INotificadorPainel` (Application) + `NotificadorPainelSignalR` (API)
   com `IHubContext<PainelHub>` + DI. DTOs de evento.
4. **A4:** Integrar no `PedidoService.CreatePedidoAsync` — emitir
   `EstoqueAtualizado` após cada decremento e `NovoPedido` após salvar, com
   try/catch para nunca derrubar a venda se o hub falhar.
5. **A5:** `dotnet build` limpo + teste manual de emissão.

## O que foi feito nesta task (A1)
Arquivos criados:
- `Backend/src/UniHub.Application/Interfaces/Vendas/IVendedorConsultaRepository.cs`
- `Backend/src/UniHub.Infrastructure/Repositories/Vendas/VendedorConsultaRepository.cs`
- `Backend/src/Unihub.API/Hubs/PainelHub.cs`

Arquivo modificado:
- `Backend/Program.cs` — `AddSignalR()`, DI de `IVendedorConsultaRepository`,
  `MapHub<PainelHub>("/hubs/painel")`.

Validação:
- `dotnet build` limpo (0 avisos, 0 erros).
- Decisão de deixar A2 (JWT via query string + CORS `AllowCredentials`) fora
  desta task, para PR isolado do `Program.cs` (AGENTS §9).

## Critérios de aceite (A1)
- [x] `PainelHub` criado com `[Authorize]`.
- [x] `AddSignalR()` e `MapHub<PainelHub>("/hubs/painel")` registrados no `Program.cs`.
- [x] Conexão resolve o vendedor pelo claim `"sub"` e agrupa em `vendedor:{id}`.
- [x] Usuário não-vendedor conecta sem entrar em grupo (não recebe eventos).
- [x] Conexão sem `sub` válido é abortada.
- [x] `dotnet build` limpo.
- [ ] Teste manual de conexão autenticada contra o front da trilha B (depende da
      A2 para o handshake WebSocket com `access_token`) — fica para A2/integração.

## Critérios de aceite (frente completa — referência)
- [ ] Vendedor vê seus produtos (estado inicial via REST), mesmo sem SignalR.
- [ ] Pedido que consome estoque atualiza o card sem refresh (< ~2s).
- [ ] Estoque zerado reflete status `Esgotado` ao vivo.
- [ ] `NovoPedido` dispara indicador discreto no painel do dono.
- [ ] Isolamento por grupo (um vendedor não recebe eventos de outro).
- [ ] Hub exige JWT válido (conexão sem token rejeitada).
- [ ] Falha/queda do hub não derruba a criação de pedido nem quebra o painel.
- [ ] Reconexão automática após queda/cold start.
- [ ] `dotnet build` limpo (back) e `npm run lint`/`npm run build` limpos (front).
- [ ] PRs pequenos revisados; `Program.cs` em PR isolado (AGENTS §9/§10).

## Pendências / dúvidas em aberto
- **A2 é pré-requisito do teste real de conexão:** o handshake do SignalR por
  WebSocket não manda header `Authorization`; precisa do `access_token` na query
  string (A2). Até lá, o `MapHub` já existe mas a conexão autenticada ponta-a-ponta
  com o front só fecha após a A2.
- **Cálculo da nova quantidade (A4):** confirmar na integração se o valor calculado
  no service bate com o banco sob concorrência (dois pedidos simultâneos).
- **Origem exata de produção para o CORS (A2):** combinar com quem cuida do deploy
  (Render) a origem a liberar com `AllowCredentials()` (não usar `AllowAnyOrigin()`
  junto com credenciais).
- **Convenção nova de SignalR:** se consolidar, adicionar ao `AGENTS.md` no mesmo
  PR que a introduz (AGENTS §16).
