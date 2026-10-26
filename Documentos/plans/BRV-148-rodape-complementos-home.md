# Plano — BRV-148 Frontend: Rodapé Global + Complementos na Home

> Issue complementar à BRV-147 (dupla). Branch sugerida:
> `feat/BRV-148-rodape-complementos-home`, criada a partir do estado da branch
> da BRV-147. Commits convencionais com o código `(BRV-148)`. PR pequeno mirando
> a branch de integração da sprint. Nenhum segredo versionado.

## Objetivo
Adicionar um rodapé global presente em todas as telas protegidas do UniHub (via
`LayoutApp`), reduzir duplicação extraindo os rótulos de enum para um módulo
compartilhado, e complementar a Home com ordenação por mais recente, contador de
resultados e badge de condição no card. Entrega complementar à BRV-147.

## Escopo
- **Inclui:**
  - Rodapé global com "UniHub" em destaque e centralizado, contato
    `time.unihub@gmail.com` (mailto), links de navegação espelhando o header e
    ano corrente.
  - Rodapé integrado no ponto único `LayoutApp`, presente em todas as telas
    protegidas.
  - Extração dos mapas de rótulo de status/condição (antes duplicados em
    `Home.jsx` e `DetalheBazar.jsx`) para `services/rotulosProduto.js`.
  - Extração da lista de navegação (`LINKS_NAV`) para `components/navegacao.js`,
    compartilhada entre header e rodapé.
  - Complementos na Home: ordenação por `dataPublicacao` desc, contador de
    resultados e badge de condição no card.
- **NÃO inclui:**
  - Rodapé na tela pública de login (`BoasVindas`).
  - Testes automatizados / Vitest (follow-up).
  - Alterações no `DetalheBazar` além da refatoração de rótulos.
  - Qualquer mudança no backend (inclui o endpoint de Alimentação — ver pendências).

## Decisões de design
- **Entidades/Domain:** nenhuma (frontend only).
- **Endpoints/API:** nenhum novo; reusa `services/anuncios.js` e `services/api.js`.
- **Banco/Migrations:** não se aplica.
- **Impacto em concorrência/LGPD:** nenhum (leitura de dados não-sensíveis do Bazar).
- **camelCase:** os campos da API chegam em camelCase (`item.id`, `item.preco`,
  `item.dataPublicacao`, `item.condicao`, `item.status`) — todo código novo segue
  esse padrão já adotado pela dupla.
- **Ponto único do rodapé:** integrado no `LayoutApp` (após o `<main>`), não em
  cada página — garante presença em todas as telas protegidas, atuais e futuras,
  com uma só edição.
- **Reuso da navegação:** `LINKS_NAV` extraído para `components/navegacao.js` e
  consumido no header e no rodapé, evitando divergência entre os dois conjuntos.
- **Rótulos compartilhados:** `services/rotulosProduto.js` como fonte única,
  mantendo o fallback defensivo para chaves string e valor desconhecido (valores
  do backend são inteiros: ProdutoStatus 0/1/2, CondicaoItem 0/1/2, pois a API
  não usa `JsonStringEnumConverter`). `rotuloCondicao` retorna `null` quando a
  condição não é informada, deixando a UI decidir se exibe o badge.
- **Sticky footer:** `.layout` já é flex-column com `min-height:100vh` e `<main>`
  com `flex:1`; o `Rodape` usa `margin-top:auto` para cair no fim em páginas curtas.
- **Tema:** reusa variáveis de `index.css` (`--cor-verde`, `--cor-tan`,
  `--cor-creme`, `--cor-superficie`, fonte de título, `--sombra`).
- **Trade-offs:** extrair listas/rótulos para módulos compartilhados em vez de
  duplicar; CSS puro + variáveis de tema, sem introduzir lib nova.

## Passos de implementação
1. **Navegação compartilhada:** `src/components/navegacao.js` exporta `LINKS_NAV`;
   `LayoutApp.jsx` passa a importá-lo (comportamento do header inalterado).
2. **Rodapé:** `src/components/Rodape.jsx` + `Rodape.css` (`<footer>` semântico,
   marca centralizada, `mailto:`, links de `LINKS_NAV`, ano via
   `new Date().getFullYear()`); integrado no `LayoutApp` após o `<main>`.
3. **Rótulos compartilhados:** `src/services/rotulosProduto.js` com `rotuloStatus`
   e `rotuloCondicao`; `Home.jsx` e `DetalheBazar.jsx` refatorados para importá-los.
4. **Complementos na Home:** `ordenarPorMaisRecente` (por `dataPublicacao` desc,
   com fallback estável), contador de resultados reativo à busca e badge de
   condição no card (estilos em `Home.css` alinhados ao badge de categoria).
5. **Validação:** `npm install`, `npm run lint` e `npm run build`.

## Critérios de aceite
- [x] Rodapé global com "UniHub" centralizado em destaque, `mailto:time.unihub@gmail.com`,
      links de navegação e ano corrente, visível em todas as telas protegidas.
- [x] Rodapé fica no fim da viewport mesmo em páginas curtas (sticky footer).
- [x] Rodapé NÃO aparece na tela de login pública.
- [x] Rótulos de status/condição vêm de `services/rotulosProduto.js` único; sem
      duplicação em Home e DetalheBazar; comportamento visual inalterado.
- [x] Home ordena por `dataPublicacao` desc, exibe contador de resultados reativo
      à busca e mostra badge de condição no card.
- [x] Estados carregando/erro/vazio/busca-sem-resultado continuam corretos.
- [x] `npm run build` e `npm run lint` limpos.

## Pendências / dúvidas em aberto
- **Alimentação na Home (follow-up):** o feed hoje só traz o Bazar porque o
  endpoint `GET /api/alimentos` NÃO existe no backend (só há `ItensBazarController`).
  Para a Home exibir também os alimentos mais recentes, é necessário primeiro criar
  esse endpoint (backend, fora do escopo desta issue) e depois agregar a fonte em
  `services/anuncios.js` — a Home e os complementos (ordenação/contador/badge) já
  ficam prontos para unificar as duas fontes sem retrabalho na UI.
- **Testes automatizados (follow-up):** Vitest + React Testing Library para cobrir
  `anuncios.js`, a lógica de busca/ordenação da Home e o `Rodape` (previsto para a
  Sprint 4, seção 12 do AGENTS.md).
- **Ambiente:** o projeto está sob `OneDrive`, o que causou erros `EPERM/ENOTEMPTY`
  no primeiro `npm install` (OneDrive travando arquivos em `node_modules`). Um
  `rmdir` + reinstalação resolveu. Vale registrar caso ocorra com o squad.
