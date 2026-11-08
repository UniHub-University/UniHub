# Plano — Ação Solidária (voluntário): Sugestões + aceitar (Task 3)

## Objetivo
Dar ao voluntário a tela `/solidariedade/sugestoes` para ver solicitações compatíveis
com suas áreas de atuação e aceitar uma delas (virando uma correspondência). É o passo
que conecta o voluntário a quem precisa de ajuda.

## Escopo
- **Inclui:**
  - Tela `Sugestoes` em `pages/solidariedade/voluntario/`.
  - Listar sugestões (`GET /api/correspondencias/sugestoes`) — só dados anônimos.
  - Aceitar uma sugestão (`POST /api/correspondencias/aceitar`), tratando o 409
    (outro voluntário aceitou primeiro) e atualizando a lista.
  - Trocar o placeholder `<EmConstrucao>` de `/solidariedade/sugestoes` pela tela real.
- **NÃO inclui:**
  - Acompanhar/concluir correspondências aceitas (Task 4).
  - Doações (Task 5). Backend (pronto).

## Decisões de design
- **Endpoints (via acaoSolidaria.js):**
  - `listarSugestoes()` → lista de `{ id, codigoPublico, categoria, descricao, status, createdAt }`.
  - `aceitarCorrespondencia(solicitacaoId)` → 200 (ok) | 409 (já não disponível) | 400 (não é voluntário).
- **LGPD (crítico):** a lista é anônima por construção — só `codigoPublico`, categoria e
  descrição. NUNCA exibir identidade. Usar `rotuloCategoria` dos rótulos compartilhados.
- **Concorrência:** ao aceitar, o backend usa UPDATE condicional; se outro voluntário
  pegou primeiro, volta **409**. A tela mostra a mensagem e **recarrega a lista** (a
  solicitação disputada some). É o mesmo espírito do 409 de estoque que já tratei antes.
- **Voluntário pausado / sem áreas:** o backend devolve lista vazia — a tela mostra um
  estado vazio com dica ("defina áreas / retome atuação na Área do voluntário").
- **Não-voluntário:** se cair aqui sem ser voluntário, o `aceitar`/`sugestoes` pode
  responder erro; mostrar aviso com link para `/solidariedade/voluntario`.
- **Feedback de sucesso:** ao aceitar, informar que a correspondência foi criada e
  apontar para "Minhas correspondências" (Task 4, ainda placeholder) — link já deixado.
- **Trade-offs:** aceitar recarrega a lista inteira (simples, sempre correto) vs remover
  só o item localmente — escolhido recarregar, pela robustez diante do 409.

## Passos de implementação
1. `pages/solidariedade/voluntario/Sugestoes.jsx` + `.css`.
2. Consumir `listarSugestoes` (estados carregando/erro/vazio) e `aceitarCorrespondencia`
   (botão por card; trata 409 recarregando).
3. `App.jsx`: trocar o placeholder de `/solidariedade/sugestoes` pelo componente real.
   ⚠️ Coordenar com o Lucas (arquivo compartilhado).

## Critérios de aceite
- [ ] Voluntário vê as sugestões compatíveis (anônimas: código público + categoria + descrição).
- [ ] Aceitar cria a correspondência e remove a sugestão da lista.
- [ ] 409 (outro aceitou primeiro) mostra mensagem e atualiza a lista.
- [ ] Estado vazio tratado (pausado/sem áreas) com dica acionável.
- [ ] `npm run lint` limpo; fluxo validado no `npm run dev`.

## Pendências / dúvidas em aberto
- Link "Minhas correspondências" aponta para a Task 4 (ainda placeholder até ela existir).