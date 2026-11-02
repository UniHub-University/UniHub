# Plano — P4/P5 Expiração de Reservas (Squad B)

## Objetivo
Fazer uma Reserva de pedido expirar automaticamente quando `ExpiraEm` vence:
devolver o estoque reservado e marcar o pedido como `Expirado`. Dois caminhos
complementares — um job em segundo plano (P4) e uma rede de segurança *lazy*
nas consultas (P5) — para que a expiração aconteça mesmo quando o processo
hiberna no plano free do Render.

## Escopo
- **Inclui:**
  - P4: `BackgroundService` que, a cada 10 min, expira reservas vencidas
    (devolve estoque + marca `Expirado`).
  - P5: checagem *lazy* nos GETs de pedido — resolve **apenas as reservas
    vencidas que aparecem na consulta atual** antes de montar a resposta.
  - Lógica de expiração compartilhada (uma única rotina reusada por P4 e P5).
  - Testes.
- **NÃO inclui:**
  - Notificação em tempo real ao usuário (fica no backlog; o aviso é o status
    `Expirado` aparecer no histórico — decisão do planejamento da sprint).
  - Alterar o fluxo de criação de Reserva (P2/P3, já mergeado) nem a conclusão
    manual (P7, já mergeado).

## Decisões de design (fechadas com o time)
- **Intervalo do job:** 10 minutos (equilíbrio entre pontualidade e volume de
  requisições ao banco).
- **TTL da reserva:** 48h. Ajustar `PedidoService.TtlReserva` de 24h → 48h.
- **Onde mora a lógica compartilhada:** no `PedidoService` (um método de
  expiração reutilizado pelo job e pela checagem lazy), reusando
  `IProdutoRepository.IncrementarEstoqueAsync` para devolver estoque.
- **Escopo da expiração lazy:** resolve **somente** as reservas vencidas
  presentes na consulta atual (não varre o banco inteiro a cada GET), por
  performance. O job (P4) cobre o resto.
- **Atomicidade:** marcar `Expirado` via UPDATE condicional
  `WHERE Status = Reservado` (mesmo espírito de `TryConcluirAsync`). Isso
  garante que, se o job e a checagem lazy (ou uma conclusão manual) caírem na
  mesma reserva, **só um vence** e o estoque é devolvido **uma única vez**
  (evita devolução dupla). Só devolve estoque se o UPDATE afetou 1 linha.

## Decisões de design — detalhes técnicos
- **Entidades/Domain:** `Pedido` ganha um método `Expirar()` (espelha
  `Concluir()`: só `Reservado` pode expirar; senão `InvalidOperationException`).
  `PedidoStatus.Expirado` e `Pedido.ExpiraEm` já existem (B2).
- **Repositório (`IPedidoRepository` + `PedidoRepository`):**
  - `GetReservasVencidasAsync(DateTime agora)` — reservas com
    `Tipo = Reserva`, `Status = Reservado`, `ExpiraEm <= agora`, carregando os
    `Itens` (produto + quantidade) para devolver estoque. Usado pelo job (P4).
  - `TryExpirarAsync(Guid pedidoId)` — UPDATE condicional atômico
    (`WHERE Status = Reservado`), define `Status = Expirado` e `UpdatedAt`.
    Retorna `true` só se afetou 1 linha (quem "ganhou" a corrida).
- **Service (`PedidoService`):**
  - `ExpirarReservaAsync(Pedido)` (privado/compartilhado): tenta
    `TryExpirarAsync`; se ganhou, devolve o estoque de cada item via
    `IncrementarEstoqueAsync`. Idempotente por construção (se outro já expirou,
    `TryExpirarAsync` devolve false e não devolvemos estoque de novo).
  - `ExpirarReservasVencidasAsync()`: usado pelo job — busca vencidas e chama
    `ExpirarReservaAsync` para cada uma.
  - `GetHistoricoAsync` e `GetPedidosDoVendedorAsync`: antes de montar a
    resposta, expiram *lazy* as reservas vencidas **daquela página** e refletem
    o novo status no DTO.
- **BackgroundService (novo):** `ExpiracaoReservasBackgroundService : BackgroundService`.
  Loop com `PeriodicTimer` de 10 min. **Cria um `IServiceScope` por ciclo**
  (via `IServiceScopeFactory`) para resolver o `PedidoService` scoped — NUNCA
  injetar repositório/DbContext scoped direto num hosted service singleton.
  Envolve cada ciclo em try/catch + log para o job não morrer se um ciclo falhar.
- **Program.cs:** `builder.Services.AddHostedService<ExpiracaoReservasBackgroundService>();`
- **Banco/Migrations:** **nenhuma** — `Expirado` e `ExpiraEm` já existem (B2).
- **Impacto em concorrência/LGPD:** concorrência tratada pelo UPDATE condicional
  (sem devolução dupla de estoque). Sem impacto de LGPD.
- **Trade-offs considerados:**
  - Job a cada 10 min vs tempo real: 10 min reduz carga; o *lazy* cobre a
    janela entre ciclos e a hibernação do Render.
  - Lazy só da consulta vs varrer tudo: só da consulta evita custo por GET; o
    job garante que o resto também expira.

## Passos de implementação
1. **Domain:** adicionar `Pedido.Expirar()`.
2. **Application (interface):** `GetReservasVencidasAsync` e `TryExpirarAsync`
   em `IPedidoRepository`.
3. **Infrastructure:** implementar os dois no `PedidoRepository` (UPDATE
   condicional atômico no `TryExpirarAsync`).
4. **Application (service):** método compartilhado de expiração no
   `PedidoService`; ajustar `TtlReserva` para 48h; chamar a expiração lazy em
   `GetHistoricoAsync` e `GetPedidosDoVendedorAsync`.
5. **BackgroundService:** criar o hosted service com `IServiceScopeFactory` +
   `PeriodicTimer` (10 min); registrar no `Program.cs`.
6. **Testes:** unitários do service (reserva vencida → estoque devolvido +
   status Expirado; idempotência — não devolve estoque duas vezes; compra não
   expira; lazy reflete no DTO).

## Critérios de aceite
- [ ] Reserva com `ExpiraEm` no passado e `Status = Reservado` vira `Expirado`
      e tem o estoque devolvido.
- [ ] Estoque é devolvido **uma única vez**, mesmo com job + lazy concorrendo.
- [ ] Compra (`Tipo = Compra`) nunca expira.
- [ ] GET de histórico e de pedidos do vendedor refletem `Expirado` na resposta.
- [ ] Job roda a cada 10 min, cria scope próprio por ciclo e não derruba a app
      se um ciclo falhar.
- [ ] `TtlReserva` = 48h.
- [ ] `dotnet build` limpo.
- [ ] Sem migration (schema inalterado).

## Pendências / dúvidas em aberto
- Nenhuma no momento — as 4 decisões abertas foram fechadas (intervalo 10 min,
  TTL 48h, lógica no PedidoService, lazy só da consulta atual).
