# Plano — Ação Solidária: frontend do lado SOLICITANTE

## Objetivo
Entregar as telas do solicitante no módulo de Ação Solidária: criar uma solicitação
de apoio, acompanhar as próprias solicitações com status, cancelar e autorizar a
revelação de identidade (ação de LGPD que só o solicitante dispara).

Trabalho em dupla: o lado VOLUNTÁRIO é do Gabriel (PR próprio). Este plano cobre só
o lado SOLICITANTE (Lucas).

## Contexto / base já existente na `main`
A infraestrutura base do módulo já foi mergeada (não recriar):
- `services/acaoSolidaria.js` — wrapper com as funções do solicitante:
  `criarSolicitacao`, `listarSolicitacoesAbertas`, `obterSolicitacao`,
  `cancelarSolicitacao`, e a de LGPD `revelarIdentidade`.
- `services/rotulosSolidariedade.js` — `OPCOES_CATEGORIA`, `rotuloCategoria`,
  `rotuloStatusSolicitacao`, etc. (enums chegam como inteiros).
- `pages/solidariedade/SolidariedadeHub.jsx` — hub que direciona solicitante/voluntário.
- Rotas já reservadas em `App.jsx` apontando para o placeholder `EmConstrucao`:
  `/solidariedade/solicitar` e `/solidariedade/minhas`. Trocar esses dois.

## Escopo
- **Inclui:**
  - Tela "Nova solicitação" (`/solidariedade/solicitar`): formulário com categoria
    (`OPCOES_CATEGORIA`) + descrição → `criarSolicitacao`.
  - Tela "Minhas solicitações" (`/solidariedade/minhas`): lista com status, cancelar
    e, quando houver correspondência, autorizar revelação de identidade.
  - Troca dos placeholders `EmConstrucao` dessas duas rotas pelas telas reais.
  - CSS no padrão do módulo/tema global.
- **NÃO inclui:**
  - Lado voluntário (Gabriel).
  - Alterar o backend (ver gap abaixo) — a confirmar com o squad.

## Gap de backend conhecido (bloqueia parte de "Minhas solicitações")
O `SolicitacaoApoioController` tem: `POST` (criar), `GET` (lista ABERTAS — visão
pública/anônima), `GET /{id}`, `PATCH /{id}/cancelar`. **NÃO existe** um
`GET /api/SolicitacaoApoio/minhas` para o solicitante listar as PRÓPRIAS solicitações
com status atualizado. O comentário no `acaoSolidaria.js` registra isso.
- **Decisão:** começar pelas telas que não dependem do gap (Nova solicitação) e
  acionar o squad/floater para criar o `GET /minhas`. A tela "Minhas solicitações"
  é construída já pronta para consumir esse endpoint quando existir.

## Decisões de design
- **Arquivos só na subpasta do solicitante:** `pages/solidariedade/solicitante/`
  (telas e CSS), sem tocar nos arquivos do Gabriel.
- **Pontos de conflito combinados:** `acaoSolidaria.js` e `App.jsx` só mudam com
  aviso ao Gabriel. Aqui só se adiciona rota/função nova se necessário.
- **Todas as chamadas via `acaoSolidaria.js`** (padrão do projeto; sem `fetch` direto).
- **Rótulos sempre via `rotulosSolidariedade.js`** (enums inteiros).
- **LGPD:** a UI é anônima por padrão (só `CodigoPublico`); a identidade real só
  aparece após `revelarIdentidade`, disparado pelo próprio solicitante.
- **Estados de loading/erro/vazio** no padrão de `Perfil.jsx`/`BuscaProdutos.jsx`.

## Fatiamento em commits (distribuir ao longo da semana, até terça)
1. Plano em `Documentos/plans/` (este commit).
2. Tela "Nova solicitação": formulário + submit + rota real.
3. CSS e polimento da "Nova solicitação" (validação, feedback de sucesso/erro).
4. Tela "Minhas solicitações": lista + status (consumindo `GET /minhas`).
5. Ação cancelar solicitação na lista.
6. Ação revelar identidade (LGPD).
7. Acessibilidade / ajustes finais (aria, responsividade, reduced-motion).

Cada commit é coeso e compila sozinho.

## Critérios de aceite
- [ ] "Nova solicitação" cria via `criarSolicitacao` e dá feedback de sucesso/erro.
- [ ] "Minhas solicitações" lista as do usuário com status legível (rótulos).
- [ ] Cancelar funciona e reflete o novo status.
- [ ] Revelar identidade só aparece/funciona para o solicitante (LGPD).
- [ ] Nenhuma tela expõe identidade antes da revelação.
- [ ] Placeholders `EmConstrucao` das duas rotas substituídos.
- [ ] `npm run lint` e `npm run build` limpos.

## Pendências / dúvidas em aberto
- `GET /api/SolicitacaoApoio/minhas`: acionar o squad/floater (gap de backend).
- Branch de integração: não existe `feat/solidariedade-sprint5` no remoto; esta
  branch sai da `main` e o PR mira na `main` (confirmar com a dupla se haverá
  branch de integração).
- Revisar o PR do lado voluntário (Gabriel), com atenção especial à LGPD.
