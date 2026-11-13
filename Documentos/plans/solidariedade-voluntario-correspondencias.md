# Plano — Ação Solidária (voluntário): Minhas correspondências + concluir (Task 4)

## Objetivo
Permitir que o voluntário veja as correspondências que aceitou e acompanhe/encerre
cada uma (concluir ou cancelar). Fecha o meio do fluxo do voluntário
(aceitar → acompanhar → concluir) e serve de base para registrar doações (Task 5).

## Escopo
- **Inclui:**
  - **Backend (gap):** novo `GET /api/correspondencias/minhas` — lista as
    correspondências do voluntário logado. Reusa o `CorrespondenciaRespostaDto`
    existente (anônimo — LGPD). É o endpoint que faltava para esta frente.
  - **Front:** tela `MinhasCorrespondencias` consumindo o endpoint, com ações
    concluir (`POST /{id}/concluir`) e cancelar (`POST /{id}/cancelar`).
  - Trocar/adicionar a rota do voluntário para "Minhas correspondências".
- **NÃO inclui:**
  - Doações (Task 5) — mas esta tela prepara o gancho "Registrar doação".
  - `revelar-identidade` (é ação do SOLICITANTE, lado do Lucas), não do voluntário.

## Decisões de design
- **Backend — `GET /api/correspondencias/minhas`:**
  - Autenticado; voluntário vem do token (claim "sub") → `ObterVoluntarioLogado()`.
    Não é voluntário → 400 (mesmo padrão de `sugestoes`/`aceitar`).
  - Projeta direto para `CorrespondenciaRespostaDto`
    `{ Id, CodigoPublicoSolicitacao, CategoriaSolicitacao, Status, CreatedAt, AceitaEm, IdentidadeRevelada }`.
  - **LGPD:** `AsNoTracking` + `.Select(...)` sem `.Include(Solicitacao.Solicitante)` —
    só o CodigoPublico e a categoria circulam. Segue exatamente o padrão de `sugestoes`.
  - Ordena por `CreatedAt` desc (mais recentes primeiro). Filtra por `VoluntarioId`.
  - Sem mudança de schema → sem migration.
- **Front:**
  - `listarMinhasCorrespondencias()` novo em `services/acaoSolidaria.js`.
  - Tela lista os cards (categoria + código público + status + datas) com rótulos de
    `rotulosSolidariedade.js`. Ações "Concluir"/"Cancelar" só quando `AceitaPeloVoluntario`.
  - Trata 409 (transição inválida) e 403 com mensagem; recarrega a lista após ação.
  - `useEffect` com função async (sem eslint-disable — padrão já corrigido nos nits).
- **Impacto concorrência:** nenhum novo (concluir/cancelar já existem e tratam estado).
- **Trade-offs:** reusar `CorrespondenciaRespostaDto` (já existe, anônimo) em vez de
  criar DTO novo — menos superfície, consistente.

## Passos de implementação
1. **Backend:** `GET minhas` no `CorrespondenciaController` (projeção anônima).
2. **Front service:** `listarMinhasCorrespondencias()` em `acaoSolidaria.js`.
3. **Front tela:** `pages/solidariedade/voluntario/MinhasCorrespondencias.jsx` + `.css`.
4. **App.jsx:** rota `/solidariedade/correspondencias` + link na Área do voluntário.

## Critérios de aceite
- [ ] `GET /api/correspondencias/minhas` devolve só as correspondências do voluntário logado, anônimas.
- [ ] Não-voluntário recebe 400; sem token, 401.
- [ ] Tela lista as correspondências com status; concluir/cancelar funcionam e refletem na lista.
- [ ] Ações aparecem só quando faz sentido (aceita).
- [ ] `dotnet build` limpo (mexe em backend → CI roda); `npm run lint` limpo.

## Pendências / dúvidas em aberto
- Adicionar teste de unidade do endpoint? O controller usa AppDbContext direto (não
  service), então teste viria como integração — avaliar com o squad/floater.
- Ligação com a Task 5: o botão "Registrar doação" nasce do card de uma correspondência aceita.