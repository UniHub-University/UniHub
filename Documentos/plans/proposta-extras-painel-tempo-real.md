# Proposta — Extras do Painel em Tempo Real (Frente 2 / SignalR)

**Autor:** Henrique (Trilha B — Frontend/Consumo) · **Para:** squad da Frente 2
**Contexto:** Sprint 5 — Painel do Vendedor em Tempo Real. Este documento lista
*possíveis incrementos* à trilha de frontend, para o time decidir **se** e **qual**
vale a pena, sem inchar o escopo.

> ATENÇÃO — antes de qualquer extra, fechar o escopo. O planejamento é explícito:
> o SignalR é *incremento, não pré-requisito*, e o prazo é apertado. Há duas
> pendências de escopo com prioridade sobre todo extra abaixo:
> 1. **B4 completo:** degradação graciosa validada + `npm run build` limpo.
> 2. **Integração ponta a ponta (T-int):** ver o card atualizar ao vivo de verdade
>    — hoje bloqueada porque o **A4** (emissão de eventos no `PedidoService`,
>    trilha A) ainda não está pronto. Sem isso, nenhum extra visual foi validado
>    de fato.
>
> Recomendação: só puxar um extra depois que B4 + T-int estiverem verdes.

---

## Estado atual da Trilha B (para referência)

- **B1** [ok] — `services/painelRealtime.js` (conexão SignalR, token via query,
  reconexão automática) + dependência `@microsoft/signalr`.
- **B2** [ok] — estado inicial via `GET /api/perfil/vendedor/produtos` (reusa o
  `MeusProdutos.jsx` já existente; não duplicamos listagem).
- **B3** [ok] — handlers de `EstoqueAtualizado` (atualiza quantidade/status do card
  em memória) e `NovoPedido` (aviso discreto) + indicador de conexão.
- **B4** [parcial] — indicador "Ao vivo/Reconectando/Offline" feito; falta validar
  degradação graciosa e o build final como item de aceite.

---

## Critérios de aceite (do planejamento) — para situar os extras

- Painel mostra produtos (estado inicial REST) **mesmo sem SignalR**.
- Card atualiza a quantidade **sem refresh**, em < ~2s.
- Estoque zera -> card reflete `Esgotado` ao vivo.
- `NovoPedido` dispara indicador discreto.
- Isolamento por vendedor; hub exige JWT; degradação graciosa; reconexão.

Os extras abaixo **não substituem** nenhum critério — são polimento acima deles.

---

## Extras propostos

Legenda de esforço: baixo / medio / alto.

### Extra A — Aviso de `NovoPedido` com autodismiss / contador
**O que:** hoje o aviso de novo pedido aparece e permanece. Melhorar para ele
sumir sozinho após alguns segundos, ou agregar ("3 novos pedidos") em vez de
sobrescrever.
- **Valor:** UX mais limpa; evita aviso "preso" na tela.
- **Esforço:** baixo (timer/estado local, sem backend).
- **Risco:** baixo. Não toca contrato nem backend.
- **Relação com escopo:** refina um critério de aceite existente (`NovoPedido`).
- **Observação:** o `NovoPedido` é o **primeiro item do corte seguro** do plano —
  investir muito aqui é contraintuitivo. Manter simples.

### Extra B — Destaque visual ("flash") no card ao atualizar ao vivo  [preferido]
**O que:** quando chega `EstoqueAtualizado`, dar um realce temporário no card que
mudou (ex.: borda/fundo destacando por ~1-2s), para o vendedor perceber a
mudança acontecendo.
- **Valor:** **alto para o pitch.** É o que torna o "tempo real" visível/impactante
  na demo — o card "reagindo" na frente do avaliador. O plano chama o painel de
  "diferencial técnico do pitch".
- **Esforço:** medio (estado de "recém-atualizado" por id + animação CSS +
  limpeza do destaque por timer).
- **Risco:** baixo/medio. É só frontend, em memória; não afeta o contrato.
- **Relação com escopo:** amplia a percepção do critério "atualiza sem refresh".
- **Candidato preferido** se o time quiser **um** extra.

### Extra C — Reforço visual do status `Esgotado` ao vivo
**O que:** quando o estoque zera via evento, além de trocar o rótulo para
`Esgotado`, reforçar visualmente (ex.: card esmaecido/tarja). Hoje o status já
atualiza; isto é ênfase.
- **Valor:** medio; deixa o "esgotou agora" óbvio na demo.
- **Esforço:** baixo (classe condicional por status).
- **Risco:** baixo.
- **Relação com escopo:** reforça critério existente ("card reflete Esgotado").
- **Observação:** pode ser combinado com o Extra B (o flash já cobre parte disso).

### Extra D — Som/notificação do navegador em `NovoPedido`
**O que:** tocar um som curto ou disparar uma Notification do browser quando
chega um novo pedido.
- **Valor:** baixo/medio; "chama atenção" mesmo com a aba em segundo plano.
- **Esforço:** medio (permissão de Notification, fallback, acessibilidade).
- **Risco:** medio — permissões, política de autoplay de áudio, pode irritar.
- **Relação com escopo:** periférico; fácil de sair do "discreto" que o plano pede.
- **Recomendação:** provavelmente **não** vale agora (custo/benefício ruim no prazo).

---

## Fora de escopo (NÃO fazer nesta frente)

- **Gerir pedidos recebidos no painel** — o planejamento exclui explicitamente
  (tela completa de pedidos é de outra frente/sprint). O painel só *reage* ao
  `NovoPedido` com um indicador.
- **Tempo real em outras telas** (Bazar público, Alimentação) — escopo é o
  painel do vendedor.
- **Botão de reconexão manual** — o `withAutomaticReconnect()` já cobre; seria
  redundante.

---

## Recomendação para a reunião

1. **Prioridade zero:** fechar B4 + integração (T-int) assim que o A4 existir.
2. **Se houver espaço para UM extra:** **Extra B** (flash no card) — melhor
   retorno para o pitch, risco contido, só frontend.
3. **Extras A e C** são "quase de graça" (baixo esforço) e podem entrar junto do
   B4 como polimento, se não atrasarem o merge (meta: 15/11).
4. **Extra D** fica no backlog — custo/risco não justificam no prazo atual.

> Decisão a registrar: qual(is) extra(s) o time aprova, e se entram **nesta**
> frente ou viram backlog pós-entrega. (AGENTS 0.1 — decidir junto, não em
> silêncio.)
