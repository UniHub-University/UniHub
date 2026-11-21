# Plano — BRV-232 (extensão Sprint 5, frente B3): UI do Painel Admin de Moderação

> Issue #115. Tela React de moderação, restrita a administradores, que lista
> denúncias (com nomes já resolvidos) e oferece ações por denúncia: mudar o status,
> suspender/reativar o usuário denunciado e inativar o produto denunciado. Consome
> um MOCK de denúncias (backend da frente B2 ainda não existe) e integra depois sem
> mudar a UI.

## Objetivo
Entregar o painel de moderação do admin, com proteção em 2 camadas (a UI
esconde/bloqueia para não-admin **e** o backend já exige `[Authorize(Roles="Admin")]`),
feedback de sucesso/erro por ação e tratamento de erro ramificado por status HTTP.

## Escopo
- **Inclui:**
  - Guard de admin (`SomenteAdmin`) espelhando o `PerfilVendedor`.
  - Service de moderação (`moderacao.js`): mock de denúncias + ações reais de
    usuário/produto.
  - Rótulos dos enums de denúncia (`rotulosDenuncia.js`).
  - Página `PainelAdmin` (+ CSS) com listagem, ações e feedback.
  - Rota `/moderacao` e link de navegação só para admin.
- **NÃO inclui (fora de escopo da task):**
  - Backend de denúncias (`GET /api/admin/denuncias`, `PATCH .../status`) — frente B2.
  - UI de criar denúncia — frente B1.
  - "Minhas denúncias" / `/minhas-denuncias` — stretch de outra pessoa.
  - Migration / mudança de schema (tabela `Denuncias` já existe).
  - Notificar o denunciado (confidencial — LGPD).

## Base da branch
- Branch: **`feat/BRV-232-UI-painel-admin`**, criada da **`main`** (`08dddc4`).

## Arquivos criados / editados
Criados:
- `Frontend/src/auth/SomenteAdmin.jsx`
- `Frontend/src/services/moderacao.js`
- `Frontend/src/services/rotulosDenuncia.js`
- `Frontend/src/pages/PainelAdmin.jsx`
- `Frontend/src/pages/PainelAdmin.css`

Editados (mínimo, pontos de conflito entre squads — AGENTS §9):
- `Frontend/src/App.jsx` — rota `/moderacao`.
- `Frontend/src/components/LayoutApp.jsx` — link "Moderação" só para admin + provider.

## Decisões de design, escolhas e trade-offs

### 1. Fonte da verdade do guard de admin: `GET /api/perfil`, não o AuthContext
O objeto `usuario` do AuthContext guarda só `{ nome }` — **não** carrega a role. O
`PerfilRespostaDto` de `/api/perfil` traz `role` como string. Por isso o
`SomenteAdmin` consulta `/api/perfil` e define `ehAdmin = perfil?.role === 'Admin'`.
- **Motivo:** usar a role do AuthContext daria "falso negativo" (ela não existe lá).
  A proteção real continua no backend (`[Authorize(Roles="Admin")]`); a UI é só a
  primeira camada (esconder/bloquear).

### 2. `SomenteAdmin` é espelho fiel do `PerfilVendedor` (não uma abstração nova)
Copiei o mesmo molde: `SomenteAdminProvider` (useEffect com `AbortController`,
reset síncrono de `carregando`/`erro`, consulta `/api/perfil`), hook `useAdmin()` e
componente `<SomenteAdmin>` com os três estados de render (carregando → `role="status"`;
erro → `role="alert"` + "Tentar novamente"; sem permissão → "Acesso restrito"),
incluindo o comentário `eslint-disable react-hooks/set-state-in-effect`.
- **Motivo:** simplicidade e consistência (AGENTS §0.2). Reusar o padrão existente
  evita divergência de comportamento entre os dois guards.

### 3. Item F — chamada dupla a `/api/perfil` (Opção 1, decisão do produto)
O `LayoutApp` já envolve a árvore com `<PerfilVendedorProvider>` (1 chamada a
`/api/perfil`). Para o link "Moderação" aparecer só a admin, envolvi a árvore
**também** com `<SomenteAdminProvider>` e consumi `useAdmin()` na navegação. Isso
resulta em **duas chamadas idênticas** a `/api/perfil` por carga do layout.
- **Discussão (apresentada e decidida com o time):**
  - **Opção 1 (escolhida):** dois providers independentes; aceita-se a chamada dupla.
  - **Opção 2:** `SomenteAdmin` reusar o contexto do `PerfilVendedorProvider` — exigiria
    alterar o `SomenteVendedor` (expor `role`/perfil cru), proibido nesta task.
  - **Opção 3:** unificar num `PerfilProvider` comum (uma só chamada) — refatoração
    maior que toca o `SomenteVendedor`, fora do escopo delimitado.
- **Motivo da escolha:** manter o escopo mínimo e **não** refatorar o `SomenteVendedor`
  nesta task (instrução explícita). A requisição extra a `/api/perfil` é barata e a
  área é administrativa (baixo tráfego).
- **Trade-off / dívida consciente:** 1 requisição redundante a `/api/perfil`. Fica
  registrada como candidata a melhoria futura: unificar os guards num provider de
  perfil comum (Opção 3) elimina a duplicação sem perda funcional.

### 4. Botão suspender/reativar — mostrar ambos sempre (Opção a)
A UI mostra "Suspender" **e** "Reativar" para o usuário denunciado, sem alternar com
base no estado atual da conta.
- **Discussão:** para alternar o botão (mostrar só um), o item precisaria de um campo
  tipo `denunciadoAtivo: boolean`, que **não** está no shape especificado e cujo DTO
  da B2 ainda não está fechado.
- **Motivo da escolha:** não inventar um campo que o backend pode não entregar
  (AGENTS §0.1/§0.2). Mostrar ambos é mais simples e deixa o admin decidir; o backend
  é a autoridade (retorna 404/400 se a ação não fizer sentido, tratado no feedback).

### 5. Denúncias atrás de um MOCK com flag `USAR_MOCK`
`moderacao.js` tem `const USAR_MOCK = true` no topo e um comentário explicando que,
quando a B2 entregar os endpoints, basta virar a flag (ou remover o ramo do mock) —
a página **não** muda, pois consome só as funções do service.
- **Motivo:** destrava a B3 sem esperar a B2 (trabalho em paralelo), mantendo a troca
  para o backend real num único ponto. As 3 ações de usuário/produto já usam os
  endpoints **reais** do `AdminController` (que existem); só as de denúncia são mock.
- **Shape do mock (decidido):** campos em camelCase, com **nomes já resolvidos**
  (a UI não resolve id→nome; isso é responsabilidade do backend/B2):
  `{ id, denuncianteNome, denunciadoId, denunciadoNome, produtoId|null,
  produtoNome|null, motivo (int), descricao, status (int), criadaEm (ISO) }`.
  4 itens de exemplo cobrindo status (0..3) e motivos variados, com e sem `produtoId`.
  - **`denunciadoId`** é necessário para suspender/reativar.
  - **`produtoId === null`** → a ação de inativar produto não é exibida.

### 6. Enums como INTEIRO, com fallback defensivo (AGENTS §14)
A API não usa `JsonStringEnumConverter`; enums chegam como inteiro. `rotulosDenuncia.js`
mapeia **chaves numéricas e string** (fallback defensivo) + um fallback final que não
quebra a tela, no molde de `rotulosProduto.js`. O seletor de status usa valores
inteiros (`OPCOES_STATUS_DENUNCIA`).
- **Motivo:** bate com o padrão do projeto e resiste a uma eventual troca de
  serialização sem quebrar a UI.

### 7. Ações com confirmação + anti-duplo-clique + feedback por item
Cada ação (status, suspender, reativar, inativar) usa `window.confirm` antes de agir,
um `useRef` (`emCurso`) para impedir reenvio, desabilita os botões enquanto envia e
mostra feedback por item: sucesso (`role="status"`) e erro (`role="alert"`). No molde
do `BotaoExcluirItemBazar`. Após sucesso, a lista é recarregada (reflete sem F5).
- **Motivo:** requisito da issue #115 (feedback visível) e consistência com o padrão
  de ação destrutiva já usado no projeto.

### 8. Tratamento de erro ramificado por `ApiError.status`
`mensagemErro(e)` distingue: **403** → sem permissão (sugere reentrar/relogar, por
token expirado ou perda do papel de admin); **409** → transição de status inválida
(mensagem amigável); **404** → recurso não encontrado (lista desatualizada); demais →
mensagem genérica do `ApiError`. Os PATCH retornam **204 sem corpo** — o wrapper `api`
já trata.
- **Motivo:** ênfase da issue #115; a mensagem de 403 é distinta porque indica um
  problema de sessão/permissão, não de dado.

### 9. Toda chamada via wrapper `api` (nunca `fetch` direto)
`moderacao.js` usa `api.get/patch` e importa `ApiError` de `services/api.js`. As
telas não montam URL.
- **Motivo:** AGENTS §14 (acesso à API centralizado; Bearer injetado; erros
  padronizados via `ApiError`).

### 10. Rota e navegação no padrão existente
`App.jsx`: `<SomenteAdmin><Titulo texto="Moderação"><PainelAdmin/></Titulo></SomenteAdmin>`
dentro de `<LayoutApp>` (sob `<RotaProtegida>`), igual às rotas de vendedor.
`LayoutApp.jsx`: link "Moderação" (`FiShield`) visível só quando `ehAdmin`, no mesmo
molde condicional do botão do vendedor.
- **Motivo:** mínimo de mudança em arquivos de conflito entre squads (AGENTS §9).

## Nota de ambiente (não é alteração de código)
O primeiro `npm run build` falhou por uma dependência **pré-existente não instalada**:
`@microsoft/signalr` (declarada no `package.json` pela Frente 2/trilha B, mas ausente
no `node_modules` local). Resolvido com `npm install` — **sem** alterar `package.json`
nem `package-lock.json`. Não tem relação com os arquivos desta task.

## Validação (AGENTS §14)
- [x] `npm run lint` limpo (0 problemas, inclui `react-hooks/set-state-in-effect`).
- [x] `npm run build` limpo (152 módulos; build concluído após o `npm install`).
- [x] Nenhum segredo versionado.
- [x] Proteção em 2 camadas (UI + backend `[Authorize(Roles="Admin")]`).

## Critérios de aceite (B3 / issue #115)
- [x] Tela restrita a admin (guard na rota + link só para admin).
- [x] Lista denúncias com nomes resolvidos, motivo/status por extenso, data e descrição.
- [x] Ação de mudar status da denúncia (via mock por enquanto).
- [x] Ação de suspender/reativar o usuário denunciado (endpoints reais).
- [x] Ação de inativar o produto denunciado, só quando há `produtoId` (endpoint real).
- [x] Feedback de sucesso e erro por ação; erro ramificado por status (403/409/404/genérico).
- [x] Confirmação + anti-duplo-clique em cada ação.
- [x] Consome só o service; integra a B2 depois sem mudar a página (flag `USAR_MOCK`).

## Pendências / dúvidas em aberto
- **Contrato do DTO da B2:** o shape do mock foi alinhado, mas o DTO real ainda será
  confirmado com o time. Manter o mock fácil de ajustar; ao integrar, conferir nomes
  dos campos e o payload do PATCH de status.
- **Chamada dupla a `/api/perfil` (item F / Opção 1):** candidata a unificação num
  provider de perfil comum numa melhoria futura (ver decisão 3).
- **`npm install` no ambiente:** quem for rodar o front localmente precisa instalar as
  deps (o `@microsoft/signalr` entrou com a Frente 2).
