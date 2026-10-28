# Planejamento Sprint 4 — Extensão (+1 semana)

7 membros, 4 frentes de desenvolvimento + bloqueantes + ponto flutuante

## Por que esta extensão

A Sprint 4 destravou o frontend (login, Home/feed, Bazar, perfil, histórico de
pedidos) e entregou a suíte de testes de concorrência. A semana extra existe
para **completar as frentes que faltam para os 3 módulos ficarem demonstráveis
até a Sprint 6**: o módulo de Alimentação (hoje sem tela e com a entidade nem
mapeada), os perfis de Vendedor e Voluntário, e o fluxo de Comprar/Reservar com
baixa pelo vendedor.

A divisão abaixo respeita a **especialização** de cada pessoa e a
**disponibilidade real nesta semana**: quem tem menos tempo fica em tarefas
menores, bem delimitadas e fora do caminho crítico.

---

## Diagnóstico de entrada (estado atual do backend)

| Área | Estado | Gap |
|---|---|---|
| Alimentação | Entidade `Alimento : Produto` existe | **NÃO mapeada no EF** (bug); sem controller/service/DTOs/front |
| Vendedor | `VendedorController` (virar vendedor, logística, info) + CRUD de `ItemBazar` com checagem de dono | Sem endpoint "meus produtos"; todo o front |
| Voluntário | Entidade mapeada + `Usuario.TornarSeVoluntario()` pronto | **Sem `VoluntarioController`**; todo o front |
| Pedido / Reserva | Reserva com decremento atômico de estoque (concorrência) | Sem distinção Comprar/Reservar; sem expiração; sem baixa pelo vendedor; sem transições de status |

---

## Decisões de design (fechadas com o time)

- **Comprar vs Reservar:** campo `TipoPedido` (`Compra = 0` | `Reserva = 1`) na entidade `Pedido`.
- **Reserva é temporária** e só vale para `ItemBazar` (não Alimento).
- **Expiração em tempo real:** um `BackgroundService` devolve o estoque (reusando
  `IncrementarEstoqueAsync`) e marca o pedido como `Expirado` quando `ExpiraEm`
  vence. **Rede de segurança:** checagem *lazy* nos GETs de pedido, para resolver
  reservas vencidas mesmo que o job tenha hibernado no Render.
- **Sem ecossistema de notificação:** o aviso ao usuário é o status `Expirado`
  aparecendo no histórico dele. (Notificação em tempo real fica para o backlog.)
- **Baixa manual do vendedor:** apenas conclui o pedido (`Reservado → Concluido`).

---

## Disponibilidade nesta semana

| Disponibilidade | Membros | Estratégia |
|---|---|---|
| **Maior** | Henrique, Letícia, Nicolas, GVBVaz | Absorvem o caminho crítico e as frentes pesadas |
| **Menor** | Pedro, Maria, Lucas | Tarefas menores, independentes e fora do caminho crítico |

> Pedro, Maria e Lucas foram alocados juntos na frente de **menor risco** (Vendedor),
> cada um com uma **fatia pequena e autocontida**. Se algum não conseguir dedicar a
> semana, o impacto fica contido e nenhuma outra frente trava.

---

## Bloqueantes (Dia 1 — destravam as frentes)

| ID | Tarefa | Destrava |
|---|---|---|
| **B0** | Corrigir enum `UserRole` (`Voluntario` e `Admin` colidem em `1` → `Aluno=0, Voluntario=1, Admin=2`) | Voluntário |
| **B1** | Mapear `Alimento` no EF (TPH `.HasValue<Alimento>("Alimento")`) + migration | Alimentação + filtro `tipo=lanche` do BRV-6 |
| **B2** | Migration do Pedido: `+ Tipo (TipoPedido)`, `+ ExpiraEm (DateTime?)`, `PedidoStatus + Expirado` | Comprar/Reservar |

> Ideal: B1 + B2 numa **única migration**, pela pessoa dos bloqueantes, com o
> processo de Guardião de Migrations e testada numa branch do Neon.

---

## Squad A — Alimentação (Letícia, com apoio do Floater) · depende de B1

| Frente | Entregável |
|---|---|
| Backend | DTOs de Alimento; `AlimentoController` + CRUD (molde: `ItensBazarController`, com checagem de dono); validação do `HorarioDisponibilidade` no service |
| Frontend | Aba Alimentação (hoje `<EmBreve>`) consumindo o CRUD |

**Por quê a Letícia:** tem disponibilidade e já é full-stack (entregou o Perfil).
O ponto flutuante (Henrique) apoia — como ele faz o B1, já estará com o contexto
do `Alimento` fresco. Se o trio do Vendedor liberar antes, quem sobrar ajuda no
front desta frente.

---

## Squad B — Comprar / Reservar / Baixa / Expiração (Nicolas + GVBVaz) · depende de B2

A frente mais pesada e de maior risco (modelagem + `BackgroundService`).

| ID | Entregável | Responsável |
|---|---|---|
| P2 | `CreatePedidoDto.Tipo` (Compra/Reserva); validar que **Reserva só para `ItemBazar`** | Nicolas |
| P3 | Ao criar Reserva: setar `ExpiraEm = now + TTL` | Floater/Nicolas |
| P4 | `BackgroundService` de expiração (devolve estoque + marca `Expirado`) | Floater |
| P5 | Checagem *lazy* nos GETs de pedido (rede de segurança anti-hibernação do Render) | Floater |
| P6 | `GET /api/pedidos/vendedor` — pedidos dos produtos do vendedor logado | Nicolas |
| P7 | `POST /api/pedidos/{id}/concluir` — baixa manual `Reservado → Concluido` (checar dono) | Nicolas |
| P8 | Front: botões "Comprar" e "Reservar" (Reservar só em ItemBazar) | GVBVaz |
| P9 | Front: histórico do comprador mostrando `Expirado`; histórico do vendedor com botão "Concluir" | GVBVaz |

**Por quê Nicolas + GVBVaz:** Nicolas já conhece `Pedido` (fez o histórico);
GVBVaz pega o front. Os dois têm disponibilidade para a frente de maior risco.
O ponto flutuante (Henrique) reforça a parte de estoque/expiração (P3/P4/P5),
por afinidade com a lógica de concorrência.

---

## Squad C — Vendedor (Pedro + Maria + Lucas) · sem bloqueante

Frente de **menor risco**, fatiada em três partes independentes — uma por pessoa
do grupo de menor disponibilidade. **Sem bloqueante: pode começar no Dia 1.**

| Fatia | Entregável | Responsável |
|---|---|---|
| C1 | **Backend** `GET /api/perfil/vendedor/produtos` (lista os próprios produtos, filtro por `VendedorId`) + front do botão "tornar-se vendedor" (`POST /api/perfil/vendedor` já existe) | Lucas |
| C2 | **Front** aba "Meus Produtos" (listagem; consome o endpoint C1) | Pedro |
| C3 | **Front** formulários de cadastro / edição / exclusão (CRUD de `ItemBazar` já existe com checagem de dono) | Maria |

**Por quê fatiado em 3:** o backend é leve e o CRUD já existe; o esforço é de
front e divide bem. Cada fatia é pequena e autocontida, então um eventual atraso
de quem tem menos tempo não trava os demais nem o resto da sprint.

---

## Papel — Bloqueantes + Voluntário + Ponto Flutuante (Henrique)

Dono dos **bloqueantes B0/B1/B2** (entregues no Dia 1, sem exceção — eles travam
três das quatro frentes), do **fechamento do PR #51** (suíte de testes), da
**cobertura de testes dos novos services** e da frente leve de **Voluntário**.
Atua como **ponto flutuante** entre os squads.

### Voluntário (frente leve absorvida pelo Floater)

| Frente | Entregável |
|---|---|
| Backend | `VoluntarioController` + `POST /api/perfil/voluntario` (molde: `VendedorController.TornarSeVendedor`; `Usuario.TornarSeVoluntario()` já existe) |
| Frontend | Botão "tornar-se voluntário" na tela de perfil |

### Critério de acionamento do Floater
(para o flutuante não virar gargalo nem apagar incêndio aleatório)
1. Decisões de **migration / arquitetura**.
2. Frente **travada há mais de meio dia**.
3. **PR pronto para review**.

Primeiro reforço da **Frente B** (expiração / `BackgroundService`) e apoio à
**Frente A** (Alimentação).

### Ordem de prioridade do Henrique
1. B0/B1/B2 + #51 (destravam o resto).
2. Voluntário (leve, backend) + tarefas P3/P4/P5 da Frente B.
3. Review e testes — contínuo, encaixando nos intervalos.

---

## Resumo da alocação

| Squad / Papel | Pessoas | Disponib. | Depende de | Risco |
|---|---|---|---|---|
| Bloqueantes + Voluntário + Floater (Henrique) | 1 | alta | — | crítico (mas é o floater) |
| A · Alimentação (Letícia + apoio) | 1 (+floater) | alta | B1 | médio |
| B · Comprar/Reservar (Nicolas + GVBVaz) | 2 | alta | B2 | **alto** |
| C · Vendedor (Pedro + Maria + Lucas) | 3 | **baixa** | — | **baixo** ✓ |

---

## Ordem de execução (dependências)

```
Dia 1:
  Henrique → B0 + B1 + B2 (migrations)              ─┐ destravam ↓
  Pedro + Maria + Lucas → Vendedor (sem bloqueante) ─┘ (já começam)

Dia 2 em diante (paralelo):
  Letícia          → Alimentação      (precisa de B1)
  Nicolas + GVBVaz → Comprar/Reservar (precisa de B2)
  Henrique         → Voluntário + #51 + testes + review + floater (contínuo)
```

---

## Riscos / corte seguro

A **Frente B** é o maior risco (BackgroundService + semântica Comprar/Reservar).
Se o prazo apertar, o corte seguro é entregar **Comprar/Reservar + baixa manual +
expiração *lazy* (P5)** e deixar o job (P4) como *nice-to-have* — a feature
funciona sem ele, só não expira em tempo real quando o app está ocioso.

O trio de menor disponibilidade está na frente de menor risco (Vendedor), com
fatias independentes — se alguém não conseguir dedicar a semana, o impacto é
contido.

---

## Dívida técnica tocada nesta extensão

- `UserRole` com `Voluntario`/`Admin` colidindo em `1` (corrigido em B0 — registrar em `MELHORIAS_FUTURAS.md` caso escape).
- `Alimento` não mapeado no EF (corrigido em B1).

---

## Definição de pronto da extensão

- [ ] Módulo de Alimentação funcional ponta a ponta (CRUD + tela), com `Alimento` mapeado no EF.
- [ ] Vendedor: virar vendedor, aba "Meus Produtos" e CRUD (cadastro/edição/exclusão) pela tela.
- [ ] Voluntário: virar voluntário via API e pela tela de perfil.
- [ ] Pedido com `Compra` e `Reserva`; Reserva temporária só para `ItemBazar`.
- [ ] Reserva expira (devolve estoque + status `Expirado`) e aparece no histórico do comprador.
- [ ] Vendedor vê as reservas dos seus produtos e conclui manualmente (`Reservado → Concluido`).
- [ ] PR #51 (testes) mergeado na main; novos services cobertos por teste.
