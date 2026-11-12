# Plano — BRV-198 (Frente 2, Sprint 5, task A4): Emissão dos eventos no PedidoService

> Continuação da Frente 2 (Painel do Vendedor em Tempo Real). A1 (BRV-185) criou o
> `PainelHub` + infra; A2 (BRV-196) ajustou JWT/CORS; A3 (BRV-197) criou os DTOs,
> `INotificadorPainel` e `NotificadorPainelSignalR`. Esta task (A4) **integra** a
> emissão real dos eventos no fluxo de criação de pedido, sem alterar a regra de
> concorrência de estoque (AGENTS §7).

## Objetivo
Fazer o `PedidoService.CreatePedidoAsync` notificar o painel do vendedor em tempo
real: `EstoqueAtualizado` (nova disponibilidade de cada produto afetado) e
`NovoPedido` (resumo do pedido que afeta produtos do vendedor). A notificação é um
efeito colateral best-effort — nunca pode comprometer a venda.

## Escopo
- **Inclui:**
  - Injetar `INotificadorPainel` no `PedidoService`.
  - Emitir `EstoqueAtualizado` e `NovoPedido` em `CreatePedidoAsync`.
  - Testes do contrato de emissão.
- **NÃO inclui:**
  - Mudança na regra de concorrência de estoque (AGENTS §7 — intacta).
  - Emissão em outros fluxos (cancelamento, expiração, conclusão) — fora do escopo
    da frente; o painel foca "novo pedido consumindo estoque".
  - Logging estruturado da falha de notificação (ver "Pendências").

## Base da branch
- Branch: **`feat/BRV-198-emissao-eventos-pedido`**, criada da **`main`**
  atualizada (`acfd024`, já com A1/A2/A3 mergeadas).

## Decisões de design e discussões

### 1. Momento da emissão: APÓS o sucesso, não durante o loop
O `CreatePedidoAsync` decrementa o estoque item a item num loop; se um item
posterior falha, faz rollback (`RollbackAsync` devolve o estoque dos já
decrementados) e o pedido não é salvo.

- **Discussão:** emitir `EstoqueAtualizado` logo após cada decremento (como sugeria
  o rascunho do plano) tem o risco de notificar uma mudança que seria **desfeita**
  por um rollback de um item seguinte — o painel mostraria um estoque que nunca
  valeu.
- **Escolha:** acumular os dados de notificação numa lista durante o loop e **emitir
  só depois do `AddAsync` bem-sucedido**. Assim só notificamos o que de fato foi
  persistido.
- **Motivo:** correção acima de simplicidade — evita evento "fantasma" sob falha
  parcial. O custo é uma lista temporária em memória (barato).

### 2. Cálculo da nova quantidade: no service, sem ida extra ao banco
`TryDecrementarEstoqueAsync` é um UPDATE condicional atômico que retorna só `bool`
(AGENTS §7). `GetByIdAsync` traz o produto com a quantidade **anterior**
(`AsNoTracking`).

- **Discussão:** para enviar a quantidade resultante, ou **(a)** re-consultar o
  produto após o decremento, ou **(b)** calcular no service (`anterior - decrementada`).
- **Escolha:** **(b)** calcular no service.
- **Motivo:** evita uma ida extra ao banco; o service já conhece as duas parcelas.
  A regra de concorrência **não muda** — o evento é só notificação. (Pendência: sob
  dois pedidos simultâneos, confirmar na integração que o valor calculado reflete o
  banco; mesmo divergindo por instantes, o painel se reconcilia no próximo load REST.)

### 3. Status no evento: derivar "Esgotado" ao zerar (só no payload)
`TryDecrementarEstoqueAsync` altera apenas `QuantidadeDisponivel`; **não** muda o
`Status` no banco. Logo, um produto com quantidade 0 continua `Disponivel` no banco.

- **Discussão:** o critério de aceite pede "quando o estoque zera, o card reflete
  Esgotado ao vivo". Três caminhos: **(i)** enviar o status real do banco
  (`Disponivel`) — fiel, mas não cumpre o critério; **(ii)** derivar `Esgotado` no
  payload quando a quantidade resultante é 0 — cumpre o critério, contido na
  notificação; **(iii)** mudar a regra para persistir `Esgotado` — fora do escopo e
  mexeria em §7.
- **Escolha:** **(ii)** derivar `Esgotado` apenas no payload do evento.
- **Motivo:** atende o critério visual sem tocar a regra de estoque (AGENTS §7/§0.4).
  A derivação vive só no evento de notificação; o estado persistido não é alterado
  por esta task.

### 4. Tipo do campo Status: enum `ProdutoStatus` (número), não string
- **Discussão:** o rascunho do plano trazia `string Status`. O padrão do projeto
  expõe enums como número (sem `JsonStringEnumConverter`), e o estado inicial do
  painel (`MeuProdutoDto.Status`) já é `ProdutoStatus`.
- **Escolha:** `ProdutoStatus` (definido na A3), serializado como número.
- **Motivo:** o correto pelo AGENTS §14 — o evento ao vivo bate com o estado inicial
  que o front já renderiza; sem formato divergente para o mesmo campo.

### 5. NovoPedido por produto + uma única lista de acumulação
`NovoPedidoDto` é por produto. Os dados (`ProdutoId`, `NomeProduto`, `Quantidade`,
`VendedorId`) já estão no loop; o `PedidoId` e o `QuandoUtc` saem do pedido salvo.

- **Discussão:** usar duas listas/dois loops (um por evento) ou uma tupla única.
- **Escolha:** **uma tupla única** (`VendedorId`, `Estoque`, `ProdutoId`,
  `NomeProduto`, `Quantidade`) e um único loop final emitindo os dois eventos.
- **Motivo:** menos duplicação, emissão coesa e no mesmo ponto (após o sucesso).

### 6. QuandoUtc = `salvo.CreatedAt`
- **Discussão:** usar `DateTime.UtcNow` no momento da emissão ou o carimbo do pedido.
- **Escolha:** `salvo.CreatedAt` (confirmado como `DateTime.UtcNow` por padrão na
  entidade `Pedido`).
- **Motivo:** reflete o momento real do pedido persistido, não o instante do envio
  do evento (mais correto semanticamente).

### 7. Falha do hub é best-effort (try/catch por evento)
- **Discussão:** se o SignalR falhar ao notificar, a venda (já persistida) não pode
  cair. Opções de tratamento: deixar propagar (inaceitável), um try/catch único
  cobrindo os dois envios, ou um try/catch por evento.
- **Escolha:** **try/catch por evento** — a falha de `EstoqueAtualizado` não impede
  o `NovoPedido`, e vice-versa. `catch` best-effort (engole a exceção de propósito).
- **Motivo:** critério de aceite "falha/queda do hub não derruba a criação de
  pedido". O painel se reconcilia no próximo carregamento REST (corte seguro).
- **Trade-off conhecido:** o `catch` não loga (não há `ILogger` injetado no
  `PedidoService`). Injetar `ILogger<PedidoService>` mudaria o construtor e os
  testes de novo; optou-se por **não** inflar a A4. Deixado comentário no código
  indicando onde logar. (Ver "Pendências".)

### 8. Impacto nos testes existentes
Mudar o construtor do `PedidoService` quebra a compilação dos 5 arquivos de teste
que fazem `new(_produtoRepo, _pedidoRepo)`.

- **Escolha:** no mesmo commit da injeção, atualizar o helper `CriarService()` dos 5
  arquivos para passar um `Substitute.For<INotificadorPainel>()`.
- **Motivo:** cada commit deve compilar limpo (AGENTS §0.3); não dá para separar
  "mudar o construtor" de "consertar os testes".

## Divisão em commits (5)
1. `feat`: injeta `INotificadorPainel` no `PedidoService` + ajusta os 5 testes.
2. `feat`: emite `EstoqueAtualizado` em `CreatePedidoAsync`.
3. `feat`: emite `NovoPedido` (tupla única + `salvo.CreatedAt`).
4. `test`: `PedidoServiceNotificacaoTests` (5 casos).
5. `docs`: este arquivo.

## O que foi feito nesta task (A4)
Arquivos modificados:
- `Backend/src/UniHub.Application/Services/Vendas/PedidoService.cs` — injeção +
  emissão dos dois eventos.
- 5 arquivos de teste do `PedidoService` — helper do construtor.

Arquivos criados:
- `Tests/UniHub.Tests/Application/PedidoServiceNotificacaoTests.cs`.

Validação:
- `dotnet build` limpo após cada commit.
- `dotnet test --filter "Categoria=Unit"` → **119 testes passando, 0 falhas**.

## Critérios de aceite (A4)
- [x] `EstoqueAtualizado` emitido para cada produto afetado, após salvar o pedido.
- [x] Nova `QuantidadeDisponivel` calculada no service (sem ida extra ao banco).
- [x] Status reflete `Esgotado` ao zerar (derivado no payload).
- [x] `NovoPedido` emitido por produto afetado, com `QuandoUtc = CreatedAt`.
- [x] Isolamento por vendedor (envio ao grupo `vendedor:{id}` — via A3).
- [x] Falha do hub não derruba a criação do pedido (try/catch best-effort).
- [x] Regra de concorrência de estoque (AGENTS §7) inalterada.
- [x] `dotnet build` limpo + 119 testes Unit passando.

## Pendências / dúvidas em aberto
- **Logging da falha de notificação:** hoje best-effort sem log. Avaliar injetar
  `ILogger<PedidoService>` numa melhoria futura (muda construtor + testes).
- **Cálculo sob concorrência (T-int):** confirmar na integração real que a
  quantidade calculada no service bate com o banco sob dois pedidos simultâneos.
- **Teste manual (A5):** criar pedido via Swagger e verificar no console do front
  (trilha B) que `EstoqueAtualizado`/`NovoPedido` chegam ao vendedor dono.
- **Emissão em outros fluxos:** cancelamento/expiração devolvem estoque, mas não
  notificam o painel. Se o painel precisar refletir isso ao vivo, é uma extensão
  futura (fora do escopo desta frente).
