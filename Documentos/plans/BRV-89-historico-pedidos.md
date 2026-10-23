# Plano — BRV-89 Histórico de pedidos

## Objetivo
Permitir que o usuário autenticado consulte os próprios pedidos (histórico), do mais recente para o mais antigo, com paginação desde o início e formato de resposta alinhado com a Squad 1. Entrega o endpoint `GET /api/pedidos` e a tela de histórico, reaproveitando `Pedido` e `ItemPedido`.

## Escopo
- **Inclui:** DTOs do histórico, consulta paginada por comprador no repositório, método de serviço com validação de `page`/`pageSize`, endpoint `GET /api/pedidos`, tela de histórico (dados mock enquanto a autenticação do front não estiver pronta).
- **NÃO inclui:** detalhe de um pedido, filtros/busca (BRV-6), cancelamento ou alteração de status, mudança de schema, refatoração de código existente fora da tarefa.

## Decisões de design
- **Entidades/Domain:** nenhuma entidade nova ou alterada; reaproveita `Pedido`, `ItemPedido` e `Produto`.
- **Endpoints/API:** `GET /api/pedidos?page=1&pageSize=10` no `PedidosController`, autenticado. Resposta proposta: `{ items, page, pageSize, totalItems, totalPages }`. `pageSize` padrão 10 e máximo 50 (proposta; a alinhar com a Squad 1 e a BRV-6).
- **Banco/Migrations:** sem mudança de schema, logo sem migration (o índice em `CompradorId` já existe no snapshot). A confirmar com o Guardião de Migrations (Squad 3 nesta sprint).
- **Impacto em concorrência/LGPD:** não toca estoque (só leitura). O dono vem do token (claim `"sub"`), nunca da requisição (evita IDOR). O DTO não expõe `Usuario`.
- **Trade-offs considerados:**
  - Paginação com `Skip/Take` (simples, suficiente para o volume) em vez de cursor.
  - `ProdutoNome` dentro do item para a tela não precisar de outra consulta; fallback "Produto indisponivel" se o produto não existir mais.
  - `PrecoUnitarioNaCompra` no item, para o histórico não mudar se o preço do produto mudar.
  - `AsNoTracking` por ser consulta somente leitura; ordenação por `CreatedAt` e `Id` (decrescente) para páginas estáveis.
  - Repositório devolve `(pedidos, total)` com uma contagem separada.

## Passos de implementação
1. Domain: nenhuma alteração.
2. Application: DTOs `HistoricoPedidoDto` e `HistoricoItemPedidoDto`; `IPedidoRepository.GetByCompradorAsync`; `PedidoService.GetHistoricoAsync` (valida `page`/`pageSize`, mapeia e calcula o total).
3. Infrastructure: `PedidoRepository.GetByCompradorAsync` (filtro por `CompradorId`, `Include` de itens e produto, paginação).
4. API: `GET api/pedidos` no `PedidosController` (claim `"sub"`), monta a resposta paginada. DI já registrado no `Program.cs`, sem alteração prevista.
5. Frontend: tela de histórico em pasta isolada, com dados mock até a autenticação da Squad 1.

## Critérios de aceite
- [x] DTOs, contrato do repositório, consulta paginada e método de serviço implementados
- [x] `dotnet build` limpo nas camadas já entregues
- [ ] `GET /api/pedidos` devolve só os pedidos do usuário autenticado
- [ ] Usuário A não vê pedidos do usuário B (testado no Swagger)
- [ ] `pageSize` acima do máximo ou inválido é ajustado; `page` inválido cai em 1
- [ ] Resposta no formato combinado com a Squad 1
- [ ] Tela de histórico exibe a lista paginada
- [ ] Migration descartada ou confirmada com o Guardião
- [ ] `dotnet build` limpo no PR final

## Pendências / dúvidas em aberto
- Formato final da resposta paginada e valores de `pageSize` (alinhar com a Squad 1 e a BRV-6).
- Confirmação do Guardião de Migrations de que não há migration.
- Autenticação no front (Squad 1) para trocar os dados mock pela API.
- Remover o arquivo marcador `Documentos/historico-pedidos.md` antes de marcar o PR como "Ready for review".
