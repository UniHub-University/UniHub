# Plano — Ação Solidária (voluntário): Área do voluntário + áreas de atuação (Task 1)

## Objetivo
Dar ao usuário a tela `/solidariedade/voluntario` para: tornar-se voluntário, definir/
editar suas áreas de atuação e pausar/retomar a atuação. É o pré-requisito do fluxo do
voluntário (sem áreas definidas, não há sugestões compatíveis).

## Escopo
- **Inclui:**
  - Tela `AreaVoluntario` em `pages/solidariedade/voluntario/`.
  - Se NÃO é voluntário: CTA "Tornar-se voluntário" (`POST /api/perfil/voluntario`).
  - Se É voluntário: editor de áreas de atuação (checkboxes das `OPCOES_CATEGORIA`) →
    `PUT /api/perfil/voluntario/areas`; e controle pausar/retomar (`PATCH /pausar|/retomar`).
  - Trocar o placeholder `<EmConstrucao>` de `/solidariedade/voluntario` pela tela real.
- **NÃO inclui:**
  - Sugestões/aceitar (Task 3), correspondências (Task 4), doações (Task 5).
  - Mudança no card de voluntário do Perfil (continua existindo; aqui é a tela dedicada do módulo).
  - Backend (tudo pronto).

## Decisões de design
- **Entidades/Domain:** nenhuma (só front).
- **Endpoints/API consumidos (via `services/acaoSolidaria.js`, já na main):**
  - `GET /api/perfil/voluntario` → `{ codigoPublico, areasDeAtuacao, disponivel }`.
    ⚠️ Retorna **BadRequest (400)** se o usuário ainda NÃO é voluntário — o front trata
    esse 400 como "ainda não é voluntário" (mostra o CTA), não como erro de verdade.
  - `POST /api/perfil/voluntario` → torna-se voluntário.
  - `PUT /api/perfil/voluntario/areas` body `{ areas: number[] }` → substitui a lista inteira.
  - `PATCH /pausar` e `/retomar` → alterna `disponivel`.
- **Estado da tela:** 3 situações — carregando / não-é-voluntário (CTA) / é-voluntário (editor).
- **Áreas:** checkboxes a partir de `OPCOES_CATEGORIA` (valores inteiros do enum CategoriaApoio).
  Botão "Salvar áreas" envia a lista completa marcada. Permitir lista vazia? Backend aceita,
  mas sem áreas o voluntário não recebe sugestões — avisar o usuário (não bloquear).
- **Impacto LGPD:** nenhum dado sensível aqui (é o próprio perfil do voluntário).
- **Reuso:** o Perfil já faz tornar-se voluntário e pausar/retomar; NÃO duplicar lógica —
  reusar `acaoSolidaria.js`. Esta tela é a visão dedicada do módulo (centraliza áreas +
  estado), que o Perfil não cobre (o Perfil não edita áreas).
- **Trade-offs:** tela única com os 3 estados (simples) vs várias telas — escolhida a única.

## Passos de implementação
1. `pages/solidariedade/voluntario/AreaVoluntario.jsx` + `.css`.
2. Consumir `obterMeuVoluntario` (tratando 400 = não-voluntário), `tornarSeVoluntario`,
   `definirAreas`, `pausarAtuacao`, `retomarAtuacao`.
3. `App.jsx`: trocar o placeholder de `/solidariedade/voluntario` pelo componente real.
   ⚠️ Coordenar com o Lucas (arquivo compartilhado).

## Critérios de aceite
- [ ] Usuário não-voluntário vê o CTA e consegue se tornar voluntário.
- [ ] Voluntário vê e edita suas áreas (salvar reflete o novo estado).
- [ ] Pausar/retomar alterna o estado `disponivel` e dá feedback.
- [ ] 400 do GET (não-voluntário) é tratado como estado normal, não como erro.
- [ ] `npm run lint` limpo; fluxo validado no `npm run dev`.

## Pendências / dúvidas em aberto
- A tela do voluntário deve ter um link de volta ao hub `/solidariedade` (sim, incluir).
- Sugestões/correspondências/doações entram como acessos a partir desta área (Tasks 3-5).