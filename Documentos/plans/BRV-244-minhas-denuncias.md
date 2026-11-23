# Plano — Minhas Denúncias (acompanhamento do denunciante)

> Stretch da Trilha 1 (Ciclo de Moderação), issue #116. Frontend + pré-requisito
> de backend (endpoint novo). Só-leitura: o denunciante acompanha o desfecho das
> próprias denúncias. Entrega em 2 PRs (backend, depois frontend).

## Objetivo

Permitir que um usuário autenticado veja a lista das denúncias que **ele mesmo
fez**, com o status de cada uma (Aberta / Em análise / Resolvida / Rejeitada),
o motivo, a data e o produto (quando a denúncia foi sobre um anúncio). Apenas
leitura — sem editar nem cancelar.

## Escopo

- **Inclui:**
  - **Backend:** `GET /api/denuncias/minhas` — retorna só as denúncias do usuário
    logado (denunciante via claim `sub`, sem IDOR). DTO de resposta próprio.
  - **Front:** `services/denuncias.js` ganha `listarMinhasDenuncias()`.
  - **Front:** página `/minhas-denuncias` (molde de `MinhasSolicitacoes.jsx`):
    lista as próprias denúncias com status, motivo, descrição, data e produto.
  - **Front:** rota protegida por login + card "Denúncias feitas por mim" no Perfil.

- **NÃO inclui:**
  - Editar/cancelar/reenviar denúncia (só leitura).
  - Notificar o denunciado (confidencial — fora de escopo pela issue).
  - Painel do admin (B2/B3, já entregue).

## Decisões de design

- **Endpoint:** `GET /api/denuncias/minhas`. O `denuncianteId` vem do token
  (claim `sub`), NUNCA de parâmetro — garante "só as próprias denúncias" (sem "Insecure Direct Object Reference").
- **DTO de resposta (`MinhaDenunciaDto`):** devolve só dados não-sensíveis e já
  resolvidos para exibição: `Id`, `DenunciadoNome` (NOME, nunca ID — nem Usuario
  nem Vendedor trafegam), `ProdutoNome` (null se a denúncia é só de perfil),
  `Motivo`, `Descricao`, `Status`, `CriadaEm`. Molde: `AdminDenunciaDto`.
- **LGPD:** o nome do denunciado pode aparecer porque o denunciante já escolheu o
  alvo (sabe quem denunciou); nenhum ID sensível vai para o front. O denunciado
  não é notificado (regra de privacidade preservada).
- **Rótulos:** reaproveita `rotulosDenuncia.js` (`rotuloStatusDenuncia`,
  `rotuloMotivoDenuncia`) — sem duplicar.
- **UI:** página só-leitura (molde `MinhasSolicitacoes.jsx`); card discreto no
  Perfil, na coluna de ações (reusa `perfil__card`), levando a `/minhas-denuncias`.
- **Sem migration:** só consulta/DTO/endpoint novos; não mexe em entidade/schema.

## Pré-requisito de backend

1. `MinhaDenunciaDto` (novo DTO de resposta).
2. `IDenunciaRepository` + `DenunciaRepository`: método
   `ListarPorDenuncianteAsync(denuncianteId)` (espelho do `ListarParaAdminAsync`,
   filtrando por `DenuncianteId`, `AsNoTracking`, ordenado por `CriadaEm` desc,
   com `Include` de Denunciado e Produto para resolver os nomes).
3. `DenunciasController`: `GET minhas` — claim `sub` → lista do próprio usuário → mapeia para `MinhaDenunciaDto`.


## Passos de implementação

1. **[Backend]** DTO + método no repositório + endpoint GET (PR próprio).
2. **[Front/service]** `listarMinhasDenuncias()` em `services/denuncias.js`.
3. **[Front/página]** `pages/.../MinhasDenuncias.jsx` (+ css) no molde de
   `MinhasSolicitacoes.jsx`: lista com status, motivo, data, produto; estados
   de carregando/vazio/erro.
4. **[Front/rota]** rota protegida `/minhas-denuncias` no `App.jsx`.
5. **[Front/perfil]** card "Denúncias feitas por mim" na coluna de ações do Perfil.
6. **[Validação]** backend: `dotnet build` + testes; front: `npm run lint` +
   `npm run build`; teste manual.

## Critérios de aceite

- [x] `GET /api/denuncias/minhas` retorna só as denúncias do usuário logado.
- [ ] Usuário sem denúncias vê estado vazio amigável.
- [ ] Cada item mostra status, motivo, descrição, data e produto (quando houver).
- [x] Rota protegida: sem login, redireciona; ninguém vê denúncia de outro (sem IDOR).
- [x] Nenhum ID sensível (UsuarioId) trafega para o front.
- [ ] Card "Denúncias feitas por mim" no Perfil leva à página.
- [ ] `dotnet build` / `npm run lint` / `npm run build` limpos.

## Pendências / dúvidas em aberto

Nenhuma — escopo e contrato definidos. B2 já entregou (backend liberado),
decisão de LGPD fechada (DTO devolve nome, nunca ID).


## Decisões confirmadas

- **Nome do card:** "Denúncias feitas por mim".
- **DTO devolve nome do denunciado, nunca ID** (LGPD).
- **Entrega em 2 PRs:** backend primeiro, frontend depois.
- **Contrato do endpoint:** `GET /api/denuncias/minhas` devolve **lista direta** (não paginada), diferente do painel admin (#119, que usa PaginatedResultDto). Decisão consciente (um usuário tem poucas denúncias). O service de front lê o array direto, sem `.items`. (Confirmado na review do PR128.)
