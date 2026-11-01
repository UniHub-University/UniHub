\# Plano — P2/P3 Comprar vs Reservar (Squad B, extensão da Sprint 4)



\## Objetivo

Permitir que o pedido seja criado como \*\*Compra\*\* (definitiva) ou \*\*Reserva\*\* (temporária, só para `ItemBazar`), e que a Reserva já nasça com a data de vencimento (`ExpiraEm`) preenchida. Destrava o front de Comprar/Reservar (P8) e a expiração (P4/P5, Henrique).



\## Escopo

\- \*\*Inclui:\*\* campo `Tipo` em `CreatePedidoDto`; validação de `Tipo` e da regra "Reserva só para `ItemBazar`" no `PedidoService`; `ExpiraEm = agora (UTC) + TTL` para Reserva; testes de unidade.

\- \*\*NÃO inclui:\*\* job de expiração (P4) e checagem lazy (P5), do Henrique; concluir pedido (P7); pedidos do vendedor (P6); telas (P8/P9); mudança de schema.



\## Decisões de design

\- \*\*Entidades/Domain:\*\* nenhuma alteração. `Tipo` e `ExpiraEm` já existem em `Pedido` (B2, já na `main`).

\- \*\*Endpoints/API:\*\* `POST /api/pedidos` passa a aceitar `{ "itens": \[...], "tipo": 0|1 }` (0 = Compra, 1 = Reserva). O `tipo` é opcional e o padrão é Compra, então clientes e testes atuais continuam válidos. O contrato de resposta (`PedidoResultDto`) não muda.

\- \*\*Banco/Migrations:\*\* sem migration (as colunas vieram na migration do B2).

\- \*\*Impacto em concorrência/LGPD:\*\* o decremento atômico de estoque (UPDATE condicional) e o rollback continuam como estão; nada novo é exposto. O comprador continua vindo do token (claim `"sub"`).

\- \*\*Trade-offs considerados:\*\*

&#x20; - `Tipo` com valor padrão no record, em vez de um DTO novo, para não quebrar chamadas existentes.

&#x20; - TTL como constante pública `PedidoService.TtlReserva` (24 h, \*\*a confirmar com o Henrique\*\*), para o job do P4 reutilizar o mesmo valor.

&#x20; - Todo pedido continua nascendo `Reservado` (Compra e Reserva), para a baixa do P7 valer para os dois.

&#x20; - Valor de `Tipo` inválido (fora do enum) é recusado com `Enum.IsDefined`, evitando erro 500 (AGENTS.md, seção 4).

&#x20; - Falhas continuam retornando 409 pelo caminho existente do controller, para não mudar o contrato.



\## Passos de implementação

1\. Domain: nenhuma alteração.

2\. Application: `CreatePedidoDto(Itens, Tipo = Compra)`; em `PedidoService.CreatePedidoAsync`, validar `Tipo`, recusar Reserva de produto que não seja `ItemBazar` (fazendo o rollback do estoque já decrementado), preencher `Tipo` e `ExpiraEm` no `Pedido`.

3\. Infrastructure: nenhuma alteração.

4\. API: nenhuma alteração prevista no controller.

5\. Testes: novos casos em `PedidoServiceTests`.



\## Critérios de aceite

\- \[ ] Pedido sem `tipo` continua sendo Compra, com `ExpiraEm` nulo

\- \[ ] Reserva de `ItemBazar` grava `Tipo = Reserva` e `ExpiraEm ≈ agora + TTL`

\- \[ ] Reserva de `Alimento` é recusada e nenhum estoque fica decrementado

\- \[ ] `tipo` inválido é recusado sem erro 500

\- \[ ] `dotnet build` e `dotnet test` (unitários) limpos

\- \[ ] Testado no Swagger: Compra, Reserva e Reserva de produto inválido



\## Pendências / dúvidas em aberto

\- Valor definitivo do TTL (proposta: 24 h), a confirmar com o Henrique.

\- Alinhar com o GVBVaz o formato do `tipo` enviado pelo front (número 0 ou 1).

