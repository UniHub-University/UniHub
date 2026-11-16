# Plano — Home: banner dos 3 módulos + linha de Ação Solidária (A+B)

## Objetivo
A Home hoje tem um banner lindo, mas que fala **só do Bazar**, e vitrine de 2
módulos (Bazar, Alimentação). Surgiu a sugestão de **retirar** o banner por estar
desalinhado. Em vez de descartar um ativo pronto, vamos **reposicioná-lo** para
representar o UniHub inteiro (3 módulos) e completar a narrativa com uma entrada
para Ação Solidária — sem criar feature nova, alinhado à meta da Sprint 6 (produto
demonstrável com os 3 módulos visíveis).

## Escopo
- **Inclui (A):** reescrever o conteúdo do banner hero para falar da plataforma
  (não de um módulo) — descrição, 3 cartões flutuantes (um por módulo) e CTA neutro.
- **Inclui (B):** adicionar uma **linha de Ação Solidária** na Home, como **convite
  institucional** que leva ao hub `/solidariedade`, NÃO como vitrine de solicitações.
- **NÃO inclui:**
  - Listar solicitações de apoio reais na Home (**proibido por LGPD — ver abaixo**).
  - Qualquer mudança de backend, endpoint ou schema.
  - O contador de estatísticas da comunidade (fica documentado como evolução — Opção C).
  - Mexer no `LinhaVitrine` de Bazar/Alimentação (já entregue no #100).

## Decisões de design

### A — Banner hero (só conteúdo, arte preservada)
- **Descrição:** reescrever para abraçar os 3 módulos (circular o que não se usa +
  comida da comunidade + apoiar quem precisa), em vez de só "dar nova história ao que
  você não usa".
- **Cartões flutuantes:** hoje são 2 do Bazar (`--livro`, `--sacola`). Passar a **3**,
  um por módulo: ▪ Bazar, ☕ Alimentação, 🤝 Ação Solidária. Reusar o estilo
  `.home__banner-cartao--*` existente, criando um 3º modificador no mesmo molde.
- **CTA:** trocar "Explorar o Bazar" por algo da plataforma (ex.: "Explorar o UniHub"
  rolando para a vitrine) OU manter o CTA do Bazar e adicionar CTAs secundários. A
  decidir na implementação (ver dúvidas em aberto).

### B — Linha de Ação Solidária (convite, não vitrine)
- Reusar o padrão visual das seções (`.home__secao--<tom>`), criando um 3º tom
  (ex.: `--solidariedade`) no mesmo molde dos tons de Bazar/Alimentação.
- **Conteúdo:** título + frase de apoio + CTAs que levam às telas reais já existentes:
  - "Precisa de apoio?" → `/solidariedade/solicitar`
  - "Quero ser voluntário" → `/solidariedade/voluntario`
  - ou um "Ver todos" único → `/solidariedade` (o `SolidariedadeHub`, que já tem esses atalhos).
- **NÃO** usa `LinhaVitrine` com dados: é um bloco de convite estático (sem `buscar`),
  porque não há — e não pode haver — "produtos" a listar aqui.

### ⚠️ Impacto LGPD (AGENTS §8) — decisão central
A Ação Solidária protege a identidade do solicitante (`CodigoPublico`). Portanto a
Home **nunca** pode exibir solicitações reais como vitrine (exporia pessoas em
necessidade). A linha é **institucional/convite** — leva o usuário a *entrar* no
módulo, não mostra quem pediu ajuda. Sem `.Include`, sem fetch de solicitações.

### Trade-offs considerados
- **Reposicionar o banner vs. retirar:** reaproveita um ativo pronto e bonito,
  corrige o desalinhamento com custo de só texto/estilo. Escolhido.
- **Convite vs. vitrine de solicitações (Ação Solidária):** vitrine violaria LGPD;
  convite respeita a privacidade e ainda cumpre o objetivo de dar visibilidade ao
  módulo. Escolhido.
- **Bloco estático vs. reusar `LinhaVitrine`:** o `LinhaVitrine` pressupõe uma lista
  de itens; Ação Solidária não tem itens públicos → bloco de convite próprio.

## Passos de implementação (só frontend)
1. `Home.jsx` — reescrever o conteúdo do banner (descrição, 3º cartão, CTA).
2. `Home.css` — 3º modificador de cartão (`--solidariedade`) e 3º tom de seção,
   reusando os padrões existentes (`--bazar`/`--alimentacao`).
3. `Home.jsx` — adicionar o bloco/seção de convite de Ação Solidária (links para
   `/solidariedade/solicitar`, `/solidariedade/voluntario` ou `/solidariedade`).
4. Validar acessibilidade (ícones `aria-hidden`, seção com `aria-labelledby`, foco
   visível nos CTAs — mesmo padrão já usado na Home).

## Critérios de aceite
- [ ] Banner fala da **plataforma** (3 módulos), não só do Bazar; 3 cartões, um por módulo.
- [ ] Home tem uma entrada de **Ação Solidária** que leva às telas do módulo.
- [ ] A entrada de Ação Solidária **não exibe** nenhuma solicitação real (LGPD).
- [ ] Nenhuma mudança de backend/endpoint/schema.
- [ ] `npm run lint` e `npm run build` limpos; fluxo validado no `npm run dev`.
- [ ] Arquivos tocados terminam com newline final (AGENTS §4).

## Evolução futura — Opção C (documentada, fora deste escopo)
**Banner com estatística viva da comunidade.** Substituir/complementar a arte do
banner por um contador real: "X itens circulando · Y refeições compartilhadas ·
Z conexões solidárias". É o que mais impressiona numa demo/pitch (dado real).
- **Requer backend:** endpoint de agregação (ex.: `GET /api/estatisticas/home`)
  com contagens — possivelmente cacheado, pois é leitura pública e frequente.
- **Cuidado LGPD:** só números agregados, nunca identificáveis.
- **Por que fica para depois:** é a única opção que exige backend novo; A+B já
  resolvem o desalinhamento e completam os 3 módulos sem isso. Candidata a
  pós-Sprint 6 ou a uma sprint com folga.

## Pendências / dúvidas em aberto
- **CTA do banner:** "Explorar o UniHub" (rola para a vitrine) vs. manter "Explorar o
  Bazar" + CTAs secundários? Decidir com o time/orientador.
- **Linha de Ação Solidária:** dois CTAs (solicitar / voluntário) vs. um único "Ver
  Ação Solidária" → `/solidariedade`? O hub já oferece os dois atalhos — talvez um
  CTA único seja mais limpo.
- Confirmar com quem sugeriu **retirar** o banner que o reposicionamento (A) endereça
  a preocupação dele.
