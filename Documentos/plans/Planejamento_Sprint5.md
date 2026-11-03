# Planejamento Sprint 5 — UniHub

## Objetivo
Faltando uma sprint para a entrega final (Sprint 6), o foco é **fechar os módulos
pela interface** e entregar o diferencial técnico (tempo real). As tarefas vêm de
4 BRVs definidas pelo time + a integração de imagens nos produtos.

## Diagnóstico de entrada
- Ação Solidária: backend 100% pronto (14 endpoints), **sem frontend**.
- Vendedor público (ver outro vendedor / anúncios de um vendedor): **endpoint não existe**.
- Restrições alimentares: entidade existe, **sem endpoint de CRUD no perfil** (a confirmar).
- SignalR (tempo real): **nada iniciado**.
- Upload de imagem (Cloudinary): **pronto e reusável**; pasta `produtos` já permitida. Falta só o campo na entidade + UI.

---

## Frente 1 — Módulo de Ação Solidária (frontend) ● carro-chefe
**BRV:** "Frontend — Módulo de Ação Solidária".
Criar a tela do módulo consumindo o backend já pronto:
- Criar solicitação de apoio e acompanhar status.
- Virar voluntário / definir áreas (reusar suporte do Perfil).
- Ver sugestões → aceitar correspondência → revelar identidade (LGPD) → doação → concluir.
- Trocar `/solidariedade` de `EmBreve` pela tela real.

⚠️ **LGPD:** identidade do solicitante só aparece após `revelar-identidade`. UI anônima por padrão.
**Backend:** pronto. **Risco:** médio (volume de telas). **Maior ganho de demonstrabilidade.**

---

## Frente 2 — Painel em Tempo Real (SignalR) 🟠 maior risco
**BRV:** "Painel em Tempo Real".
Painel de vendedores/produtos refletindo disponibilidade **ao vivo** (sem refresh):
- Hub SignalR no backend; notificar quando estoque muda / novo pedido.
- Front assina o hub e atualiza a listagem.

**Backend:** não existe — criar do zero. É o **diferencial técnico do pitch** (a concorrência já é provada por testes; falta a vitrine ao vivo).
**Risco:** alto. **Corte seguro:** se apertar, priorizar as frentes que fecham a demo end-to-end e deixar o tempo real como bônus.

---

## Frente 3 — Frontend: Correções e Detalhes 🟡 integração
**BRV:** "Frontend — Correções e Detalhes". Vários itens:
- Conectar a busca completa + filtros aos anúncios.
- Conectar o histórico de pedidos ao perfil.
- No Perfil: campo de **restrições alimentares** + alternar condição de Vendedor/Voluntário.
- Detalhes do vendedor: ver vendedores, anúncios de um vendedor específico, info do vendedor no anúncio.

⚠️ **Gaps de backend a resolver antes do front:**
- **Listar vendedores / anúncios de um vendedor específico:** o `VendedorController` só tem rotas do *próprio* vendedor. Falta endpoint público → **criar** (ex.: `GET /api/vendedores/{id}` e `GET /api/vendedores/{id}/produtos`).
- **Restrições alimentares:** entidade existe, mas **sem endpoint de CRUD no perfil** → confirmar/criar.

**Risco:** médio (parte é integração; parte exige backend novo).

---

## Frente 4 — Vendedor + Chave PIX no anúncio ○ consumir
**BRV:** "Frontend — Vendedor + Chave PIX".
No anúncio, exibir **nome + chave PIX + imagem de perfil** do vendedor, para o comprador
identificar e pagar na chave certa.
- **Backend pronto** (o detalhe de produto já expõe `ChavePix`; perfil tem foto).
- **Decisão registrada:** expor o PIX no anúncio é **intencional** (requisito de pagamento) — deixa de ser dívida técnica. A UI deve deixar claro que é para pagamento.
**Risco:** baixo.

---

## Frente 5 — Imagem nos produtos (1 foto) 🟡
**Escopo decidido:** **1 imagem por produto**, nos dois módulos (Bazar e Alimentação).
Galeria (múltiplas fotos no Bazar) fica como melhoria futura.
- **Domain:** `string? ImagemUrl` em `Produto` (herda para ItemBazar e Alimento).
- **Migration:** coluna `ImagemUrl` em `Produtos` — ⚠️ com o Guardião de Migrations.
- **Backend:** DTOs (criar/editar/resposta) aceitam/expõem a URL.
- **Front:** formulários (Bazar + Alimento) com seletor → `POST /api/upload/imagem?pasta=produtos` (já existe) → salva a URL; cards e detalhe exibem.
> Upload/Cloudinary já prontos e reusáveis (molde: o fluxo de foto do `Perfil.jsx`).
**Risco:** médio. **Alto impacto visual na demo.**

---

## Resumo das frentes

| Frente | BRV / origem | Backend | Risco |
|---|---|---|---|
| 1 · Ação Solidária (front) | BRV Ação Solidária | pronto | médio |
| 2 · Painel Tempo Real | BRV Painel Tempo Real | **criar (SignalR)** | **alto** |
| 3 · Correções e Detalhes | BRV Correções | parcial (gaps) | médio |
| 4 · Vendedor + PIX no anúncio | BRV Vendedor+PIX | pronto | baixo |
| 5 · Imagem no produto (1 foto) | decisão do time | migration + campo | médio |

## Ordem / dependências
1. **Dia 1 (destravar):** endpoints que faltam para a Frente 3 (listar vendedor público + restrições) e a migration da imagem (Frente 5) — tarefas de backend que bloqueiam front.
2. **Paralelo:** Ação Solidária (maior frente) · SignalR (maior risco) · Vendedor+PIX (rápida) · Imagem.
3. **Integração/qualidade:** contínuo.

## Riscos / corte seguro
- SignalR é o maior risco — se atrasar, as Frentes 1, 3, 4 e 5 já garantem os 3 módulos demonstráveis pela interface. O tempo real é o bônus que eleva a nota.
- Ação Solidária toca módulo sensível (LGPD) — revisar com atenção os endpoints de identidade.

## Definição de pronto da Sprint 5
- [ ] Ação Solidária funcional pela interface (solicitar → voluntário → sugestões → aceitar → revelar → doar → concluir).
- [ ] Anúncio exibe nome, PIX e foto do vendedor (pagamento identificável).
- [ ] Perfil com restrições alimentares e alternância Vendedor/Voluntário; busca/filtros e histórico conectados.
- [ ] Ver vendedores e os anúncios de um vendedor específico.
- [ ] Produtos com 1 imagem (Bazar e Alimentação), via upload existente.
- [ ] (Desejável) Painel do vendedor em tempo real (SignalR).
