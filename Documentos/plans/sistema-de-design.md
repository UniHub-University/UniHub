# Plano — Sistema de Design (refino visual do tema global)

## Objetivo
Evoluir a estilização do UniHub para um visual mais moderno, convidativo e coeso,
mantendo a identidade verde institucional (UNIFESP). Fundação de design tokens +
sistema de botões, sem quebrar nenhuma tela existente.

## Escopo
- **Inclui:**
  - Camada 1: escala completa de tokens de cor (verdes, neutros, tan, creme),
    um "verde de ação" vivo para CTAs/estados ativos, e tokens de raio, sombra,
    espaçamento e transição em degraus — tudo em `index.css`.
  - Camada 2: sistema de botões global (`.btn`, `.btn--primario`, `.btn--secundario`,
    `.btn--sutil`, tamanhos) em `index.css`.
  - Camada 3: refino da Home (`Home.css`) e do Header (`LayoutApp.css`) consumindo os
    tokens novos — CTA do banner e link ativo do header com o verde de ação, hovers
    com elevação suave, hex soltos trocados por tokens. (Incluída após o merge do
    PR #111 da Home, que liberou os arquivos `Home.css`/`index.css` sem risco de conflito.)
- **NÃO inclui:**
  - Dark mode (Camada 4) — avaliar depois.
  - Reescrever todas as 32 folhas de estilo. As variáveis legadas
    (`--cor-verde`, etc.) continuam existindo apontando para a escala nova, então
    as telas não tocadas seguem funcionando iguais.
  - Qualquer mudança de comportamento/JSX que não seja troca de classe/estilo.

## Decisões de design
- **Compatibilidade retroativa:** as variáveis antigas (`--cor-verde`,
  `--cor-verde-escuro`, `--cor-tan`, `--cor-creme`, `--cor-superficie`, `--cor-erro`,
  `--raio`, `--sombra`) são mantidas, agora definidas em função da escala nova.
  Zero tela quebra; migração das demais telas fica incremental e futura.
- **Paleta derivada da LOGO (decisão revista).** A 1ª tentativa usou um verde-grama
  vivo (`#3f8f2e`) para os CTAs; foi **descartado** por destoar da logo (verde floresta
  + dourado + creme). Direção correta: a interface serve à marca, não o contrário — a
  logo NÃO muda. O destaque dos CTAs vem de:
  1. **Verde da logo** (`#2a4515`) como ação primária, com profundidade (gradiente
     sutil + sombra verde + filete dourado no hover) em vez de cor saturada.
  2. **Dourado da logo** (`#b19563`) promovido a cor de ACENTO/destaque (tokens
     `--destaque-*`): CTA alternativo sobre fundos verdes (banner), anel do link
     ativo, ícones, selos. Quente e convidativo, 100% da identidade.
- **Escala de tokens:** verdes 50→900, neutros 50→900, raios (sm/md/lg/xl/full),
  sombras (sm/md/lg), espaçamento (1→8) e transições padronizadas.
- **Sistema de botões:** base `.btn` + variantes; hover com leve elevação e transição,
  respeitando `prefers-reduced-motion`. Alvo de toque mínimo 44px (acessibilidade).
- **Trade-offs considerados:**
  - *Refatorar todas as telas agora* vs *camada de compatibilidade*: escolhida a
    segunda — menor risco, PR pequeno (§9), migração incremental.
  - *Verde único* vs *verde texto + verde ação*: escolhido o par, para criar
    hierarquia visual (energia nos CTAs sem perder a sobriedade institucional).

## Passos de implementação
1. `Frontend/src/index.css` — escala de tokens + aliases legados + sistema de botões
   (inclui `scroll-behavior: smooth` global com guarda de `prefers-reduced-motion`).
2. `Frontend/src/pages/Home.css` — CTA do banner e "ver todos" com verde de ação e
   hover de elevação; hex soltos trocados por tokens; raios/sombras via tokens.
3. `Frontend/src/components/LayoutApp.css` — link ativo do header com verde da logo +
   anel dourado de acento; transições via tokens; newline final corrigido (§4).
4. `index.css` — utilitários de animação reutilizáveis (`.anim-surgir` e `.anim-fade`
   + atrasos escalonados, `.elevar` para hover de cartões) com guarda de
   `prefers-reduced-motion`, trazendo dinamismo sem repetir keyframes por tela.
   `.anim-fade` (só opacidade) é a versão segura para elementos que já usam
   `transform` (ex.: cartões rotacionados do banner), onde `.anim-surgir` apagaria
   a rotação.
5. Aplicação de dinamismo nas telas (sem mudar lógica/JSX além de classe):
   - `Home.jsx`: conteúdo do banner com `.anim-surgir`; cartões flutuantes com
     `.anim-fade` escalonado (`anim-atraso-1/2/3`).
   - `SolidariedadeHub.jsx`: cartões de atalho com `.elevar`.
6. Remoção do filete dourado do item ativo da navegação e do hover do `.btn--primario`
   (feedback do time: o filete mínimo não agradou).
7. Validar no front: `npm run lint` e `npm run build` limpos; conferir visual no `npm run dev`.

## Fora deste PR (próximos, incrementais)
- Migração dos botões das telas (Bazar, formulários, perfil) para o sistema `.btn`:
  é PR próprio (§9 PRs pequenos, §0.2 não refatorar além do necessário); cada tela tem
  `<button>` com estados/loading que exigem inspeção individual.
- Dark mode (Camada 4).

## Critérios de aceite
- [ ] Nenhuma tela existente quebra (aliases legados preservados).
- [ ] CTAs e estado ativo usam o verde de ação; hierarquia visual perceptível.
- [ ] Raios/sombras padronizados via tokens.
- [ ] `.btn` disponível globalmente (base para migração futura das telas).
- [ ] CTA do banner da Home e link ativo do header usam o verde de ação.
- [ ] `npm run lint` limpo.
- [ ] `npm run build` limpo.
- [ ] `prefers-reduced-motion` respeitado nos novos hovers.
- [ ] Newline final em todos os arquivos tocados (§4).

## Pendências / dúvidas em aberto
- Hex exatos da logo a confirmar com color picker (hoje assumidos: verde `#2a4515`,
  dourado `#b19563`, creme `#fef6c7` — batem com o tema atual).
- Dark mode (Camada 4) fica para um PR futuro, se o time quiser.
- Migração incremental dos botões das telas para o sistema `.btn` (PR futuro).
