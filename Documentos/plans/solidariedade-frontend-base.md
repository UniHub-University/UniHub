# Plano — Ação Solidária (frontend): infraestrutura base compartilhada

## Objetivo
Estabelecer a fundação compartilhada do módulo de Ação Solidária no front (Frente 1,
Sprint 5), para que as duas pessoas (lado solicitante e lado voluntário) desenvolvam
em paralelo sem conflito. Troca o `/solidariedade` de `EmBreve` por uma estrutura real.

## Escopo
- **Inclui:**
  - `services/acaoSolidaria.js`: wrapper único com TODAS as chamadas do módulo
    (solicitações, voluntário, correspondências, doações).
  - `services/rotulosSolidariedade.js`: rótulos dos enums + `OPCOES_CATEGORIA` para selects.
  - `pages/solidariedade/SolidariedadeHub.jsx` + `.css`: tela de entrada ("preciso de
    apoio" / "quero ajudar").
  - `pages/solidariedade/EmConstrucao.jsx`: placeholder temporário das telas ainda não feitas.
  - `App.jsx`: `/solidariedade` → hub + 5 sub-rotas (placeholders por ora).
- **NÃO inclui:**
  - As telas reais de cada lado (ficam nas branches individuais de Gabriel e Lucas).
  - Qualquer mudança de backend (o backend do módulo já está pronto — 14 endpoints).

## Decisões de design
- **Entidades/Domain:** nenhuma (só front, consome backend existente).
- **Endpoints/API consumidos (já na main):**
  - Solicitação: `POST/GET /api/SolicitacaoApoio`, `GET /{id}`, `PATCH /{id}/cancelar`.
  - Voluntário: `GET/POST /api/perfil/voluntario`, `PUT /areas`, `PATCH /pausar|/retomar`.
  - Correspondência: `GET /sugestoes`, `POST /aceitar`, `POST /{id}/concluir|cancelar|revelar-identidade`.
  - Doação: `POST /api/doacoes`, `GET /minhas`, `GET /{id}`, `POST /{id}/confirmar-entrega|cancelar`.
- **Banco/Migrations:** nenhum.
- **Impacto em concorrência/LGPD:**
  - LGPD: a UI é anônima por padrão — só circula `CodigoPublico`. A identidade real do
    solicitante só aparece após `revelar-identidade`, que SÓ o solicitante dispara. Os
    DTOs do backend já garantem isso; o front não deve exibir nada além do que a API devolve.
  - Concorrência: o `aceitar` pode retornar 409 (outro voluntário aceitou primeiro) —
    as telas devem tratar.
- **Estrutura de pastas:** `pages/solidariedade/` com subpastas `solicitante/` (Lucas)
  e `voluntario/` (Gabriel) — isola os dois lados, zero conflito de arquivo.
- **Pontos de conflito controlados:** `acaoSolidaria.js` e `App.jsx` são compartilhados;
  ficam COMPLETOS nesta base para ninguém precisar editá-los durante o desenvolvimento
  dos dois lados. Alterações futuras neles exigem alinhamento entre a dupla.
- **Trade-offs:** base compartilhada mergeada primeiro (branch-guia de fundação, AGENTS.md
  seção 14) vs cada um criar a sua — escolhido a base única para evitar duplicação e conflito.

## Divisão de tarefas (após esta base)
| Rota | Lado | Dono |
|---|---|---|
| `/solidariedade` | Hub (pronto nesta base) | — |
| `/solidariedade/solicitar`, `/solidariedade/minhas` | Solicitante | Lucas |
| `/solidariedade/voluntario`, `/sugestoes`, `/doacoes` | Voluntário | Gabriel |

## Passos de implementação
1. `services/acaoSolidaria.js` (todas as funções).
2. `services/rotulosSolidariedade.js` (rótulos + opções de categoria).
3. `pages/solidariedade/SolidariedadeHub.jsx` + `.css`.
4. `pages/solidariedade/EmConstrucao.jsx` (placeholder).
5. `App.jsx`: trocar EmBreve → hub + sub-rotas; remover import órfão de EmBreve.

## Critérios de aceite
- [ ] `/solidariedade` abre o hub (não mais o EmBreve).
- [ ] As sub-rotas abrem placeholders sem quebrar a navegação.
- [ ] Service e rótulos disponíveis para os dois lados importarem.
- [ ] `npm run lint` limpo; `npm run dev` roda sem erro.

## Pendências / dúvidas em aberto
- **Gap de backend:** não existe `GET` de "minhas solicitações" do solicitante para
  acompanhar status. Necessário para `/solidariedade/minhas` (Lucas). Acionar o
  floater/squad para criar (ex.: `GET /api/SolicitacaoApoio/minhas`).
- Confirmar com a dupla o nome final das sub-rotas antes de preencher as telas.
