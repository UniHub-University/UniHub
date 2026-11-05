# Planejamento Sprint 5 — UniHub

## Objetivo
Faltando uma sprint para a entrega final (Sprint 6), o foco é **fechar os módulos
pela interface** e entregar o diferencial técnico (tempo real). As tarefas vêm de
4 BRVs definidas pelo time + a integração de imagens nos produtos.

> **Equipe:** 7 pessoas. A alocação abaixo concentra gente nas frentes grandes
> (Ação Solidária e SignalR) e deixa uma pessoa como ponto flutuante/qualidade.

## Já concluído no fim da Sprint 4 (não entra nesta sprint)
- **Gestão de Alimento pela UI do vendedor** (criar/editar/inativar) — entregue no
  PR #72. Sobraram apenas dois ajustes pequenos (ver Frente 3).

## Diagnóstico de entrada
- Ação Solidária: backend 100% pronto (14 endpoints), **sem frontend**.
- Vendedor público (ver outro vendedor / anúncios de um vendedor): **endpoint não existe**.
- Restrições alimentares: entidade existe, **sem endpoint de CRUD no perfil** (a confirmar).
- SignalR (tempo real): **nada iniciado**.
- Upload de imagem (Cloudinary): **pronto e reusável**; pasta `produtos` já permitida.
  Falta só o campo na entidade + UI.
- `reativarAlimento` existe no service do front, mas **sem botão na UI** (resto da gestão de alimento).

---

## Frente 1 — Módulo de Ação Solidária (frontend) ● carro-chefe · **2 pessoas**
**BRV:** "Frontend — Módulo de Ação Solidária".
Criar a tela do módulo consumindo o backend já pronto:
- Criar solicitação de apoio e acompanhar status.
- Virar voluntário / definir áreas (reusar suporte do Perfil).
- Ver sugestões → aceitar correspondência → revelar identidade (LGPD) → doação → concluir.
- Trocar `/solidariedade` de `EmBreve` pela tela real.

⚠️ **LGPD:** identidade do solicitante só aparece após `revelar-identidade`. UI anônima por padrão.
**Backend:** pronto. **Risco:** médio (volume de telas). **Maior ganho de demonstrabilidade.**
**Por que 2 pessoas:** é a maior frente da sprint e o fluxo tem muitas telas
(solicitar, sugestões, correspondência, doação) — dá para dividir bem (ex.: uma no
lado do solicitante, outra no lado do voluntário).

> **Gap de backend atribuído (pós-#74):** o endpoint `GET minhas solicitações`
> (listar as solicitações do próprio usuário para a tela de acompanhamento) faltava
> no backend de Ação Solidária. **Dono: Lucas** (Frente Ação Solidária). Fazer no dia 1
> para não travar a tela de acompanhamento de status.

---

## Frente 2 — Painel em Tempo Real (SignalR) 🟠 maior risco · **2 pessoas**
**BRV:** "Painel em Tempo Real".
Painel de vendedores/produtos refletindo disponibilidade **ao vivo** (sem refresh):
- Hub SignalR no backend; notificar quando estoque muda / novo pedido.
- Front assina o hub e atualiza a listagem.

**Backend:** não existe — criar do zero. É o **diferencial técnico do pitch**.
**Risco:** alto. **Por que 2 pessoas:** é full-stack (hub no backend + integração no
front) e tecnologia nova para o time — vale uma dupla (uma focada no backend/hub,
outra no consumo no front). **Corte seguro:** se apertar, as Frentes 1, 3, 4 e 5 já
garantem os 3 módulos demonstráveis; o tempo real é o bônus que eleva a nota.

---

## Frente 3 — Frontend: Correções e Detalhes 🟡 integração · **1 pessoa**
**BRV:** "Frontend — Correções e Detalhes" + restos da gestão de alimento. Itens:
- Conectar a busca completa + filtros aos anúncios.
- Conectar o histórico de pedidos ao perfil.
- No Perfil: campo de **restrições alimentares** + alternar condição de Vendedor/Voluntário.
- Detalhes do vendedor: ver vendedores, anúncios de um vendedor específico, info do vendedor no anúncio.
- **Resto da gestão de alimento:** expor o botão "reativar" na UI (service já existe) e
  ajustar o texto "Excluir" → "Inativar" (reversível) no botão de alimento.

⚠️ **Gaps de backend a resolver antes do front (dia 1):**
- **Listar vendedores / anúncios de um vendedor específico:** o `VendedorController` só
  tem rotas do *próprio* vendedor. Falta endpoint público → **criar** (ex.:
  `GET /api/vendedores/{id}` e `GET /api/vendedores/{id}/produtos`).
- **Restrições alimentares:** entidade existe, mas **sem endpoint de CRUD no perfil** →
  confirmar/criar.

**Risco:** médio (parte é integração; parte exige backend novo). **Por que 1 pessoa:**
são vários itens pequenos e independentes; cabe em uma pessoa focada, com apoio do
floater nos gaps de backend.

---

## Frente 4 — Vendedor + Chave PIX no anúncio ○ consumir · **1 pessoa (depois vira floater)**
**BRV:** "Frontend — Vendedor + Chave PIX".
No anúncio, exibir **nome + chave PIX + imagem de perfil** do vendedor, para o comprador
identificar e pagar na chave certa.
- **Backend pronto** (o detalhe de produto já expõe `ChavePix`; perfil tem foto).
- **Decisão registrada:** expor o PIX no anúncio é **intencional** (requisito de
  pagamento) — não é dívida técnica. A UI deve deixar claro que é para pagamento.

**Risco:** baixo. **Por que 1 pessoa:** é pequena e sem bloqueios; essa pessoa termina
cedo e assume o papel de **floater** — apoia os gaps de backend da Frente 3, revisa PRs,
cobre testes e socorre a frente que travar.

---

## Frente 5 — Imagem nos produtos (1 foto) 🟡 · **1 pessoa**
**Escopo decidido:** **1 imagem por produto**, nos dois módulos (Bazar e Alimentação).
Galeria (múltiplas fotos no Bazar) fica como melhoria futura.
- **Domain:** `string? ImagemUrl` em `Produto` (herda para ItemBazar e Alimento).
- **Migration:** coluna `ImagemUrl` em `Produtos` — ⚠️ com o Guardião de Migrations.
- **Backend:** DTOs (criar/editar/resposta) aceitam/expõem a URL.
- **Front:** formulários (Bazar + Alimento) com seletor → `POST /api/upload/imagem?pasta=produtos`
  (já existe) → salva a URL; cards e detalhe exibem.
> Upload/Cloudinary já prontos e reusáveis (molde: o fluxo de foto do `Perfil.jsx`).

**Risco:** médio. **Alto impacto visual na demo.** **Por que 1 pessoa:** full-stack mas
delimitada (campo + migration + UI dos formulários/cards), com o upload já pronto.

---

## Resumo da alocação (7 pessoas)

| Frente | Pessoas | Backend | Risco |
|---|---|---|---|
| 1 · Ação Solidária (front) | **2** | pronto | médio |
| 2 · Painel Tempo Real (SignalR) | **2** | criar do zero | **alto** |
| 3 · Correções e Detalhes (+ restos alimento) | **1** | parcial (gaps) | médio |
| 4 · Vendedor + PIX no anúncio → floater | **1** | pronto | baixo |
| 5 · Imagem no produto (1 foto) | **1** | migration + campo | médio |
| **Total** | **7** | | |

> O **floater** (pessoa da Frente 4, após terminar) é quem destrava os gaps de
> backend da Frente 3, mantém a cobertura de testes e revisa os PRs.

## Ordem / dependências
1. **Dia 1 (destravar):** endpoints que faltam para a Frente 3 (listar vendedor público
   + restrições) e a migration da imagem (Frente 5) — tarefas de backend que bloqueiam front.
2. **Paralelo:** Ação Solidária (maior frente) · SignalR (maior risco) · PIX (rápida) ·
   Imagem · Correções.
3. **Integração/qualidade:** contínuo, com o floater.

## Riscos / corte seguro
- SignalR é o maior risco — se atrasar, as Frentes 1, 3, 4 e 5 já garantem os 3 módulos
  demonstráveis pela interface. O tempo real é o bônus que eleva a nota.
- Ação Solidária toca módulo sensível (LGPD) — revisar com atenção os endpoints de identidade.

## Definição de pronto da Sprint 5
- [ ] Ação Solidária funcional pela interface (solicitar → voluntário → sugestões →
      aceitar → revelar → doar → concluir).
- [ ] Anúncio exibe nome, PIX e foto do vendedor (pagamento identificável).
- [ ] Perfil com restrições alimentares e alternância Vendedor/Voluntário; busca/filtros
      e histórico conectados.
- [ ] Ver vendedores e os anúncios de um vendedor específico.
- [ ] Produtos com 1 imagem (Bazar e Alimentação), via upload existente.
- [ ] Reativar alimento pela UI + texto do botão ajustado.
- [ ] (Desejável) Painel do vendedor em tempo real (SignalR).
