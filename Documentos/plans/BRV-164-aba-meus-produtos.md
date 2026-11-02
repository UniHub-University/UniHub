# Plano — BRV-164: Aba "Meus Produtos" (vendedor)

## Objetivo
Permitir que o vendedor visualize e gerencie os produtos que publicou, consumindo o
endpoint `GET /api/perfil/vendedor/produtos` (já existente na base desta branch).
Acesso pela área do vendedor (Painel) e pelo card "Vendedor" no Perfil.

## Escopo
- **Inclui:**
  - Nova página `MeusProdutos.jsx` (+ `.css`) na rota `/vendedor/produtos`, protegida
    por `SomenteVendedor`.
  - Service `services/vendedor.js` com `listarMeusProdutos()` → `GET /api/perfil/vendedor/produtos`.
  - Listagem dos produtos do vendedor (nome, tipo, preço, quantidade, status),
    incluindo esgotados/inativos, com estados carregando/erro/vazio.
  - Ações por item: "Editar" (link para `/vendedor/produtos/:id/editar`) e "Excluir"
    (reusa `BotaoExcluirItemBazar`, recarregando a lista no `onExcluido`). Atalho
    "Adicionar produto" (→ `/vendedor/produtos/novo`).
  - Entradas de acesso: atalho "Meus produtos" no `PainelVendedor` e botão "Meus
    produtos" no card Vendedor do `Perfil.jsx`.
- **NÃO inclui:**
  - Backend novo (o endpoint já existe) nem mudança de schema/migration.
  - Paginação (segue o padrão do projeto; a listagem é de um único vendedor).
  - Edição inline ou formulário novo (reusa o `FormularioBazar` existente via rota).
  - Produtos de Alimentação sem gerência de Bazar: a exclusão/edição reusa as rotas
    de item-bazar; itens do tipo "lanche" são exibidos, mas o gerenciamento segue o
    que o backend/rotas já suportam.

## Decisões de design
- **Base:** branch `feat/BRV-164-aba-meus-produtos`, criada de `caf8141`
  (`feat/BRV-XX-meus-produtos-tornar-vendedor`), que já traz o endpoint e o guard.
- **Endpoint consumido:** `GET /api/perfil/vendedor/produtos` → `MeuProdutoDto[]`
  (`id, nome, descricao, preco, quantidadeDisponivel, tipo "bazar"|"lanche", status,
  dataPublicacao`). Autorização por posse via token no backend; o front não envia id.
- **Chamada via wrapper:** novo `services/vendedor.js` usando `api.get` (AGENTS.md §14;
  nunca `fetch` direto). Tratamento de erro via `ApiError`.
- **Reuso:**
  - `BotaoExcluirItemBazar` (já desenhado para listagem: `onExcluido`, `onOcupado`,
    `onSemAutorizacao`).
  - `rotuloStatus` de `services/rotulosProduto.js` para o rótulo de status.
  - Tema/estilo no padrão de `PainelVendedor.css`/`Bazar.css` (variáveis de `index.css`).
- **Rotas:** adicionar `/vendedor/produtos` em `App.jsx`, protegida por
  `SomenteVendedor`, sem conflitar com `/vendedor/produtos/novo` e
  `/vendedor/produtos/:id/editar` (rota exata não colide com as filhas).
- **Exclusão:** ao excluir, remover o item da lista em memória (ou recarregar) para a
  UI refletir sem F5.
- **camelCase:** campos do DTO chegam em camelCase (padrão do projeto).
- **Trade-offs:** reusar o botão de exclusão e a rota de edição em vez de recriar;
  página dedicada (`/vendedor/produtos`) em vez de embutir no Painel, para acesso
  consistente a partir do Painel e do Perfil.

## Passos de implementação
1. `services/vendedor.js` — `listarMeusProdutos()`.
2. `pages/MeusProdutos.jsx` + `MeusProdutos.css` — listagem, estados e ações.
3. `App.jsx` — rota `/vendedor/produtos` (protegida).
4. `pages/PainelVendedor.jsx` — atalho "Meus produtos".
5. `pages/Perfil.jsx` — botão "Meus produtos" no card Vendedor.
6. Validação: `npm run lint` e `npm run build`.

## Critérios de aceite
- [ ] `/vendedor/produtos` lista os produtos do vendedor autenticado (inclui
      esgotados/inativos), com estados carregando/erro/vazio.
- [ ] Cada item permite Editar (→ formulário) e Excluir (com confirmação, atualizando
      a lista); atalho para Adicionar produto.
- [ ] Acesso pela área do vendedor (atalho no Painel) e pelo Perfil (card Vendedor).
- [ ] Rota protegida por `SomenteVendedor`; não vendedor é bloqueado.
- [ ] `npm run lint` e `npm run build` limpos (ressalva: erro pré-existente de
      `set-state-in-effect` no efeito do voluntário em `Perfil.jsx`, fora do escopo —
      ver BRV-XX).

## Pendências / dúvidas em aberto
- Número real da task no board (branch usa BRV-164; confirmar ID).
- Depende do merge da BRV-XX (endpoint) antes da integração final na main.
