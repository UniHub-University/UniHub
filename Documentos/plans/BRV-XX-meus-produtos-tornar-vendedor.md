# Plano — BRV-XX: Meus produtos (vendedor) + botão "Tornar-se vendedor"

## Objetivo
Permitir que o vendedor liste os próprios produtos pela API e que qualquer usuário
crie seu perfil de vendedor a partir da página de Perfil.

## Escopo
- **Inclui:**
  - Backend: `GET /api/perfil/vendedor/produtos`, que lista os produtos do vendedor
    autenticado em qualquer status (Disponível, Esgotado, Inativo).
  - Frontend: cartão "Vendedor" na coluna de ações do `Perfil.jsx`, com o botão
    "Ser vendedor" (chama o `POST /api/perfil/vendedor`, que já existe).
- **NÃO inclui:**
  - Paginação (lista de um único vendedor; segue o padrão das listagens do projeto).
  - A tela "Meus Produtos" no `PainelVendedor.jsx`. O próprio arquivo diz que essa
    listagem será integrada pela equipe responsável.
  - Mudança de schema / migration.

## Decisões de design
- **Entidades/Domain:** nenhuma alteração.
- **Endpoint:** `[HttpGet("produtos")]` no `VendedorController` (rota base
  `api/perfil/vendedor`). O vendedor é derivado do token (claim `"sub"` →
  `Vendedor.UsuarioId`), nunca de um id na requisição (evita IDOR).
  - Sucesso: `200` com a lista de `MeuProdutoDto`, mais recentes primeiro.
  - Usuário sem perfil de vendedor: `404 { mensagem }`.
- **Acesso a dados:** método `ListarMeusProdutosAsync` no `VendedorService`, que já
  usa `AppDbContext` direto (mantém o estilo do módulo). Consulta `AsNoTracking`.
- **DTO:** `MeuProdutoDto` (Id, Nome, Descricao, Preco, QuantidadeDisponivel, Tipo,
  Status, DataPublicacao) e `MeusProdutosResultDto` (padrão Sucesso/Mensagem do
  módulo). O `ProdutoResumoDto` da busca não serve: não tem `Status` e traz dados do
  vendedor que aqui são redundantes.
- **Tipo:** `p is Alimento ? "lanche" : "bazar"`, possível agora que `Alimento` está
  mapeado na TPH.
- **DI:** nenhuma mudança (`VendedorService` já está registrado).
- **Front:** segue o padrão do "Ser voluntário" no próprio `Perfil.jsx` (chamada via
  `api`, mensagens de sucesso/erro no cartão de dados). Após o POST, atualiza o
  `perfil` local e chama `recarregar()` do `usePerfilVendedor`, para o menu de
  vendedor e as rotas `/vendedor` liberarem sem recarregar a página.
- **Concorrência/LGPD:** nenhum impacto. Leitura apenas dos dados do próprio usuário.

## Passos de implementação
1. `DTOs/Vendedor/MeusProdutosDtos.cs` (novo).
2. `VendedorService.ListarMeusProdutosAsync` (alterado).
3. `VendedorController` → `GET produtos` (alterado).
4. `Frontend/src/pages/Perfil.jsx` → cartão "Vendedor" + `tornarSeVendedor` (alterado).

## Critérios de aceite
- [ ] `GET /api/perfil/vendedor/produtos` retorna só produtos do vendedor do token,
      em todos os status, com `Status` e `Tipo`.
- [ ] Usuário sem perfil de vendedor recebe 404 com mensagem.
- [ ] Sem token: 401.
- [ ] No Perfil, quem não é vendedor vê "Ser vendedor"; ao clicar, vira vendedor,
      vê a mensagem de sucesso, "Vendedor: Sim" e o atalho para o Painel.
- [ ] O menu de vendedor aparece sem recarregar a página.
- [x] `dotnet build` limpo (0 erros, 0 avisos) e testes unitários passando (60/60).
- [x] `npm run build` limpo.
- [ ] `npm run lint`: 1 erro em `Perfil.jsx` (efeito do voluntário, `react-hooks/set-state-in-effect`)
      que já existe na `main` e está fora do escopo desta task. Não corrigido aqui por
      decisão (AGENTS.md seção 0.4); avisar o autor do perfil de voluntário.

## Pendências
- Número real da task no YouTrack (a branch usa `BRV-XX`).
