# Plano — Cancelar reserva pelo comprador (Squad B, extensão da Sprint 4)

## Objetivo
Permitir que o comprador cancele uma **reserva** de item do Bazar com um "✕", devolvendo o estoque. O pedido cancelado some da visão do comprador, mas continua visível ao vendedor como Cancelado. Compra (definitiva) não pode ser cancelada.

## Escopo
- **Inclui:** `Pedido.Cancelar()`; `POST /api/pedidos/{id}/cancelar`; cancelamento atômico com devolução única do estoque; filtro que esconde pedidos cancelados do histórico do comprador; botão "✕" com confirmação em `/pedidos`; testes de unidade.
- **NÃO inclui:** cancelar Compra; cancelamento pelo vendedor; notificações; apagar o pedido do banco; mudança de schema; alterar o card "Últimas compras" do Perfil (mostrar Compra vs Reserva), que fica fora desta entrega.

## Decisões de design
- **Entidades/Domain:** `Pedido.Cancelar()` só vale para `Tipo = Reserva` e `Status = Reservado`; qualquer outro caso lança `InvalidOperationException` (vira 409). Atualiza `UpdatedAt`. Usa o status `Cancelado` (3), que já existe.
- **Endpoints/API:** `POST /api/pedidos/{id}/cancelar`, sem corpo. 200 (cancelada), 401 (token sem `sub`), 403 (pedido de outro usuário), 404 (inexistente), 409 (não é Reserva, ou não está mais `Reservado`).
- **Dono:** o comprador vem do token (claim `"sub"`); a checagem de posse vem antes da regra de estado, para não revelar o status a quem não é dono (como no P7).
- **Concorrência:** UPDATE condicional (`WHERE Status = Reservado AND Tipo = Reserva`), no padrão de `TryExpirarAsync`. **Só quem ganha a corrida devolve o estoque**, então o estoque volta uma única vez mesmo se o job de expiração, a checagem lazy, a baixa do vendedor ou um duplo clique disputarem o pedido.
- **Visibilidade:** o pedido cancelado **não é apagado** (a linha fica no banco com status Cancelado, preservando o rastro). `GetByCompradorAsync` passa a filtrar `Status != Cancelado`, então o histórico e a paginação do comprador não o mostram. A lista do vendedor (`GetByVendedorAsync`) **não muda**: ele vê o pedido como Cancelado, sem o botão Concluir.
- **Banco/Migrations:** nenhuma.
- **Frontend:** `cancelarReserva(pedidoId)` em `services/pedidos.js`; "✕" na página de histórico, visível só quando `tipo = 1` e `status = 0`; `window.confirm` antes de enviar; após o sucesso a lista é recarregada; o erro da API é exibido.
- **Trade-offs considerados:**
  - Esconder (filtrar) em vez de apagar: o comprador não vê, mas o vendedor e o rastro de auditoria continuam, e a devolução única do estoque fica segura.
  - POST `/cancelar` em vez de DELETE: o registro é preservado.
  - 403 (e não 404) para pedido de outro usuário: segue o padrão do P7.
  - Um PR com commits separados (plano, domínio, backend, endpoint, front), para facilitar a revisão.

## Passos de implementação
1. Domain: `Pedido.Cancelar()` + testes da regra.
2. Application/Infrastructure: `TryCancelarAsync`, filtro de cancelados em `GetByCompradorAsync`, DTO de resultado, `PedidoService.CancelarReservaAsync` + testes.
3. API: `POST api/pedidos/{id}/cancelar` no `PedidosController`.
4. Front: `cancelarReserva` em `services/pedidos.js`; botão "✕" em `HistoricoPedidos.jsx` e estilo no `.css`.

## Critérios de aceite
- [ ] O comprador cancela a própria reserva; o pedido some do histórico dele
- [ ] O estoque dos itens volta exatamente uma vez
- [ ] O vendedor continua vendo o pedido, como Cancelado, sem botão Concluir
- [ ] Compra, pedido concluído, expirado ou já cancelado devolvem 409
- [ ] Pedido de outro usuário devolve 403; inexistente, 404
- [ ] O "✕" só aparece em reservas ativas
- [ ] `dotnet build`, `dotnet test` (unitários) e `npm run build` limpos
- [ ] Testado no Swagger e no site

## Pendências / dúvidas em aberto
- Nenhuma.