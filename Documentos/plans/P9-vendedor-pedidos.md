# Plano — P9 (parte vendedor): Pedidos recebidos + Concluir

## Objetivo
Dar ao vendedor uma tela para ver os pedidos que contêm produtos dele (P6) e dar
baixa manual num pedido reservado, marcando-o como Concluído (P7). Fecha a parte
do P9 que estava bloqueada pelos endpoints de backend (agora na main).

## Escopo
- **Inclui:**
  - Tela "Pedidos recebidos" do vendedor, consumindo `GET /api/pedidos/vendedor` (paginado).
  - Botão **Concluir** em cada pedido ainda `Reservado`, chamando `POST /api/pedidos/{id}/concluir`.
  - Card de atalho "Pedidos recebidos" no `PainelVendedor` + rota `/vendedor/pedidos`.
  - Reuso de `services/pedidos.js`: novos `listarPedidosDoVendedor()` e `concluirPedido(id)`.
- **NÃO inclui:**
  - Alterar os endpoints/DTOs do backend (já prontos — P6/P7 na main).
  - Filtros/busca na lista de pedidos do vendedor (só paginação por ora).
  - Expor qualquer dado do comprador além do NOME (LGPD — ver abaixo).
  - Mudar as telas existentes de Meus Produtos / Adicionar produto (Squad C).

## Decisões de design
- **Onde fica:** novo card no `PainelVendedor` → rota protegida `/vendedor/pedidos`
  (dentro de `<SomenteVendedor>`, como as demais rotas de vendedor). Encaixa no padrão
  de cards de atalho que o painel já usa (não reinventa layout).
- **Endpoints/API consumidos (já na main):**
  - `GET /api/pedidos/vendedor?page=&pageSize=` → `PaginatedResultDto<PedidoVendedorDto>`:
    `{ pedidoId, status, tipo, createdAt, expiraEm, compradorNome, itens[], total }`.
  - `POST /api/pedidos/{id}/concluir` (sem corpo) → `{ status, mensagem }`;
    HTTP 200 ok, 403 não é vendedor do pedido, 404 inexistente, 409 não está mais Reservado.
- **LGPD:** o backend já devolve só `compradorNome` (sem e-mail/telefone/foto/id). O front
  exibe **apenas o nome** — nunca inventar/solicitar outros dados do comprador.
- **Botão Concluir:** só aparece quando `status === Reservado (0)`. Após 200, atualiza a
  tela (recarrega a página atual da lista) para o pedido refletir o novo status.
- **Erros do Concluir:** 403/404/409 → mostrar a `mensagem` da API; 409 costuma significar
  "o pedido mudou de estado" (ex.: já concluído ou expirado) — recarregar a lista nesse caso.
- **Reuso:** segue o padrão do projeto — toda chamada pelo `services/api.js` via
  `services/pedidos.js`; status traduzidos reusando o mesmo mapa de `HistoricoPedidos`
  (0..4) para consistência visual.
- **Trade-offs:**
  - Card novo no painel (consistente) vs link solto — escolhido o card.
  - Recarregar a página da lista após concluir (simples, sempre correto) vs mutação local
    do item (mais "fluido", mais código) — escolhido recarregar, pela simplicidade.

## Passos de implementação
1. **`services/pedidos.js`**: adicionar `listarPedidosDoVendedor({ page, pageSize })`
   (`GET /api/pedidos/vendedor`) e `concluirPedido(pedidoId)` (`POST .../concluir`).
2. **`pages/PedidosVendedor.jsx`** (novo) + **`.css`**: lista paginada; cada pedido mostra
   comprador (só nome), data, tipo (Compra/Reserva), itens, total, status; botão Concluir
   quando Reservado; estados carregando/erro/vazio.
3. **`PainelVendedor.jsx`**: adicionar card "Pedidos recebidos" → `/vendedor/pedidos`.
4. **`App.jsx`**: rota `/vendedor/pedidos` dentro de `<SomenteVendedor>` + `<Titulo>`.

## Critérios de aceite
- [ ] Vendedor acessa "Pedidos recebidos" pelo painel e vê os pedidos com seus produtos.
- [ ] Cada pedido mostra só o NOME do comprador (nada mais — LGPD).
- [ ] Itens/Total refletem apenas os produtos deste vendedor.
- [ ] Botão Concluir aparece só em pedido Reservado; ao concluir, o status atualiza para Concluído.
- [ ] Erros (403/404/409) exibem mensagem amigável; 409 recarrega a lista.
- [ ] Paginação funciona (mesmo contrato do histórico).
- [ ] Quem não é vendedor recebe lista vazia (backend já garante) — a tela trata o vazio.
- [ ] `npm run lint` limpo; fluxo testado no `npm run dev`.

## Pendências / dúvidas em aberto
- Alinhar com o Squad C (dono do `PainelVendedor`) a inclusão do 4º card (mudança aditiva).
- Mostrar `expiraEm` ("reserva até dd/mm") na lista do vendedor também? (o DTO já traz)
- Texto/ícone do card: "Pedidos recebidos" com ícone de lista/caixa (react-icons/fi).