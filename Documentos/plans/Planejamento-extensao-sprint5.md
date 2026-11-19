# Planejamento Sprint 5 — Extensão

Ciclo de Moderação (Denúncia + Painel Admin) + hardening da entrega · 7 membros · 2 trilhas paralelas

## Por que esta extensão
No teste em sala, o professor pediu que alguns colegas levassem uma modificação do
código ao deploy e **não conseguiram**. A extensão da Sprint 5 existe para: (A)
destravar o fluxo de contribuição/deploy para todo o time; (B) entregar o **Ciclo de
Moderação** como a função que prova esse fluxo ponta a ponta (a ser avaliada na próxima
semana); e (C) usar a capacidade restante para **fechar dívida e polir** o que já
existe, rumo à entrega final.

## Estratégia (Opção 2 — aprovada pelo líder do grupo)
A feature (Ciclo de Moderação) é pequena para 7 pessoas. Em vez de espremer todos nela
— gerando ociosidade e conflito —, o time roda **duas trilhas em paralelo**:
- **Trilha 1 — Moderação (4 pessoas):** a feature nova end-to-end.
- **Trilha 2 — Hardening (3 pessoas):** fecha issues abertas, limpa débito e prepara a demo.
A **Parte A (prática de deploy)** é coletiva e precede as duas trilhas.

## Decisões já batidas (ciclo de moderação)
- Denunciar nos **dois lados**: detalhe do produto (vendedor + produto) e perfil do vendedor.
- Motivo = **enum** (6 opções, enviado como nome) + **justificativa** em texto (`Descricao`).
- Transições de status **flexíveis, sem reabertura** (método de domínio; inválida → 409).
- Virar admin: **manual no banco Neon** (conta de demonstração) → **relogar** depois.
- Painel **só para admin**, em **2 camadas** (UI esconde + backend `[Authorize(Roles="Admin")]`).
- Painel mostra **nomes resolvidos** (join), não ids.
- **"Minhas denúncias"**: botão discreto no Perfil → página `/minhas-denuncias`. **Stretch.**
- Denunciado **não** é notificado (confidencial; privacidade/LGPD).

## Diagnóstico de entrada (verificado no código)
- `AdminController` `[Authorize(Roles="Admin")]`: PATCH inativar produto, suspender/reativar
  usuário. Suspender seta `IsActive=false` e o `AuthService` bloqueia login. ✓
- `DenunciasController`: só `POST /api/denuncias` (denunciante via token; `Motivo`
  string→enum com `Enum.TryParse`; guard "não denunciar a si mesmo"). ✓
- Entidade `Denuncia`: `DenunciadoId` + `ProdutoId?`; enums de status (4) e motivo (6). ✓
- Role `Admin` (`UserRole.Admin = 2`). ✓
- **Gaps:** ✗ `GET` de listagem de denúncias; ✗ transição de status; ✗ nenhuma UI de denúncia.
- **Hardening disponível:** issues **#89** e **#104** abertas; `EmConstrucao.jsx` órfão
  (follow-up #101); plano da Home A+B pronto (`home-vitrine-3-modulos.md`).

---

## PARTE A — Prática de contribuição e deploy · **todos os 7, no início**
Exercício coletivo antes de abrir as trilhas.
1. **A1 — Diagnóstico (JUNTOS):** onde cada um trava (rodar local? PR? CI? deploy Render? permissões?).
2. **A2 — Onboarding reproduzível:** README + AGENTS §13 + DEPLOY.md sem conhecimento
   tácito; documentar como o deploy acontece; resolver permissões.
3. **A3 — Exercício guiado:** cada pessoa leva uma mudança trivial ao ar (branch → PR →
   CI → merge → deploy), com um "padrinho" em par.
   - **Critério de saída:** todos os 7 veem a própria mudança no deploy. Só então as
     trilhas "valem" como ensaio da avaliação.

---

## TRILHA 1 — Ciclo de Moderação · **4 pessoas**

### Dia 0 (JUNTOS na trilha) — contratos + risco
- [ ] Contrato do DTO do painel (B2 ↔ B3): campos do `GET /admin/denuncias`.
- [ ] Contrato do modal de denúncia (B1): nomes dos motivos + shape do `POST`.
- [ ] **Validar a claim de role no JWT** (`RoleClaimType="role"`) — trava B3 se estiver errado.

| Frente | Pessoas | Escopo | Depende de |
|---|---|---|---|
| **B2 — Backend do painel** ● gargalo | **2** | `GET /admin/denuncias` (nomes resolvidos) + `PATCH .../status` (método de domínio + testes) + guard de duplicada | — (dia 1) |
| **B1 — UI de denúncia** | **1** | Botão + modal no detalhe do produto **e** no perfil do vendedor → `POST /denuncias` | backend já existe (paralelo) |
| **B3 — UI do painel admin** | **1** | Rota só-admin (`<SomenteAdmin>`) + lista + ações (status, suspender/reativar, inativar) | **B2** (mock → integra) |

**Stretch da trilha (se sobrar):** "Minhas denúncias" (`GET /denuncias/minhas` + botão no
Perfil + página `/minhas-denuncias`). Pega quem terminar primeiro (provavelmente B1).

**Perfil ideal:** B2 = 2 mais à vontade com backend/C#/EF; B1 e B3 = React.

**Ordem de merge:** B2 primeiro (destrava B3) → B1 e B3 depois (ambos tocam
`App.jsx`/detalhe/perfil; o segundo rebaseia e mantém as duas partes, AGENTS §9).

---

## TRILHA 2 — Hardening da entrega · **3 pessoas**

Trabalho real já mapeado (issues abertas + débito + plano pronto). Itens independentes,
baixo acoplamento com a Trilha 1.

| Item | Pessoa | Escopo |
|---|---|---|
| **H1 — Issue #89** | **1** (backend) | Destravar inativar/reativar alimento **sem estoque** (o "Esgotado" preso). Remove a trava de front que o #83 deixou. |
| **H2 — Issue #104** | compartilhado com H1 | Robustez do Cloudinary: try/catch best-effort na remoção de imagem antiga nos **3 locais** (Vendedor, Alimento, Bazar). Backend, pequeno. |
| **H3 — Home A+B** | **1** (front) | Banner dos 3 módulos + convite de Ação Solidária, conforme `home-vitrine-3-modulos.md`. **Atenção:** as 2 dúvidas de CTA ainda são decisão de time — confirmar antes de codar. |
| **H4 — Limpeza + demo** | **1** (floater/qualidade) | Remover `EmConstrucao.jsx` órfão (#101); passada de responsivo/acessibilidade; consistência visual entre as telas; **ensaiar o roteiro da demo**; revisar PRs das duas trilhas. |

> H1+H2 são ambos backend e pequenos → cabem na mesma pessoa (ou dupla, se preferir).
> H4 é o papel de qualidade que reduz risco na entrega final.

---

## Cronograma (preencher com datas reais)
```
Dia 1      2      3      4      5      6      7 (entrega)
A    [A1/A2][===== A3: todos até ver no deploy =====]
T1-0        [contratos + claim JWT]
T1-B2        [==== GET + PATCH + testes ====]
T1-B1        [== modal denúncia ==][→ stretch?]
T1-B3             [mock][== integra → painel ==]
T2-H1/H2     [== #89 + #104 (backend) ==]
T2-H3        [== Home A+B ==]
T2-H4        [=== limpeza · qualidade · ensaio demo (contínuo) ===]
Demo                               [ensaio][ buffer ]
```

## Critérios de aceite
**Parte A**
- [ ] Todos os 7 levaram uma mudança ao deploy.

**Trilha 1 — Moderação**
- [ ] Usuário denuncia produto e vendedor (motivo + justificativa).
- [ ] Admin lista denúncias (com nomes) e muda status; inválida → 409.
- [ ] Admin suspende/reativa usuário e inativa produto pela interface.
- [ ] Painel restrito a Admin (rota escondida + endpoints protegidos).
- [ ] (Stretch) `/minhas-denuncias` lista as próprias denúncias.

**Trilha 2 — Hardening**
- [ ] #89 resolvida (inativar/reativar alimento sem estoque; trava de front removida).
- [ ] #104 resolvida (falha do Cloudinary não derruba a edição, nos 3 locais).
- [ ] Home reposicionada (3 módulos + convite Ação Solidária).
- [ ] `EmConstrucao.jsx` removido; demo ensaiada.

**Geral**
- [ ] Builds/testes limpos; tudo demonstrável no deploy.

## Pendências para o time decidir
1. **Nomes por frente** (back/front + disponibilidade de cada um).
2. **Datas** da extensão (preencher o cronograma).
3. **CTA da Home** (H3): "Explorar o UniHub" vs. manter Bazar + CTAs secundários; e
   1 vs. 2 CTAs na linha de Ação Solidária (as 2 dúvidas em aberto do plano da Home).
4. **Guardião de Migrations** (não deve haver migration, mas manter o papel definido).
5. **#89 + #104 na mesma pessoa** (recomendado) ou separados?
