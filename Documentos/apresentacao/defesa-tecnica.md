# Defesa Técnica — UniHub (20 min)

> Pontos de fala da parte técnica. Cada seção: "o que falar" + "evidência" (onde está
> no código, se a banca perguntar). Tempos somam ~18 min + 2 min de margem/transição.

## 1. Stack + Clean Architecture (3 min)
- Stack: .NET 10 (ASP.NET Core) + EF Core 10/Npgsql, Neon (Postgres serverless), React 19 + Vite, SignalR, Cloudinary, Render + Vercel + Cloudflare.
- 4 camadas, dependência só pra dentro: **API → Application → Infrastructure → Domain**.
- **Por que importa:** regra de negócio na entidade, testável sem web/banco.
- **Exemplo:** o SignalR não acopla o `PedidoService` — emissão via interface (`INotificadorPainel`) na Application; implementação na API. Dependência respeitada.

## 2. · Concorrência de estoque — o diferencial (6 min) — INVESTIR AQUI
- **Problema:** N compradores disputando a última unidade ao mesmo tempo → risco de oversell.
- **Solução:** decremento **atômico** `UPDATE ... WHERE QuantidadeDisponivel >= qtd` no Postgres. Verifica `linhasAfetadas == 1` → quem "ganhou a corrida".
- **Prova:** teste de concorrência real (`ConcorrenciaEstoqueIntegracaoTests`) — **10 compradores simultâneos, 1 unidade → exatamente 1 vence, estoque nunca negativo.** Conexões paralelas, commit real.
- **Fala:** "Não é um CRUD — tratamos concorrência como produção, e provamos com teste automatizado."
- **Bônus:** mesmo padrão em expiração de reserva e baixa do vendedor; efeito colateral (devolver estoque) só por quem ganha a corrida — evita dupla-devolução.

## 3. Segurança & LGPD (5 min)
- **Auth:** JWT assinado/validado; role no token; painel admin restrito a `Admin`.
- **Escalonamento de privilégio:** analisado e fechado — role só muda no banco; nenhum DTO aceita `Role`; fail-safe (role desconhecida → menor privilégio).
- **Hardening:** Swagger só em dev, handler global de exceção (não vaza stack), rate limiting (login, denúncia); DDoS na borda (Cloudflare).
- **LGPD (Ação Solidária):** identidade pseudonimizada (`CodigoPublico`); dados sensíveis projetados só após revelação autorizada; id de usuário nunca exposto no front (traduzido no backend).
- **Upload seguro:** valida extensão **e** magic bytes.
- **Evidência:** `Documentos/Analise-de-Seguranca.md`.

## 4. Qualidade, testes e processo (4 min)
- **28 arquivos de teste:** domínio, application (mocks NSubstitute), integração (Postgres real, transação+rollback), concorrência.
- **CI (GitHub Actions):** build + migrations pendentes + testes unitários + imagem Docker.
- **Processo:** fluxo BRV → Issue → PR → review → merge; **mais de 90 PRs** integrados; ~50 planos em `Documentos/plans/`; `AGENTS.md` como fonte de verdade.
- **Fala:** "Não é só código funcionando — é processo de engenharia: planejamento, review rigoroso, testes e CI."

## 5. Margem / transição (2 min)

## 🎯 Perguntas prováveis da banca (preparar)
- *"Dois usuários comprando ao mesmo tempo?"* → seção 2 (o ouro).
- *"Como protegem dados na Ação Solidária?"* → seção 3 (pseudonimização, revelação autorizada).
- *"Como alguém vira admin?"* → só pelo banco; analisado contra escalonamento.
- *"Testaram?"* → 28 arquivos, incluindo concorrência real; estratégia documentada.
- *"E se Cloudinary/SignalR cair?"* → best-effort: falha de serviço externo nunca derruba a operação.
