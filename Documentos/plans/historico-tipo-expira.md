\# Plano — `tipo` e `expiraEm` no histórico de pedidos (Squad B, extensão da Sprint 4)



\## Objetivo

Fazer o `GET /api/pedidos` devolver o tipo do pedido (Compra ou Reserva) e o prazo da reserva, para o front diferenciar os dois nos selos do histórico e do card do Perfil. Hoje toda reserva aparece igual a uma compra ("Reservado").



\## Escopo

\- \*\*Inclui:\*\* campos `Tipo` e `ExpiraEm` no `HistoricoPedidoDto`; preenchimento no `PedidoService.GetHistoricoAsync`; teste de unidade.

\- \*\*NÃO inclui:\*\* alterar telas (selo no `HistoricoPedidos.jsx` e no `Perfil.jsx`); P6/P7 (PRs seguintes); job de expiração (P4) e checagem lazy (P5), do Henrique; mudança de schema.



\## Decisões de design

\- \*\*Entidades/Domain:\*\* nenhuma mudança; `Pedido` já tem `Tipo` e `ExpiraEm` (P2/P3).

\- \*\*Endpoints/API:\*\* `GET /api/pedidos` passa a devolver, em cada pedido, `tipo` (0 = Compra, 1 = Reserva, número, como o `status`) e `expiraEm` (data ISO ou `null` na Compra). Os campos entram no \*\*fim\*\* do record `HistoricoPedidoDto`, então nada que o front usa hoje muda.

\- \*\*Banco/Migrations:\*\* nenhuma; as colunas já existem.

\- \*\*Concorrência/LGPD:\*\* sem impacto; o comprador continua vindo do token e os dados novos são do próprio pedido.

\- \*\*Trade-offs considerados:\*\*

&#x20; - Campos no fim do record, para não quebrar quem constrói ou lê o DTO.

&#x20; - Enum como número, igual ao `status`, mantendo uma convenção só na API.

&#x20; - PR separado dos P6/P7, por ser pequeno e destravar o front (Gabriel) mais cedo.



\## Passos de implementação

1\. `HistoricoPedidoDto`: acrescentar `Tipo` e `ExpiraEm` no fim.

2\. `PedidoService.GetHistoricoAsync`: preencher os dois campos a partir do `Pedido`.

3\. Teste de unidade no `PedidoServiceTests` cobrindo Compra (`ExpiraEm` nulo) e Reserva (`ExpiraEm` preenchido).



\## Critérios de aceite

\- \[ ] `GET /api/pedidos` devolve `tipo` e `expiraEm` em cada pedido

\- \[ ] Compra vem com `expiraEm: null`; Reserva, com data

\- \[ ] Os campos que o front usa hoje continuam iguais

\- \[ ] `dotnet build` e `dotnet test` (unitários) limpos

\- \[ ] Conferido no Swagger



\## Pendências / dúvidas em aberto

\- Texto dos selos no front (sugestão: "Reserva até dd/mm" e "Aguardando retirada"), a combinar com o GVBVaz.

