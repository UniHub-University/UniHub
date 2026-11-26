# Melhorias Futuras

> Pontos de melhoria mapeados ao longo do desenvolvimento. Nenhum é bloqueante para
> a entrega atual (os 3 módulos funcionam end-to-end, com deploy no ar); são itens
> de qualidade/evolução priorizáveis depois.

## Código / robustez
- **`Doacao` — `if` sem chaves nos métodos de estado:** em `ConfirmarEntrega()` e
  `Cancelar()`, os `if` de validação não usam chaves `{ }`. Funciona hoje (o `throw`
  impede seguir), mas é armadilha de manutenção — adicionar uma linha viraria bug.
  **Resolver:** chaves explícitas. Impacto: baixo.

## Frontend / UX
- **Adotar o sistema de design nas telas restantes (issue #127):** o design system
  (tokens + `.btn` + animações) foi criado e aplicado na Home e no Hub de Ação
  Solidária. Migrar os botões das demais telas para `.btn` dá consistência total.
  Fatiar por módulo (risco visual se feito tudo de uma vez).
- **Dark mode:** registrado como evolução no plano do design system.

## Produto / escopo futuro
- **Painel de moderação — exibir o produto denunciado:** o backend já devolve o nome
  do produto; falta a UI renderizar quando a denúncia for sobre um anúncio.
- **Backlog de features:** Chat entre usuários, Reputação/avaliações, Favoritos,
  notificações — fora do escopo da entrega, candidatos a evolução.

## Resiliência (não-bloqueantes, infra)
- **Suspensão em tempo real:** hoje `IsActive` é checado no login; um usuário já
  logado segue com o token válido até expirar. Mitigação rigorosa seria um middleware
  validando `IsActive` por requisição. Aceitável no escopo atual.
- **Robustez do Cloudinary (resolvido, #104):** falha na remoção de imagem antiga já
  é best-effort e não derruba a edição. Mantido aqui como registro.

> **Histórico:** achados anteriores deste documento (ex.: `Alimento` não mapeado no
> EF, `Correspondencia.Concluir` sem sincronizar) foram **resolvidos** durante o
> desenvolvimento e removidos desta lista.
