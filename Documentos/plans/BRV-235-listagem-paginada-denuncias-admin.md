# Plano — BRV-235: Listagem paginada de denúncias para Admin

## Objetivo

Implementar a listagem paginada de denúncias para uso do painel administrativo. O endpoint deve ser acessível somente a usuários com a role Admin e retornar os dados necessários para exibição, incluindo os nomes resolvidos do denunciante, do denunciado e, quando houver, do produto relacionado.

Esta tarefa é a parte da BRV-235 referente ao `GET`; o `PATCH` de status e a prevenção de denúncias duplicadas pertencem à outra parte da implementação da issue GitHub #114.

## Escopo

- **Inclui:** `GET /api/admin/denuncias`; paginação por `page` e `pageSize`; DTO administrativo; consulta ordenada e paginada no repositório; nomes resolvidos; restrição por role Admin.
- **NÃO inclui:** `PATCH` de status, regra de transição, prevenção de duplicidade, interface React, mudanças de schema ou migration.

## Decisões de design

- **DTO:** criar `AdminDenunciaDto` com `Id`, `DenuncianteNome`, `DenunciadoNome`, `ProdutoNome`, `Motivo`, `Descricao`, `Status` e `CriadaEm`. `ProdutoNome` é nulo quando a denúncia não está relacionada a um produto.
- **Endpoint/API:** `GET /api/admin/denuncias?page=1&pageSize=20`, protegido com `[Authorize(Roles = "Admin")]`.
- **Paginação:** reutilizar `PaginatedResultDto<T>`. Validar `page >= 1` e `1 <= pageSize <= 100`. Ordenar por `CriadaEm` decrescente e usar `Id` como desempate.
- **Repositório:** `ListarParaAdminAsync(page, pageSize)` retorna a página de denúncias e o total geral. A consulta é somente leitura e carrega denunciante, denunciado e produto.
- **Banco/Migrations:** não muda o schema; nenhuma migration necessária. O `IDenunciaRepository` já está registrado no DI.
- **Impacto em concorrência/LGPD:** não afeta estoque ou concorrência. A identidade e os detalhes das denúncias são retornados exclusivamente ao Admin, conforme a autorização da rota.
- **Integração:** coordenar alterações em `IDenunciaRepository.cs` e `DenunciaRepository.cs` com a pessoa responsável pelo `PATCH` e pela duplicidade, preservando os métodos de ambas as partes.

## Passos de implementação

1. Criar `AdminDenunciaDto` em `Backend/src/UniHub.Application/DTOs/Denuncias/`.
2. Acrescentar `ListarParaAdminAsync(page, pageSize)` à interface `IDenunciaRepository`.
3. Implementar a consulta paginada em `DenunciaRepository`, incluindo as navegações necessárias e o total antes da paginação.
4. Expor o endpoint GET protegido por Admin. Usar o controller escolhido pela equipe e evitar conflito com a implementação do PATCH.
5. Mapear as entidades para `AdminDenunciaDto` e montar `PaginatedResultDto<AdminDenunciaDto>`.
6. Validar o build, os testes unitários e o endpoint manualmente com token Admin e com token sem a role Admin.

## Critérios de aceite

- [ ] `GET /api/admin/denuncias` exige a role Admin.
- [ ] A resposta contém `items`, `page`, `pageSize`, `totalItems` e `totalPages`.
- [ ] Cada item inclui status, motivo, descrição, data, nome do denunciante e nome do denunciado.
- [ ] `ProdutoNome` é nulo quando a denúncia não tem produto associado.
- [ ] A listagem é ordenada de forma estável, da mais recente para a mais antiga.
- [ ] `page` e `pageSize` inválidos retornam `400 Bad Request`.
- [ ] Usuário autenticado sem role Admin recebe `403 Forbidden`.
- [ ] `dotnet build Backend.csproj -c Release` passa.
- [ ] Testes unitários passam.
- [ ] Nenhuma migration foi criada.

## Pendências / dúvidas em aberto
- Confirmar com a pessoa que fará o PATCH como serão consolidadas as alterações compartilhadas em `IDenunciaRepository.cs` e `DenunciaRepository.cs`.