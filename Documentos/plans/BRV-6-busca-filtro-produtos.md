# Plano — BRV-6: Busca e filtro de produtos

## Objetivo
Permitir que a comunidade acadêmica pesquise por palavra-chave e filtre os produtos
disponíveis (por categoria, faixa de preço, campus e horário de disponibilidade),
com paginação, para encontrar ofertas rapidamente sem navegar por todos os anúncios.
Entrega um endpoint de busca no backend e uma tela de listagem com filtros no frontend.

## Status da entrega
- **Backend:** implementado e com `dotnet build` limpo (SDK .NET 10). Pronto para
  teste via Swagger e PR.
- **Frontend:** ADIADO. A branch `feat/BRV-6-busca-filtro-produtos` foi criada antes do
  merge da fundação de front (BRV-116): `src/{auth,config,services,pages}` estão vazias,
  não há `services/api.js`, `AuthContext`, `RotaProtegida` nem tema. Implementar o front
  aqui exigiria recriar essa fundação à mão (duplicação + risco de conflito). Decisão:
  entregar o backend agora e fazer o front depois, a partir da branch de integração
  `feat/frontend-sprint4` (AGENTS.md seção 14), que já tem a fundação — a confirmar com
  o squad de frontend.

## Escopo
- **Inclui (nesta entrega):**
  - Endpoint `GET /api/produtos` com filtros (termo, tipo, faixa de preço, campus,
    dia da semana, horário) e paginação.
  - Camada Application: DTOs de query/resultado, validação e normalização dos filtros
    no `ProdutoService`.
  - Camada Infrastructure: repositório de busca dedicado (`ProdutoBuscaRepository`).
- **Front (entrega seguinte, fora desta branch):** serviço de API, card de produto e
  tela de busca com filtros + paginação, rota protegida `/produtos`.
- **NÃO inclui:**
  - Mudança de schema / migration. A entidade `Alimento` **não** está mapeada no EF
    (não há `DbSet<Alimento>` nem presença no snapshot). O mapeamento do `Alimento`
    e a migration correspondente ficam para uma task/PR separado, coordenado com o
    **Guardião de Migrations** (AGENTS.md seção 6). Ver "Decisão: lanches" abaixo.
  - Ordenação configurável pelo usuário (fica fixa em mais recentes primeiro).
  - Detalhe/compra do produto (fora do escopo desta story).

## Decisões de design

- **Entidades/Domain:** nenhuma entidade nova ou alterada. Localização e horário já
  moram no `Vendedor` (coleções `Locais` → `LocalVenda.Campus/PontoDeEncontro` e
  `Horarios` → `HorarioVenda.DiaSemana/HoraInicio/HoraFim`). O filtro de campus/horário
  é, na prática, "o vendedor atende naquele campus / naquele dia e hora".

- **Endpoints/API:** `GET /api/produtos` (autenticado, como os demais controllers).
  Query string: `termo`, `tipo` (`bazar`|`lanche`), `precoMin`, `precoMax`, `campus`,
  `diaSemana` (0–6), `hora` (`HH:mm`), `page`, `pageSize`. Resposta:
  `PaginatedResultDto<ProdutoResumoDto>`.

- **Fluxo de camadas:** Controller → `ProdutoService.BuscarAsync` (valida/normaliza) →
  `IProdutoBuscaRepository.BuscarAsync` (consulta EF). O service valida enums com
  `Enum.TryParse`/`Enum.IsDefined`, garante `precoMin <= precoMax`, e limita `pageSize`
  (clamp 1–50) — seguindo AGENTS.md seção 4 (validação de enum e autorização/robustez).

- **Decisão: lanches (opção A, sem mexer no schema).** O filtro por tipo usa a coluna
  discriminadora da TPH (`Discriminator`) via `EF.Property<string>`. Como `Alimento`
  ainda não está mapeado, filtrar `tipo=lanche` retorna lista vazia por enquanto —
  sem quebrar nada. Quando o Guardião mapear `Alimento` e rodar a migration, o filtro
  passa a funcionar sem alteração de código.
  - **Risco conhecido para o PR futuro do schema:** `Alimento.Categoria` e
    `ItemBazar.Categoria` têm mesmo nome/tipo; na TPH o EF pode compartilhar a coluna
    `Categoria` (hoje `IsRequired` no ItemBazar) e gerar `AlterColumn` inesperado.
    A migration do `Alimento` deve ser revisada à risca (AGENTS.md seção 6).

- **Banco/Migrations:** esta task **não** gera migration. Nenhuma alteração em
  `AppDbContext`.

- **Regras de negócio da busca:** retorna apenas produtos com `Status == Disponivel`
  e `QuantidadeDisponivel > 0`, de vendedores cujo `Usuario.IsActive == true`
  (não expõe anúncios de contas suspensas pela Moderação).

- **Impacto em concorrência/LGPD:** nenhum. A busca é somente leitura (`AsNoTracking`),
  não decrementa estoque e não toca no módulo Ação Solidária. Expõe só dados públicos
  do anúncio e o nome do vendedor.

- **Trade-offs considerados:**
  - *Repositório dedicado (`IProdutoBuscaRepository`) vs. ampliar `IProdutoRepository`:*
    escolhido o dedicado para não inchar a interface de escrita de estoque (que tem
    semântica de concorrência bem específica) com uma responsabilidade de leitura/consulta.
  - *Filtro de tipo por discriminator vs. por string `Categoria`:* escolhido o
    discriminator, que reflete o tipo real (Bazar x Alimento) de forma estável; a string
    `Categoria` é livre e inconsistente entre as subclasses.
  - *Paginação offset (`Skip/Take`) vs. keyset:* offset, por simplicidade e por ser o
    padrão esperado na story; volume de dados pequeno nesta fase.

## Passos de implementação
1. **Application/DTOs:** `DTOs/Produtos/BuscaProdutosQueryDto.cs` (query + enum
   `TipoProduto` + record `FiltrosBuscaProdutos`) e `DTOs/Produtos/ProdutoResumoDto.cs`.
   (`DTOs/Common/PaginatedResultDto.cs` já existe.)
2. **Application/Interfaces:** `Interfaces/Vendas/IProdutoBuscaRepository.cs`.
3. **Application/Services:** adicionar `BuscarAsync` ao `ProdutoService` (validação +
   normalização dos filtros).
4. **Infrastructure:** `Repositories/Vendas/ProdutoBuscaRepository.cs` (consulta EF com
   ILike, faixa de preço, campus e horário via `Any` nas coleções do vendedor, paginação).
5. **API:** `Controllers/ProdutosController.cs` (`GET /api/produtos`, trata
   `ArgumentException` → 400) e registrar o repositório no `Program.cs` (1 linha de DI).
6. **Frontend:** `services/produtosService.js` (monta query string), `components/ProdutoCard.jsx`
   (+ css), `pages/BuscaProdutos.jsx` (+ css), rota protegida `/produtos` no `App.jsx`.

## Critérios de aceite
- [ ] Busca por palavra-chave casa em `Nome` e `Descricao` (case-insensitive).
- [ ] Filtro por tipo `bazar` retorna itens de bazar; `lanche` retorna vazio por ora
      (limitação documentada — depende da migration do `Alimento`).
- [ ] Faixa de preço (`precoMin`/`precoMax`) filtra corretamente; `precoMin > precoMax`
      retorna 400.
- [ ] Filtro por campus casa com algum `LocalVenda` do vendedor (case-insensitive).
- [ ] Filtro por dia/horário casa com alguma janela de `HorarioVenda` do vendedor.
- [ ] Resultado paginado com `page`, `pageSize`, `totalItems`, `totalPages` corretos;
      `pageSize` limitado a 50.
- [ ] Não retorna produtos inativos, esgotados nem de vendedores suspensos.
- [ ] Enums/valores inválidos (tipo, diaSemana, hora) retornam 400, não 500.
- [x] `dotnet build` limpo (SDK .NET 10; rodado pelo usuário — terminal do Kiro indisponível).
- [ ] (Front — entrega seguinte) Tela de busca lista os produtos, aplica filtros, pagina
      e trata erro/estado vazio; `npm run lint` e `npm run build` limpos.

## Pendências / dúvidas em aberto
- **Guardião de Migrations:** confirmar quem é o Guardião da sprint para agendar o PR
  separado que mapeia `Alimento` e habilita o filtro "lanches".
- **Branch do frontend:** AGENTS.md seção 14 define branch de integração
  `feat/frontend-sprint<N>` (existe `origin/feat/frontend-sprint4`). Definir, antes do
  PR, se a parte de front da BRV-6 mira nessa branch de integração ou se a feature
  full-stack fica toda em `feat/BRV-6-busca-filtro-produtos`.
- **Verificação:** o terminal interno do Kiro está indisponível nesta sessão; `dotnet build`
  e os comandos de lint/build do front precisam ser rodados manualmente pelo usuário.
