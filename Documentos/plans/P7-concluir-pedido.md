\# Plano — P7: `POST /api/pedidos/{id}/concluir` (Squad B, extensão da Sprint 4)



\## Objetivo

Permitir que o vendedor dê baixa manual em um pedido (Compra ou Reserva) que contém produtos seus, mudando o status de `Reservado` para `Concluido`. Destrava o botão "Concluir" da visão do vendedor (P9, GVBVaz).



\## Escopo

\- \*\*Inclui:\*\* `Pedido.Concluir()` (regra de transição); endpoint `POST /api/pedidos/{id}/concluir`; verificação de dono (vendedor do pedido); baixa atômica no repositório; testes de unidade.

\- \*\*NÃO inclui:\*\* lista de pedidos do vendedor (P6, PR seguinte); job de expiração (P4) e checagem lazy (P5), do Henrique; telas (P9); cancelamento; mudança de schema.



\## Decisões de design

\- \*\*Entidades/Domain:\*\* `Pedido.Concluir()` só vale a partir de `Reservado`; em qualquer outro status lança `InvalidOperationException`. Atualiza `UpdatedAt`. A regra fica na entidade (AGENTS.md).

\- \*\*Endpoints/API:\*\* `POST /api/pedidos/{id}/concluir`, sem corpo. Respostas: 200 (concluído), 404 (pedido não existe), 403 (o usuário não é vendedor de nenhum produto do pedido), 409 (pedido não está `Reservado`, ou mudou de estado durante a operação), 401 (token inválido).

\- \*\*Dono:\*\* derivado do token (claim `"sub"` = `Usuario.Id`). `Produto.VendedorId` é o id do `Vendedor`, então a verificação liga `Produto.Vendedor.UsuarioId` ao `sub`. Nunca vem do corpo ou da query (anti-IDOR).

\- \*\*Banco/Migrations:\*\* nenhuma; não muda schema.

\- \*\*Concorrência:\*\* o Service valida a regra (`Concluir()`) e o repositório grava com UPDATE condicional (`WHERE Status = Reservado`). Se o job de expiração (P4) ou outra requisição mudar o pedido no meio, o UPDATE afeta 0 linhas e a resposta é 409. Nunca sobrescreve `Expirado`.

\- \*\*Trade-offs considerados:\*\*

&#x20; - 403 (e não 404) quando o pedido existe mas não é do vendedor: segue o padrão do `ItensBazarController`; os ids são GUIDs não adivinháveis.

&#x20; - A baixa vale para Compra e Reserva: a Compra também nasce `Reservado` e só vira `Concluido` com a entrega.

&#x20; - Validar na entidade e gravar com UPDATE condicional (em vez de só um dos dois): a entidade dá a mensagem de regra, o UPDATE garante a corrida.



\## Passos de implementação

1\. Domain: `Pedido.Concluir()`.

2\. Application: `IPedidoRepository` (`PedidoTemItemDoVendedorAsync`, `TryConcluirAsync`); DTO `ConcluirPedidoResultDto`; `PedidoService.ConcluirPedidoAsync`.

3\. Infrastructure: implementação no `PedidoRepository`.

4\. API: endpoint no `PedidosController`.

5\. Testes de unidade (404, 403, 409 por status, 409 por corrida, sucesso).



\## Critérios de aceite

\- \[ ] Vendedor dono conclui pedido `Reservado` e o status vira `Concluido`

\- \[ ] Outro usuário recebe 403; pedido inexistente, 404

\- \[ ] Pedido já `Concluido`, `Cancelado` ou `Expirado` devolve 409

\- \[ ] `dotnet build` e `dotnet test` (unitários) limpos

\- \[ ] Testado no Swagger com dois usuários



\## Pendências / dúvidas em aberto

\- Reserva com `ExpiraEm` vencido mas ainda `Reservado` (job ainda não rodou): hoje pode ser concluída. Decidir com o Henrique se a checagem lazy (P5) deve bloquear.

