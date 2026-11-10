\# Plano — Vendedores públicos: lista, perfil, anúncios e bloco no anúncio (Frente 3, Sprint 5)



\## Objetivo

Permitir que qualquer usuário autenticado \*\*veja os vendedores\*\*, abra o \*\*perfil público de um vendedor\*\* com os \*\*anúncios dele\*\*, e chegue a esse perfil a partir do \*\*anúncio\*\*, para decidir se confia no vendedor. Hoje o `VendedorController` só tem rotas do próprio vendedor.



\## Escopo

\- \*\*Inclui (backend):\*\* `GET /api/vendedores` (lista paginada), `GET /api/vendedores/{id}` (perfil público) e `GET /api/vendedores/{id}/produtos` (anúncios disponíveis do vendedor, paginados); DTOs públicos; testes de unidade.

\- \*\*Inclui (front):\*\* rota `/vendedores` (lista de vendedores), rota `/vendedores/:id` (cabeçalho do vendedor + grade de anúncios, reusando `ProdutoCard`, com paginação) e, no detalhe do anúncio (Bazar e Alimentação), um bloco "Vendedor" com o nome (link para o perfil) e a nota; estados de carregando, erro e não encontrado.

\- \*\*NÃO inclui:\*\* foto e chave PIX no detalhe do anúncio (Frente 4); avaliar vendedor; editar dados do vendedor; busca/filtro de vendedores; mudança de schema.



\## Decisões de design

\- \*\*Dados públicos:\*\* nome (do usuário), foto, descrição do negócio, nota média, quantidade de avaliações, locais (campus e ponto de encontro) e horários de venda. \*\*Nunca\*\* expõe telefone, e-mail, `UsuarioId`, `GoogleId` nem a chave PIX (LGPD; o PIX é exposto apenas no anúncio, decisão da Frente 4).

\- \*\*Autenticação:\*\* todas as rotas exigem token (como o restante da API); qualquer usuário autenticado pode ver.

\- \*\*Lista de vendedores:\*\* só vendedores com ao menos um anúncio disponível, do mais bem avaliado para o menos, com desempate estável por `Id`; paginação com padrão 12 e máximo 50, validada no Service.

\- \*\*Anúncios do vendedor:\*\* reaproveita a busca de produtos (`ProdutoService.BuscarAsync`) com um filtro opcional por vendedor, para manter a mesma regra de "o que é público" (só disponíveis e com estoque) e o mesmo contrato paginado.

\- \*\*Respostas:\*\* 200 com os dados; 404 se o vendedor não existe (perfil e anúncios); 400 para parâmetros inválidos de filtro.

\- \*\*Banco/Migrations:\*\* nenhuma (só leitura).

\- \*\*Front:\*\* páginas novas `ListaVendedores.jsx` e `PaginaVendedor.jsx` + service `vendedoresPublico.js`; reutiliza `ProdutoCard` e o padrão de paginação das buscas.

\- \*\*Convivência com a Frente 4:\*\* o bloco "Vendedor" do detalhe do anúncio traz só nome (link) e nota; a foto e o PIX são da Frente 4. Os dois trechos devem ser combinados para não gerar conflito.

\- \*\*PR único:\*\* backend e front na mesma branch, em commits separados por camada, para facilitar a revisão.



\## Passos de implementação

1\. Plano (este arquivo).

2\. Backend: DTOs públicos + consulta do vendedor + `GET /api/vendedores/{id}` + testes.

3\. Backend: `GET /api/vendedores` (lista paginada) + testes.

4\. Backend: filtro por vendedor na busca + `GET /api/vendedores/{id}/produtos` + testes.

5\. Front: service e página `/vendedores/:id`.

6\. Front: página `/vendedores` (lista) e entrada no menu/rotas.

7\. Front: bloco "Vendedor" no detalhe do anúncio (Bazar e Alimentação) e link nos cards.



\## Critérios de aceite

\- \[ ] `GET /api/vendedores` lista vendedores paginados, sem expor PIX, telefone nem e-mail

\- \[ ] `GET /api/vendedores/{id}` devolve o perfil público sem PIX, telefone nem e-mail

\- \[ ] `GET /api/vendedores/{id}/produtos` devolve só anúncios disponíveis do vendedor, paginados

\- \[ ] Vendedor inexistente devolve 404 no perfil e nos anúncios

\- \[ ] Todas as rotas exigem token

\- \[ ] `/vendedores` lista os vendedores e leva ao perfil de cada um

\- \[ ] `/vendedores/:id` mostra o vendedor e seus anúncios, com paginação

\- \[ ] O detalhe do anúncio mostra o nome do vendedor com link para o perfil

\- \[ ] `dotnet build`, `dotnet test` (unitários) e `npm run build` limpos

\- \[ ] Testado no site



\## Pendências / dúvidas em aberto

\- Combinar com quem pegou a Frente 4 (nome + PIX + foto no anúncio) quem edita cada trecho do detalhe do anúncio, para os PRs não conflitarem.

