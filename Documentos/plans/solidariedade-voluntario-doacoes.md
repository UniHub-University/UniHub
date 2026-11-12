# Plano — Ação Solidária (voluntário): Doações (Task 5)

## Objetivo
Dar ao voluntário a tela `/solidariedade/doacoes` para registrar uma doação para uma
solicitação que já aceitou e acompanhar as próprias doações (status + confirmar
entrega / cancelar). Fecha a ponta "doação → concluir" do fluxo do voluntário.

## Escopo
- **Inclui:**
  - Tela `Doacoes` em `pages/solidariedade/voluntario/`.
  - Listar minhas doações (`GET /api/doacoes/minhas`) com status e datas.
  - Registrar nova doação (`POST /api/doacoes` com `{ solicitacaoId, descricao }`).
  - Confirmar entrega (`POST /api/doacoes/{id}/confirmar-entrega`) e cancelar
    (`POST /api/doacoes/{id}/cancelar`).
  - Trocar o placeholder `<EmConstrucao>` de `/solidariedade/doacoes` pela tela real.
- **NÃO inclui:**
  - Lista de correspondências aceitas + concluir (Task 4).
  - Backend (pronto). Mudança de schema (nenhuma).

## Decisões de design
- **Endpoints (via acaoSolidaria.js):**
  - `listarMinhasDoacoes()` → lista de `DoacaoRespostaDto`
    `{ id, codigoPublicoSolicitacao, descricao, status, createdAt, entregueEm }`.
  - `criarDoacao({ solicitacaoId, descricao })` → 201 (criada) | 400 (não é voluntário)
    | 409 (não aceitou a solicitação antes).
  - `confirmarEntregaDoacao(id)` / `cancelarDoacao(id)` → 204 | 403 (não é dona) | 409 (estado inválido).
- **LGPD:** o DTO só traz `CodigoPublicoSolicitacao` — a tela exibe apenas isso, nunca
  identidade do solicitante. (Há teste de backend que quebra se vazar.)
- **Como o voluntário informa a solicitacaoId ao doar:** o backend exige que ele já
  tenha ACEITO a solicitação. Nesta tela, o campo é o **CÓDIGO PÚBLICO**? Não — o
  `criarDoacao` espera o `solicitacaoId` (GUID). Decisão: v1 pede o `solicitacaoId`
  num campo (vindo da tela de sugestões/correspondências ao aceitar). Para não travar,
  aceitamos o id colado/manual nesta v1; a ligação fluida "aceitar → doar" com o id já
  preenchido fica para a integração com a Task 4 (quando houver a lista de aceitas).
  ⚠️ Confirmar com o fluxo: idealmente o botão "Registrar doação" nasce a partir de uma
  correspondência aceita (Task 4). Enquanto a Task 4 não existe, a tela de doações
  permite registrar informando a solicitação.
- **Status:** usar `rotuloStatusDoacao` (Pendente/Concluída/Cancelada) dos rótulos compartilhados.
- **Ações por estado:** "Confirmar entrega" e "Cancelar" só aparecem quando `Pendente`.
- **Trade-offs:** recarregar a lista após cada ação (simples, sempre correto) vs mutação
  local — escolhido recarregar via gatilho, pela robustez.

## Passos de implementação
1. `pages/solidariedade/voluntario/Doacoes.jsx` + `.css`.
2. Form de nova doação (solicitacaoId + descrição) → `criarDoacao`; tratar 409 (não aceitou).
3. Lista de minhas doações (`listarMinhasDoacoes`) com status + ações por estado.
4. `App.jsx`: trocar o placeholder de `/solidariedade/doacoes` pelo componente real.
   ⚠️ Coordenar com o Lucas (arquivo compartilhado).

## Critérios de aceite
- [ ] Voluntário vê suas doações com status e datas (só código público — LGPD).
- [ ] Registrar doação de uma solicitação aceita funciona; 409 (não aceitou) mostra mensagem.
- [ ] Confirmar entrega muda status para Concluída; cancelar muda para Cancelada.
- [ ] Ações aparecem só quando faz sentido (Pendente).
- [ ] `npm run lint` limpo (effect com função async, sem eslint-disable); fluxo validado no `npm run dev`.

## Pendências / dúvidas em aberto
- Ligar "doar" diretamente a partir de uma correspondência aceita (Task 4) — melhora o
  fluxo (id já preenchido). Enquanto a Task 4 não existe, a tela aceita a solicitação informada.
- Confirmar com a dupla se o registro de doação deve viver aqui ou dentro da Task 4.