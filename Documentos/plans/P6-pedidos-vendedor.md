\# Plano — P6: `GET /api/pedidos/vendedor` (Squad B, extensão da Sprint 4)



\## Objetivo

Listar, para o vendedor autenticado, os pedidos que contêm produtos dele, para a visão do vendedor (P9, GVBVaz) mostrar quem comprou o quê e permitir a baixa (P7).



\## Escopo

\- \*\*Inclui:\*\* `GET /api/pedidos/vendedor?page=1\&pageSize=10`, paginado no contrato `PaginatedResultDto`; consulta no repositório; DTOs de resposta; testes de unidade.

\- \*\*NÃO inclui:\*\* telas (P9); filtros por status ou tipo; job de expiração (P4) e checagem lazy (P5), do Henrique; mudança de schema.



\## Decisões de design

\- \*\*Entidades/Domain:\*\* nenhuma mudança.

\- \*\*Endpoints/API:\*\* `GET /api/pedidos/vendedor?page=1\&pageSize=10`. Respostas: 200 (lista, possivelmente vazia), 401 (token sem `sub`). Contrato `PaginatedResultDto` (`items`, `page`, `pageSize`, `totalItems`, `totalPages`), mais recentes primeiro, `pageSize` padrão 10 e máximo 50, mesmas regras do histórico do comprador.

\- \*\*Cada pedido traz:\*\* `pedidoId`, `status`, `tipo`, `createdAt`, `expiraEm`, `compradorNome`, `itens` (só os itens \*\*deste\*\* vendedor: `produtoId`, `produtoNome`, `quantidade`, `precoUnitarioNaCompra`) e `total` (soma só desses itens).

\- \*\*Dono:\*\* derivado do token (claim `"sub"` = `Usuario.Id`). `Produto.VendedorId` é o id do `Vendedor`, então o filtro liga `Produto.Vendedor.UsuarioId` ao `sub`. Nunca vem da query ou do corpo (anti-IDOR).

\- \*\*LGPD:\*\* do comprador, só `compradorNome`. Sem e-mail, telefone, foto ou id do usuário.

\- \*\*Banco/Migrations:\*\* nenhuma; consulta somente leitura (`AsNoTracking`).

\- \*\*Sem perfil de vendedor:\*\* o usuário que não é vendedor recebe lista vazia (200), não erro.

\- \*\*Trade-offs considerados:\*\*

&#x20; - O vendedor vê só os itens dos produtos dele em pedidos com vários vendedores, e o total reflete só a parte dele, para não expor o que outro vendedor vendeu.

&#x20; - Reaproveitar o mesmo padrão de paginação do histórico, sem refatorar o que já funciona.

&#x20; - Lista vazia (200) em vez de 403 para quem não é vendedor, para o front tratar um caso só.



\## Passos de implementação

1\. Application: DTOs `PedidoVendedorDto` e `PedidoVendedorItemDto`.

2\. Application: `IPedidoRepository.GetByVendedorAsync` e `PedidoService.GetPedidosDoVendedorAsync`.

3\. Infrastructure: implementação no `PedidoRepository`.

4\. API: endpoint `GET api/pedidos/vendedor` no `PedidosController`.

5\. Testes de unidade (só itens do vendedor e total correto; nome do comprador; paginação; lista vazia).



\## Critérios de aceite

\- \[ ] O vendedor lista só pedidos com produtos seus, paginados, do mais recente ao mais antigo

\- \[ ] Cada pedido mostra só os itens do vendedor e o total dessa parte

\- \[ ] O comprador aparece apenas pelo nome (sem e-mail nem telefone)

\- \[ ] Usuário sem perfil de vendedor recebe lista vazia

\- \[ ] `dotnet build` e `dotnet test` (unitários) limpos

\- \[ ] Testado no Swagger com dois usuários



\## Pendências / dúvidas em aberto

\- Confirmar com o GVBVaz os nomes dos campos (`compradorNome`, `itens`, `total`).

\- Filtro por status (ex.: só `Reservado`) pode ser pedido pela tela depois.

