# Plano — Melhorias de Pedidos: tag Tipo no histórico + impedir autocompra

## Objetivo
Duas melhorias no fluxo de pedidos:
1. **(Front)** No histórico do comprador, distinguir visualmente Compra de Reserva
   (tag + "reserva até dd/mm"), já que o backend expõe `tipo`/`expiraEm`.
2. **(Backend + Front)** Impedir que um usuário compre/reserve o próprio produto.

## Escopo
- **Inclui:**
  - M1: tag "Compra"/"Reserva" e data de expiração no `HistoricoPedidos.jsx` (+ CSS).
  - M2 (backend): `PedidoService.CreatePedidoAsync` recusa item cujo dono é o próprio
    comprador, com 409 e mensagem clara; método de apoio no `IProdutoRepository`.
  - M2 (front): esconder o botão de ação no `DetalheBazar` quando o usuário logado é o
    dono do item (UX; a trava real é no backend).
- **NÃO inclui:**
  - Mudança de schema / migration (nenhuma; só leitura de relacionamento existente).
  - Alterar o contrato de resposta dos endpoints de pedido.

## Decisões de design
- **M1 — Entidades/Domain:** nenhuma. `HistoricoPedidoDto` já traz `tipo` e `expiraEm`
  (PR #64 na main). É só exibir no front (espelha o que já fiz na tela do vendedor).
- **M2 — regra de negócio (a trava de verdade) fica no BACKEND:** o front pode esconder
  o botão, mas isso é burlável via API; a garantia é no `PedidoService`.
  - **Como detectar "é meu produto":** `Produto.VendedorId` aponta para `Vendedor.Id`;
    `Vendedor.UsuarioId` aponta para o `Usuario`. Então o produto é do comprador quando
    `produto.Vendedor.UsuarioId == compradorId`. Como `GetByIdAsync` não traz o Vendedor
    (AsNoTracking, sem Include), adiciono um método dedicado no repositório:
    `Task<bool> ProdutoPertenceAoUsuarioAsync(Guid produtoId, Guid usuarioId)`
    (join Produtos→Vendedores, compara UsuarioId). Evita mexer no GetByIdAsync existente.
  - **Onde validar:** dentro do loop de itens em `CreatePedidoAsync`, logo após achar o
    produto e antes de decrementar estoque. Se for do próprio comprador: rollback dos já
    decrementados + 409 "voce nao pode comprar o proprio produto".
- **M2 — front:** o `DetalheBazar` consome `ItemBazarRespostaDto`, que traz `VendedorId`
  (Id do Vendedor, NÃO do Usuario). O front não conhece o `Vendedor.Id` do usuário logado,
  então **não dá para comparar com segurança no front** sem um dado novo. Opções:
    (a) não esconder no front e confiar no 409 do backend (mostra a mensagem) — mais simples;
    (b) expor no detalhe um booleano "ehMeu" calculado no backend.
  **Decisão:** ir de (a) por simplicidade — o backend recusa e a tela já mostra o motivo
  (409). Esconder o botão fica como melhoria futura se o time quiser expor "ehMeu".
- **Impacto concorrência/LGPD:** nenhum. Não altera o decremento atômico; não expõe dados.
- **Trade-offs:** método novo no repositório (consulta barata) vs carregar Vendedor no
  GetByIdAsync (afetaria todos os usos). Escolhido o método dedicado, mais cirúrgico.

## Passos de implementação
1. **Backend — `IProdutoRepository`**: declarar `ProdutoPertenceAoUsuarioAsync`.
2. **Backend — `ProdutoRepository`**: implementar (join Produtos→Vendedores por UsuarioId).
3. **Backend — `PedidoService.CreatePedidoAsync`**: no loop, recusar produto do próprio
   comprador (rollback + 409).
4. **Backend — testes** (`PedidoServiceTests`): caso "comprar o próprio produto é recusado".
5. **Front — `HistoricoPedidos.jsx`/`.css`**: tag Compra/Reserva + "reserva até".

## Critérios de aceite
- [ ] Comprar/reservar o próprio produto retorna 409 com mensagem clara; estoque intacto.
- [ ] Comprar produto de outro vendedor continua funcionando normalmente.
- [ ] Histórico do comprador mostra tag "Compra"/"Reserva"; reservas mostram "reserva até dd/mm".
- [ ] `dotnet build` limpo; novo teste de unidade passando.
- [ ] `npm run lint` limpo (fora o erro pré-existente de Perfil.jsx, alheio).

## Pendências / dúvidas em aberto
- Expor "ehMeu" no detalhe para esconder o botão no front? (deixado como futuro)
- Mensagem exata do 409 (proposta: "Você não pode comprar o próprio produto.").