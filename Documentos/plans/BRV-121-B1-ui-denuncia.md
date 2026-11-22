# Plano — B1: UI de Denúncia (Ciclo de Moderação)

> Trilha 1 da extensão da Sprint 5. Frontend (+ um ajuste de backend pré-requisito).
> O `POST /api/denuncias` já existe; falta a UI e um pequeno ajuste no contrato (ver Opção B).

## Objetivo

Permitir que um usuário autenticado denuncie um vendedor (e, quando aplicável, um produto), escolhendo um motivo e escrevendo uma justificativa. A denúncia é enviada para análise da administração. Exibir/gerenciar as denúncias é de outras frentes (B2 backend do painel, B3 UI do painel) — fora do escopo da B1.

## Escopo

- **Inclui:**
  - Service `denuncias.js` com `criarDenuncia` → `POST /api/denuncias`.
  - **Modal de denúncia** (overlay escurecendo o fundo + "balãozinho" central):
    select de motivo (rótulos amigáveis) + textarea de justificativa + enviar/cancelar.
  - Botão **"Denunciar"** no **detalhe do produto** (`DetalheBazar` e `DetalheAlimentacao`)
    — com `produtoId` preenchido.
  - Botão **"Denunciar"** no **perfil do vendedor** (`PaginaVendedor`) — sem `produtoId`.
  - Estados de carregando, sucesso e erro.

- **NÃO inclui:**
  - Painel admin / listagem / gestão de denúncias (B2 e B3).
  - Notificação ao denunciado (confidencial — LGPD).
  - "Minhas denúncias".

## Decisões de design

- **Endpoint:** `POST /api/denuncias`. Denunciante vem do token (claim `sub`).
- **Como identificar o denunciado — Opção escolhida por LGPD:** o front envia o
  **`VendedorId`** (que as telas têm) e o **backend traduz** para o `UsuarioId` dono
  internamente. O `UsuarioId` NUNCA é exposto no front (respeita a LGPD, que já omite
  o `UsuarioId` do `VendedorPublicoDto`). Requer um ajuste de backend (ver pré-requisito).
- **Motivos (enum, enviado como nome):** `ConteudoOfensivo`, `ComportamentoInadequado`,
  `Assedio`, `Fraude`, `ProdutoInadequado`, `Outro`. No select, rótulos amigáveis
  ("Conteúdo ofensivo", "Comportamento inadequado", "Assédio", "Fraude", "Produto
  inadequado", "Outro"); envia o nome do enum. Reaproveitar `rotulosDenuncia.js` do PR da B3 (Pedro).
- **Justificativa:** `Descricao`, obrigatória, 10–500 caracteres (validação do backend).
- **UI do botão:** cor **vermelha/alerta** (não verde — não é ação positiva como Comprar),
  discreto. No detalhe do produto: do lado oposto aos botões Comprar/Reservar, mesma altura.
  No perfil do vendedor: no card do cabeçalho, à direita.
- **Modal reutilizável:** um só componente, recebe `vendedorId` e `produtoId` (opcional).
- **Chamadas via wrapper `api.js`** (padrão do projeto).

## Pré-requisito de backend (Opção escolhida por LGPD)

Antes do front consumir, o `POST /api/denuncias` precisa aceitar o `VendedorId` e
traduzir para `UsuarioId`. Mudanças previstas (sem migration):
1. `CriarDenunciaDto`: adicionar `Guid? VendedorId` (opcional).
2. Repositório de vendedor: método `ObterUsuarioIdPorVendedorAsync(vendedorId)`
   (irmão do `ObterVendedorIdPorUsuarioAsync` que já existe).
3. `DenunciasController`: se vier `VendedorId`, traduzir para `DenunciadoId` antes de salvar.

~~> ⚠️ Mexer no backend de denúncia é, a rigor, escopo da B2. **Alinhar com a pessoa da B2** quem faz esse ajuste: a B1 (como trabalho extra, em PR separado) ou a própria B2. Decisão pendente.~~ **RESOLVIDO**

## Passos de implementação

1. **[Backend — pré-requisito]** Ajuste da Opção escolhida por LGPD (DTO + método de tradução + controller), em PR próprio. (Só após alinhar com a B2.)
2. **[Service]** `services/denuncias.js` com `criarDenuncia({ vendedorId, produtoId, motivo, descricao })`.
3. **[Componente]** `components/ModalDenuncia.jsx` (+ css): overlay + modal, select de motivo,
   textarea, validação (motivo obrigatório, descrição 10–500), estados enviando/sucesso/erro.
4. **[Detalhe do produto]** Botão "Denunciar" (vermelho) em `DetalheBazar` e
   `DetalheAlimentacao` → abre o modal com `vendedorId` e `produtoId = item.id`.
5. **[Perfil do vendedor]** Botão "Denunciar" (vermelho) em `PaginaVendedor` → abre o
   modal com `vendedorId`, sem `produtoId`.
6. **[Validação]** `npm run lint` e `npm run build` limpos; teste manual nos dois lugares.

## Critérios de aceite

- [x] Usuário denuncia um vendedor pelo **detalhe do produto** (motivo + justificativa + produto).
- [x] Usuário denuncia um vendedor pelo **perfil do vendedor** (motivo + justificativa, sem produto).
- [x] Modal aparece sobre a tela escurecida; dá para cancelar sem enviar.
- [x] Os 6 motivos aparecem com rótulos amigáveis e enviam o nome do enum.
- [x] Justificativa valida 10–500 caracteres antes de enviar.
- [x] Mensagem de sucesso após enviar; erros tratados (401/400/404/500).
- [x] Não é possível denunciar a si mesmo (backend bloqueia; UI exibe a mensagem).
- [x] Botão "Denunciar" é visualmente distinto (alerta/vermelho), não confundível com Comprar.
- [x] `npm run lint` e `npm run build` limpos.

## Pendências / dúvidas em aberto

1. ~~Quem faz o ajuste de backend da Opção B?~~ **RESOLVIDO:** feito por B1 (eu), como trabalho extra em PR separado (#121, mergeado), com aval da Maria e Henrique.
2. ~~Ordem de merge com a B3 (Pedro)~~ **RESOLVIDO:** o PR do Pedro (#118) já está na main. A task 1 (service+modal) não toca arquivos compartilhados; ao chegar nos botões (detalhe/perfil), rebasear na main antes de abrir o PR de front.
3. ~~Reaproveitar `rotulosDenuncia.js`~~ **RESOLVIDO:** já está na main (#118).

## Decisões confirmadas

- **Abordagem do ID = Opção escolhida por LGPD** (backend traduz `VendedorId` → `UsuarioId`), por proteger
  a LGPD (o `UsuarioId` nunca sai para o front).
- **Entrega em 2 PRs:** (1) ajuste de backend — PR 121, mergeado; (2) frontend — PR único a partir da branch `feat/BRV-121-B1-front-ui-denuncia`, com commits por task: service + modal, depois o botão no detalhe do produto, depois no perfil do vendedor.
