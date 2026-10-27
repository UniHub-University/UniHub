## Achados de design (descobertos durante os testes automatizados)

Registrados durante o desenvolvimento da suíte de testes. Escrever os testes
expôs estes pontos — nenhum é escopo do PR de testes; ficam aqui para priorização.

### 1. ● `Alimento` não está mapeado no EF Core
A entidade `Alimento` (subtipo de `Produto`, Table-Per-Hierarchy) existe no domínio,
mas **não está registrada no `AppDbContext`** — falta o `DbSet<Alimento>` e/ou a
configuração do discriminator no `OnModelCreating`.

- **Sintoma:** salvar/ler um `Alimento` pelo EF falha com *"No discriminators matched
  the discriminator value 'Produto'"*. Hoje o único subtipo mapeado é o `ItemBazar`.
- **Impacto:** ALTO — Alimentação é o módulo carro-chefe e está não-funcional na camada
  de dados. Conecta com a lacuna já conhecida do "controller de Alimentação": o problema
  começa na entidade, não só na API.
- **Como resolver:** registrar `Alimento` no EF (`DbSet<Alimento>` +
  `HasDiscriminator`/`HasValue`) e gerar a migration. [SCHEMA] — coordenar com o
  Guardião de Migrations e testar na branch development do Neon antes da produção.
- **Workaround atual:** os testes de produto usam `ItemBazar` (que funciona).

### 2. 🟡 `Doacao` — `if` sem chaves nos métodos de estado
Nos métodos `ConfirmarEntrega()` e `Cancelar()` de `Doacao`, o `if` de validação não
usa chaves `{ }`.

- **Sintoma:** funciona hoje por acaso (o `throw` impede chegar na linha seguinte quando
  inválido), mas a indentação sugere que a mudança de status estaria dentro do `if` —
  engana e vira bug se alguém adicionar uma linha.
- **Impacto:** BAIXO (funciona), mas é armadilha de manutenção.
- **Como resolver:** adicionar chaves `{ }` explícitas nos `if`.

### 3. 🟡 `Correspondencia.Concluir()` — conclusão sem sincronizar a solicitação
Em `Correspondencia.Concluir()`, a chamada `Solicitacao?.Concluir()` é condicional (`?.`).
Se a `Solicitacao` não estiver carregada (null), a correspondência é marcada como
`Concluida` **sem** concluir a solicitação associada.

- **Sintoma:** possível inconsistência entre o estado da correspondência e o da solicitação.
- **Impacto:** MÉDIO — depende de o fluxo sempre carregar a `Solicitacao` antes.
- **Como resolver:** decidir com o squad — exigir a `Solicitacao` carregada (remover o
  `?.` e validar) ou documentar que a sincronização é responsabilidade do chamador.
