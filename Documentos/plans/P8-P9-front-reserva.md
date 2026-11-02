# Plano — P8/P9 Front: Comprar / Reservar / Expirado (Frente B)

## Objetivo
Dar interface ao fluxo Comprar vs Reservar (P2/P3 do Nicolas): botões "Comprar" e
"Reservar" no Bazar (P8) e exibir o status `Expirado` no histórico do comprador (P9).
Torna o diferencial Reserva demonstrável ponta a ponta na tela.

## Escopo
- **Inclui:**
  - P8: botões **Comprar** (qualquer produto) e **Reservar** (só `ItemBazar`) na
    tela de detalhe do Bazar, chamando `POST /api/pedidos` com `tipo`.
  - P8: feedback de sucesso (diferenciando Compra/Reserva pelo `tipo` enviado) e de
    erro (mostrar o `mensagem` do 409).
  - P9 (parte do comprador): incluir `4: 'Expirado'` no mapa de status do
    `HistoricoPedidos.jsx` + estilo do selo.
  - `services/pedidos.js` novo, isolando a chamada de criação (padrão do projeto).
- **NÃO inclui (bloqueado / fora de escopo agora):**
  - P9 visão do **vendedor** (botão "Concluir" + lista): depende de P6/P7 (Nicolas,
    não iniciados).
  - Mostrar "Reserva até dd/mm" no histórico: depende de `GET /api/pedidos` devolver
    `tipo`/`expiraEm` (hoje **não** devolve) — mudança de DTO a combinar.
  - Botão de reservar no `ProdutoCard`/listagem: manter a ação só no detalhe por ora
    (simplicidade); reavaliar depois.

## Decisões de design
- **Entidades/Domain:** nenhuma (só front).
- **Endpoints/API:** consome o já existente `POST /api/pedidos`
  (contrato P2/P3: `{ tipo, itens: [{ produtoId, quantidade }] }`). Sem endpoint novo.
- **Banco/Migrations:** nenhum (colunas Tipo/ExpiraEm/status Expirado já na main via B2).
- **Impacto em concorrência/LGPD:** nenhum novo. O decremento atômico de estoque é o
  mesmo; o front só dispara o POST. Comprador sempre vem do token (não enviamos id).
- **Quantidade:** seletor de quantidade no detalhe, limitado a 1..`quantidadeDisponivel`.
- **Sucesso:** feedback inline no detalhe + link para `/pedidos` (opção A, não redireciona).
- **Acesso ao histórico:** mantido como está — `/pedidos` segue acessível pelo card
  "Últimas compras" do Perfil. Decidimos NÃO adicionar link na navbar (navegação global
  é de outra frente, BRV-147/148, e o acesso pelo Perfil já atende).
- **Trade-offs considerados:**
  - *Botão no detalhe vs no card:* começar só no detalhe (menos superfície, mais
    simples; o detalhe já tem os dados do item para decidir Compra/Reserva).
  - *Como esconder "Reservar" para não-Bazar:* como esta tela (`DetalheBazar`) só
    mostra `ItemBazar`, "Reservar" aparece sempre aqui; a guarda por tipo de produto
    vale quando o botão for para telas mistas (Alimentação) — registrar como futuro.

## Passos de implementação
1. **`services/pedidos.js`** (novo): `criarPedido({ produtoId, quantidade, tipo })`
   → `POST /api/pedidos`. Exporta `TIPO_PEDIDO = { COMPRA: 0, RESERVA: 1 }`.
2. **`DetalheBazar.jsx`**: área de ação com seletor de quantidade (1..disponível) +
   botões Comprar/Reservar; estados enviando/sucesso/erro; só habilita se Disponível
   e estoque > 0. Sucesso = mensagem inline + link para `/pedidos`.
3. **`DetalheBazar.css`**: estilo do seletor, dos botões e das mensagens de feedback.
4. **`HistoricoPedidos.jsx`**: `STATUS[4] = 'Expirado'`.
5. **`HistoricoPedidos.css`**: estilo do selo `--4` (Expirado).

> Acesso ao histórico permanece pelo card "Últimas compras" do Perfil (sem link na navbar).

## Critérios de aceite
- [ ] No detalhe de um ItemBazar disponível, "Comprar" cria pedido (`tipo 0`) e mostra sucesso.
- [ ] "Reservar" cria pedido (`tipo 1`) e mostra sucesso diferenciado.
- [ ] Falha de negócio (409) mostra o `mensagem` da API ao usuário (ex.: estoque insuficiente).
- [ ] Botões desabilitados/ocultos quando o item não está disponível ou sem estoque.
- [ ] Histórico exibe "Expirado" (não "Desconhecido") para pedidos com status 4.
- [ ] `npm run lint` limpo; fluxo validado no `npm run dev`.

## Pendências / dúvidas em aberto
- **[LACUNA SEM DONO] Histórico não distingue Compra de Reserva.** O `GET /api/pedidos`
  (via `HistoricoPedidoDto`) NÃO devolve `Tipo` nem `ExpiraEm`, então toda reserva aparece
  igual a uma compra ("Reservado"). Não está no escopo de ninguém: o P2/P3 (Nicolas)
  exclui telas/DTO; o BRV-89 (histórico) é anterior ao conceito de Reserva; o P9 só previu
  o selo `Expirado`. Mudança pequena (3 pontos: `HistoricoPedidoDto` + `Select` do
  `GetHistoricoAsync` + front). **Decisão pendente com o squad** sobre quem faz — é backend
  (território do Nicolas/Pedido) e o DTO também alimenta o card de pedidos do Perfil (BRV-115).
- **[DEPENDÊNCIA] Status nunca muda de "Reservado":** sem P7 (baixa do vendedor) e P4 (job
  de expiração), nenhum pedido vira `Concluido`/`Expirado`. O selo `Expirado` está pronto,
  mas só fica visível quando o P4 existir.
- TTL de 24h confirmado pelo Henrique?
- "Reservar" deve ir também para o card da listagem do Bazar, ou só no detalhe?