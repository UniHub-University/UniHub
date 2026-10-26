# Plano — BRV-89 Histórico de pedidos

## Objetivo
Permitir que o usuário autenticado consulte os próprios pedidos (histórico), do mais recente para o mais antigo, com paginação e formato de resposta alinhado com a Squad 1. Entrega o endpoint `GET /api/pedidos` e a tela de histórico, reaproveitando `Pedido` e `ItemPedido`.

## Escopo
- **Inclui:** DTOs do histórico, consulta paginada por comprador no repositório, método de serviço com validação de `page`/`pageSize`, endpoint `GET /api/pedidos`, tela de histórico consumindo a API real (rota protegida `/pedidos`).
- **NÃO inclui:** detalhe de um pedido, filtros/busca (BRV-6), cancelamento ou alteração de status, mudança de schema, link de acesso na Home/menu (depende da entrega de navegação, BRV-147), refatoração de código existente fora da tarefa.

## Decisões de design
- **Entidades/Domain:** nenhuma entidade nova ou alterada; reaproveita `Pedido`, `ItemPedido` e `Produto`.
- **Endpoints/API:** `GET /api/pedidos?page=1&pageSize=10` no `PedidosController`, autenticado. Resposta no contrato `PaginatedResultDto<T>` da Squad 1 (BRV-6): `{ items, page, pageSize, totalItems, totalPages }`, reaproveitado via cherry-pick do commit do Lucas Gazale (autoria preservada). `pageSize` padrão 10 e máximo 50; `page` mínimo 1.
- **Banco/Migrations:** sem mudança de schema, logo sem migration (o índice em `CompradorId` já existe no snapshot). Guardião de Migrations (Squad 3) ciente.
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
4. API: `GET api/pedidos` no `PedidosController` (claim `"sub"`), monta `PaginatedResultDto`. DI já registrado, sem alteração no `Program.cs`.
5. Frontend: `HistoricoPedidos.jsx` + CSS e rota protegida `/pedidos` no `App.jsx`, consumindo `GET /api/pedidos` com o JWT já enviado pelo `api.js`.

## Critérios de aceite
- [x] DTOs, contrato do repositório, consulta paginada e método de serviço implementados
- [x] `GET /api/pedidos` devolve os pedidos do usuário autenticado (testado no Swagger; sem token retorna 401)
- [x] `pageSize` acima do máximo ou inválido é ajustado (500→50, 0→10); `page` inválido cai em 1
- [x] Resposta no formato combinado com a Squad 1 (`PaginatedResultDto`)
- [x] Tela de histórico exibe a lista paginada com a API real (testada localmente)
- [x] Migration descartada: sem mudança de schema; Guardião ciente
- [x] `dotnet build` limpo e checks do PR verdes (build, Docker, model changes, Vercel)
- [ ] Usuário A não vê pedidos do usuário B (teste com token de outro `sub`)

## Pendências / observações
- Teste de isolamento entre usuários ainda não executado (a garantia hoje vem do código: o comprador vem só do token).
- Link "Meus pedidos" na Home/menu fica para a entrega de navegação (BRV-147); até lá a tela é acessada por `/pedidos`.
- Testes automatizados da suíte ficam com a Squad de qualidade.
