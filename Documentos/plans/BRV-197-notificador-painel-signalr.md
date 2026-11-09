# Plano — BRV-197 (Frente 2, Sprint 5, task A3): Notificador do Painel (SignalR)

> Continuação da Frente 2 (Painel do Vendedor em Tempo Real). A A1 (BRV-185)
> entregou o `PainelHub` + infra base; a A2 (BRV-196) ajustou JWT/CORS. Esta task
> (A3) cria a **camada de notificação**: os DTOs dos eventos, a abstração
> `INotificadorPainel` (Application) e a implementação `NotificadorPainelSignalR`
> (API). A emissão de fato dentro do `PedidoService` fica para a A4.

## Objetivo
Disponibilizar uma forma de notificar o painel do vendedor em tempo real sem
acoplar a regra de negócio ao SignalR, preservando a direção de dependência da
Clean Architecture (AGENTS §3). Ao fim da A3, o `PedidoService` (A4) só precisa
depender de `INotificadorPainel` e chamar dois métodos.

## Escopo
- **Inclui:**
  - DTOs dos eventos: `EstoqueAtualizadoDto` e `NovoPedidoDto` (Application).
  - Interface `INotificadorPainel` (Application).
  - Implementação `NotificadorPainelSignalR` (API) via `IHubContext<PainelHub>`.
  - Registro no DI (`Program.cs`).
- **NÃO inclui:**
  - Emissão dos eventos dentro do `PedidoService.CreatePedidoAsync` → task A4.
  - Mudança no `PainelHub` (já entregue na A1).
  - Mudança de schema / migration (eventos carregam dados já existentes).
  - Alterar a regra de concorrência de estoque (AGENTS §7 — intacta).

## Base da branch
- Branch: **`feat/BRV-197-notificador-painel-signalr`**, criada a partir da
  **`main`** (A1 e A2 já mergeadas na `main` — base atualizada para `42e4d29`).

## Decisões de design

### DTOs dos eventos (Application — `DTOs/Vendas/PainelEventosDtos.cs`)
- `EstoqueAtualizadoDto(Guid ProdutoId, int QuantidadeDisponivel, ProdutoStatus Status)`
  - `QuantidadeDisponivel` é a quantidade **resultante** (pós-decremento).
  - `Status` é o enum `ProdutoStatus`, serializado como **número** — consistente
    com o estado inicial do painel (`MeuProdutoDto.Status`) e com o padrão do
    projeto, em que o front recebe enums como número e faz o mapeamento
    (AGENTS §14). Assim o evento ao vivo bate com o que o front já renderiza.
- `NovoPedidoDto(Guid PedidoId, Guid ProdutoId, string NomeProduto, int Quantidade, DateTime QuandoUtc)`
  - `QuandoUtc` em UTC (o front formata no fuso local).
- `record` com namespace file-scoped, seguindo os demais DTOs de Vendas.
- Serialização em camelCase (padrão do projeto).

### Interface `INotificadorPainel` (Application — `Interfaces/Vendas`)
- Métodos: `NotificarEstoqueAsync(Guid vendedorId, EstoqueAtualizadoDto)` e
  `NotificarNovoPedidoAsync(Guid vendedorId, NovoPedidoDto)`.
- Mora na Application para o `PedidoService` depender só da abstração, nunca do
  SignalR/`IHubContext` (direção de dependência — AGENTS §3).
- Recebe o `vendedorId` **explícito**: quem emite (o `PedidoService` na A4) já
  tem o `produto.VendedorId`, então a implementação não precisa resolver nada —
  só traduzir para o grupo do hub.

### Implementação `NotificadorPainelSignalR` (API — `Hubs/`)
- Injeta `IHubContext<PainelHub>` e envia cada evento **apenas ao grupo**
  `PainelHub.NomeGrupoVendedor(vendedorId)` → isolamento por vendedor (um
  vendedor não recebe eventos de outro).
- Reusa o helper `PainelHub.NomeGrupoVendedor` (criado na A1) para o nome do
  grupo bater exatamente com o usado no `OnConnectedAsync` — sem string mágica
  divergente.
- Nomes dos eventos: `"EstoqueAtualizado"` e `"NovoPedido"` (contrato com a
  trilha B / front).

### DI (`Program.cs`)
- `builder.Services.AddScoped<INotificadorPainel, NotificadorPainelSignalR>();`,
  ao lado dos demais serviços de vendas.
- **Lifetime Scoped:** adequado porque o `PedidoService` (consumidor na A4) é
  scoped. A implementação injeta `IHubContext<PainelHub>` (singleton no SignalR,
  que não instancia o hub), então não há conflito de lifetime (captive dependency).

## Passos de implementação (3 commits separados)
1. **Commit 1 — DTOs:** `PainelEventosDtos.cs` (`EstoqueAtualizadoDto`,
   `NovoPedidoDto`).
2. **Commit 2 — Interface:** `INotificadorPainel.cs`.
3. **Commit 3 — Implementação + DI:** `NotificadorPainelSignalR.cs` +
   registro no `Program.cs`.
4. **Commit 4 — Doc:** este arquivo.

## O que foi feito nesta task (A3)
Arquivos criados:
- `Backend/src/UniHub.Application/DTOs/Vendas/PainelEventosDtos.cs`
- `Backend/src/UniHub.Application/Interfaces/Vendas/INotificadorPainel.cs`
- `Backend/src/Unihub.API/Hubs/NotificadorPainelSignalR.cs`

Arquivo modificado:
- `Backend/Program.cs` — registro de `INotificadorPainel`.

Validação:
- `dotnet build` limpo (0 avisos, 0 erros) após cada commit.

## Critérios de aceite (A3)
- [x] DTOs `EstoqueAtualizadoDto` e `NovoPedidoDto` criados (Application).
- [x] Interface `INotificadorPainel` na Application (abstração, sem SignalR).
- [x] Implementação `NotificadorPainelSignalR` na API via `IHubContext<PainelHub>`,
      enviando ao grupo `vendedor:{id}`.
- [x] Registro no DI (`Program.cs`).
- [x] `dotnet build` limpo.
- [ ] Emissão real a partir do `PedidoService` — task A4.

## Pendências / dúvidas em aberto
- **A4 (emissão no `PedidoService`):** emitir `EstoqueAtualizado` após cada
  decremento bem-sucedido e `NovoPedido` após salvar o pedido, com try/catch para
  nunca derrubar a venda se o hub falhar. Calcular `QuantidadeDisponivel`
  resultante no service (o service já conhece a quantidade anterior e a
  decrementada) — confirmar sob concorrência na integração.
- **Teste manual (A5):** criar pedido via Swagger e verificar no console do front
  (trilha B) que o evento chegou.
