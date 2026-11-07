## O que este PR faz
<resumo em 1-2 linhas>

## Issue relacionada
Closes #

## Mudancas principais
-

## Como testei
- [ ] `dotnet test --filter "Categoria=Unit"` passou
- [ ] Testes de integracao (se aplicavel) rodados localmente com `UNIHUB_TEST_DB`
- [ ] Testado manualmente no front (`npm run dev`)

## Checklist (ver AGENTS.md, secao 10)
- [ ] `dotnet build` limpo
- [ ] Sem migration pendente (ou migration incluida e coordenada com o Guardiao)
- [ ] Claim de usuario via `User.FindFirst("sub")`
- [ ] DI registrado no `Program.cs` para todo service/repository novo
- [ ] Autorizacao por posse via token (sem IDOR)
- [ ] LGPD: sem vazamento de identidade na Acao Solidaria
- [ ] Arquivos novos terminam com newline final
