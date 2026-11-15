# Plano — Ação Solidária (voluntário): Doações (Task 5) — ATUALIZADO

## Objetivo
Fechar o lado do voluntário: registrar uma doação a partir de uma correspondência
aceita e acompanhar/gerenciar as próprias doações (confirmar entrega / cancelar).
Última ponta da Frente 1 (lado voluntário).

## Escopo
- **Inclui:**
  - **Backend:** `POST /api/doacoes` passa a aceitar `CorrespondenciaId` (além do
    `SolicitacaoId`, mantido por compatibilidade). O id da solicitação é resolvido a
    partir da correspondência do voluntário logado — o front NÃO manuseia `solicitacaoId`
    (LGPD + simplicidade).
  - **Front:** tela `/solidariedade/doacoes` — lista "minhas doações" + registra a partir
    do gancho `?solicitacao={correspondenciaId}` (vindo do card da Task 4) + confirmar
    entrega / cancelar.
  - Trocar o placeholder `<EmConstrucao>` de `/solidariedade/doacoes` pela tela real.
- **NÃO inclui:** mudança de schema (nenhuma); lado solicitante (Lucas).

## Decisões de design
- **Backend — `CriarDoacaoDto`:** adiciona `CorrespondenciaId` (Guid?, opcional). Regra:
  - Se vier `CorrespondenciaId`: busca a correspondência; valida que é do voluntário
    logado e está `AceitaPeloVoluntario`; usa a `SolicitacaoId` dela. (403/409 se não.)
  - Senão, cai no comportamento antigo (valida aceite por `SolicitacaoId`). Mantém
    compatibilidade com quem já usa o contrato antigo.
  - Resolver o id no backend evita expor `SolicitacaoId` ao front (só circula código público).
- **Front — service:** `criarDoacao` passa a aceitar `{ correspondenciaId, descricao }`.
- **Front — tela Doacoes:**
  - Lê `?solicitacao=<correspondenciaId>` da URL (gancho da Task 4) → abre o form de
    registro já vinculado àquela correspondência.
  - Lista minhas doações (`GET /doacoes/minhas`) com status (`rotuloStatusDoacao`) e datas.
  - Ações "Confirmar entrega"/"Cancelar" só quando `Pendente`.
  - `useEffect` com função async (sem eslint-disable — padrão corrigido).
- **LGPD:** `DoacaoRespostaDto` só traz `CodigoPublicoSolicitacao` — exibir só isso.
- **Impacto concorrência:** nenhum novo.
- **Trade-offs:** `CorrespondenciaId` opcional (aditivo, não quebra contrato) vs novo
  endpoint `/correspondencias/{id}/doar` — escolhido o aditivo, menos superfície.

## Passos de implementação
1. **Backend:** `CriarDoacaoDto` + `DoacaoController.Criar` resolvendo por `CorrespondenciaId`.
2. **Front service:** `criarDoacao({ correspondenciaId, descricao })`.
3. **Front tela:** `pages/solidariedade/voluntario/Doacoes.jsx` + `.css`.
4. **App.jsx:** rota `/solidariedade/doacoes` → `Doacoes` (remove placeholder).

## Critérios de aceite
- [ ] A partir de uma correspondência aceita, "Registrar doação" abre o form e cria a doação.
- [ ] 409 (correspondência não aceita / não é do voluntário) tratado com mensagem.
- [ ] Lista "minhas doações" com status e datas (só código público — LGPD).
- [ ] Confirmar entrega → Concluída; cancelar → Cancelada; ações só quando Pendente.
- [ ] `dotnet build` limpo; `npm run lint` limpo; fluxo validado no `npm run dev`.

## Pendências / dúvidas em aberto
- Teste de unidade do novo caminho (CorrespondenciaId) — o Criar usa AppDbContext direto;
  avaliar teste de integração com o squad/floater.