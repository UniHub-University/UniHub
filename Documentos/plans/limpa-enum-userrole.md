# Plano — Limpeza do UserRole + fail-safe + tag de admin no perfil

> Extensão da Sprint 5 · toca a modelagem de `UserRole`. Documentado após decisão
> em equipe (surgiu de uma dúvida sobre a tag de papel no perfil).

## Objetivo
1. Remover o valor morto `Voluntario` do enum `UserRole` (ser voluntário é
   representado pela existência do perfil `Usuario.Voluntario`, nunca por role —
   o enum tinha `Voluntario = 1` sem nunca ser usado como role).
2. Adicionar um fail-safe de segurança: um `Role` com valor desconhecido degrada
   para `Aluno` (menor privilégio), nunca concede admin por acidente.
3. Melhorar a UI do perfil: admin passa a exibir **duas tags** (Aluno + Admin),
   a de admin destacada.

## Escopo
- **Inclui:** ajuste do enum, método `RoleEfetivo` (fail-safe), uso dele na claim
  e no PerfilDto, dupla-tag no front, teste do fail-safe, atualização do AGENTS §15.
- **NÃO inclui:** mudança de schema/migration (a coluna `Role` continua `int`);
  renumeração de valores (Admin mantém `2`); tags de vendedor/voluntário no perfil
  (seguem representadas pelos botões "tornar-se…", para evitar duplicação).

## Decisões de design

### Enum: remover `Voluntario`, manter `Admin = 2`
```csharp
public enum UserRole { Aluno = 0, Admin = 2 }  // 1 (Voluntario) removido
```
- **Por que manter `Admin = 2` (buraco no 1):** o enum é serializado como **int** no
  banco. Renumerar `Admin` para `1` exigiria migrar os dados (`UPDATE Role 2→1`) com
  uma janela de inconsistência (admin quebrado durante o deploy). Verificado no banco
  (dev e prod): existem apenas `Role = 0` (alunos) e `Role = 2` (admins); **nenhum
  `Role = 1`**. Logo, manter `Admin = 2` bate com os dados existentes — **zero migração,
  zero risco**. O buraco no `1` é cosmético e inofensivo.

### Fail-safe de segurança (`Usuario.RoleEfetivo`)
Método na entidade que normaliza o `Role`: se o valor não for um valor conhecido do
enum (ex.: um `1` órfão ou lixo gravado direto no banco), retorna `Aluno`. Usado na
**claim do token** (ponto que autoriza) e no **PerfilDto** (exibição). Princípio
"fail closed": valor inesperado nunca vira admin por acidente.
- Nota de segurança: não é defesa contra auto-promoção pela aplicação (isso já está
  fechado — sem DTO que aceite Role, sem endpoint, JWT assinado). É defesa em
  profundidade contra um valor inconsistente no banco.

### Tag de admin no perfil (UI)
Admin é superconjunto de aluno (pode tudo que o aluno faz + painel). A UI reflete isso:
- Aluno: `[Aluno(a)]`
- Admin: `[Aluno(a)] [Administrador(a)]` — a de admin em chip "reverso" (fundo creme,
  texto e borda verde 2px) para destacar por contraste de luminosidade, dentro da paleta.
- A distinção não depende só de cor (textos diferentes) — ok para acessibilidade.

## Arquivos alterados
- `Backend/src/UniHub.Domain/Entities/Usuario.cs` — enum + `RoleEfetivo`.
- `Backend/src/UniHub.Application/Services/Seguranca/AuthService.cs` — claim usa `RoleEfetivo`.
- `Backend/src/UniHub.Application/DTOs/Perfil/PerfilDto.cs` — exibição usa `RoleEfetivo`.
- `Frontend/src/pages/Perfil.jsx` — dupla-tag + `ehAdmin`.
- `Frontend/src/pages/Perfil.css` — estilos `.perfil__badges` e `.perfil__badge--admin`.
- `Tests/UniHub.Tests/...` — teste do fail-safe (`RoleEfetivo`).
- `AGENTS.md` §15 — glossário atualizado (`Aluno=0, Admin=2`).

## Critérios de aceite
- [x] Enum `UserRole` só com `Aluno = 0` e `Admin = 2`.
- [x] `RoleEfetivo` retorna `Aluno` para valor desconhecido e `Admin` para `2`.
- [x] Claim e PerfilDto usam `RoleEfetivo`.
- [x] Perfil de aluno mostra 1 tag; perfil de admin mostra 2 (admin destacada).
- [x] `dotnet build` limpo; sem migration pendente; testes Unit passando.

## Verificação feita
- `SELECT "Role", COUNT(*) ... GROUP BY "Role"` em dev e prod → `0: 4`, `2: 2`,
  nenhum `1`. Confirma que manter `Admin = 2` não exige migração de dados.
- `dotnet ef migrations has-pending-model-changes` → sem pendência (remover valor
  de enum não altera o schema `int`).
