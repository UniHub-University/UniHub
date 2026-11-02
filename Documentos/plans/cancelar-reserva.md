\# Plano — Cancelar reserva pelo comprador + tipo no card do Perfil (Squad B, extensão da Sprint 4)



\## Objetivo

(1) Permitir que o comprador cancele uma \*\*reserva\*\* sua com um "✕", devolvendo o estoque; o pedido cancelado some da visão do comprador, mas continua visível ao vendedor como Cancelado. (2) Mostrar Compra vs Reserva no card "Últimas compras" do Perfil. Compra (definitiva) não pode ser cancelada.



\## Escopo

\- \*\*Inclui:\*\* `Pedido.Cancelar()`; `POST /api/pedidos/{id}/cancelar`; cancelamento atômico com devolução única do estoque; filtro que esconde pedidos cancelados do histórico do comprador; botão "✕" com confirmação em `/pedidos` e no card do Perfil; etiqueta de tipo no card; testes de unidade.

\- \*\*NÃO inclui:\*\* cancelar Compra; cancelamento pelo vendedor; notificações; apagar o pedido do banco; mudança de schema.



\## Decisões de design

\- \*\*Entidades/Domain:\*\* `Pedido.Cancelar()` só vale para `Tipo = Reserva` e `Status = Reservado`; qualquer outro caso lança `InvalidOperationException` (vira 409). Atualiza `UpdatedAt`. Usa o status `Cancelado` (3), que já existe.

\- \*\*Endpoints/API:\*\* `POST /api/pedidos/{id}/cancelar`, sem corpo. 200 (cancelada), 401 (token sem `sub`), 403 (pedido de outro usuário), 404 (inexistente), 409 (não é Reserva, ou não está mais `Reservado`).

\- \*\*Dono:\*\* o comprador vem do token (claim `"sub"`); a checagem de posse vem antes da regra de estado, para não revelar o status a quem não é dono (como no P7).

\- \*\*Concorrência:\*\* UPDATE condicional (`WHERE Status = Reservado AND Tipo = Reserva`), no padrão de `TryExpirarAsync`. \*\*Só quem ganha a corrida devolve o estoque\*\*, então o estoque volta uma única vez mesmo se o job de expiração, a checagem lazy, a baixa do vendedor ou um duplo clique disputarem o pedido.

\- \*\*Visibilidade:\*\* o pedido cancelado \*\*não é apagado\*\* (a linha fica no banco com status Cancelado, preservando o rastro). `GetByCompradorAsync` passa a filtrar `Status != Cancelado`, então o histórico, o card e a paginação do comprador não o mostram. A lista do vendedor (`GetByVendedorAsync`) \*\*não muda\*\*: ele vê o pedido como Cancelado, sem o botão Concluir.

\- \*\*Banco/Migrations:\*\* nenhuma.

\- \*\*Frontend:\*\* `cancelarReserva(pedidoId)` em `services/pedidos.js`; "✕" visível só quando `tipo = 1` e `status = 0`; `window.confirm` antes de enviar; após o sucesso a lista é recarregada; o erro da API é exibido. O card do Perfil ganha a linha "Compra" ou "Reserva até dd/mm", igual ao histórico.

\- \*\*Trade-offs considerados:\*\*

&#x20; - Esconder (filtrar) em vez de apagar: o comprador não vê, mas o vendedor e o rastro de auditoria continuam, e a devolução única do estoque fica segura.

&#x20; - POST `/cancelar` em vez de DELETE: o registro é preservado.

&#x20; - 403 (e não 404) para pedido de outro usuário: segue o padrão do P7.

&#x20; - Um PR com commits separados (domínio, backend, histórico, card do Perfil), para facilitar a revisão.



\## Passos de implementação

1\. Domain: `Pedido.Cancelar()` + testes da regra.

2\. Application/Infrastructure: `TryCancelarAsync`, filtro de cancelados em `GetByCompradorAsync`, DTO de resultado, `PedidoService.CancelarReservaAsync` + testes.

3\. API: `POST api/pedidos/{id}/cancelar` no `PedidosController`.

4\. Front: `cancelarReserva` em `services/pedidos.js`; botão "✕" em `HistoricoPedidos.jsx` e estilo no `.css`.

5\. Front: card do Perfil com tipo e "✕" (`Perfil.jsx` e `Perfil.css`).



\## Critérios de aceite

\- \[ ] O comprador cancela a própria reserva; o pedido some do histórico e do card dele

\- \[ ] O estoque dos itens volta exatamente uma vez

\- \[ ] O vendedor continua vendo o pedido, como Cancelado, sem botão Concluir

\- \[ ] Compra, pedido concluído, expirado ou já cancelado devolvem 409

\- \[ ] Pedido de outro usuário devolve 403; inexistente, 404

\- \[ ] O "✕" só aparece em reservas ativas

\- \[ ] O card do Perfil mostra Compra ou Reserva até dd/mm

\- \[ ] `dotnet build`, `dotnet test` (unitários) e `npm run build` limpos

\- \[ ] Testado no Swagger e no site



\## Pendências / dúvidas em aberto

\- Nenhuma.

