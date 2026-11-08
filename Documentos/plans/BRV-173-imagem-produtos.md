# Plano — BRV-173: Imagem nos produtos (1 foto por anúncio)

## Objetivo

Permitir que cada produto (Bazar e Alimentação) tenha **1 imagem**, enviada pelo
vendedor no formulário e exibida nos cards e na tela de detalhe. Aumenta a confiança
do comprador e o impacto visual da plataforma, reaproveitando o upload (Cloudinary)
que já existe no projeto.

## Escopo

- **Inclui:**
  - Campo `ImagemUrl` (opcional) na entidade `Produto`, herdado por `ItemBazar` e `Alimento`.
  - Migration da coluna `ImagemUrl` em `Produtos` (com o Guardião de Migrations).
  - `ImagemUrl` nos DTOs de criar/editar/resposta dos dois módulos.
  - Upload de 1 imagem nos formulários de Bazar e de Alimento (via endpoint já existente).
  - Possibilidade de **remover** a imagem na edição (o produto volta a ficar sem foto).
  - Exibição da imagem nos cards (Bazar e Alimentação) e nas telas de detalhe.

- **NÃO inclui:**
  - Galeria / múltiplas fotos por produto (apenas 1 por produto; galeria é melhoria futura).
  - Novo endpoint de upload (o `POST /api/upload/imagem?pasta=produtos` já existe e é reusado).
  - Edição/recorte de imagem no cliente.
  - Remoção automática da imagem antiga do Cloudinary ao trocar/remover a foto (fica para o final da BRV — ver passo 4 e pendências).

## Decisões de design

- **Entidades/Domain:** adicionar `public string? ImagemUrl { get; set; }` na classe
  abstrata `Produto`. Por ser TPH (tabela única `Produtos` com discriminador), o campo
  é herdado por `ItemBazar` e `Alimento` sem duplicação. Campo **opcional** (nullable):
  produto pode não ter imagem.

- **Endpoints/API:** nenhuma rota nova. O upload usa o `UploadController` existente
  (`POST /api/upload/imagem?pasta=produtos`, pasta já na lista de permitidas), que
  retorna a URL pública do Cloudinary. A URL é persistida junto do produto pelos
  endpoints de criar/editar que já existem (Bazar e Alimento).

- **Fluxo de salvar (molde `Perfil.jsx`):** a imagem é selecionada no formulário, mas o
  upload só acontece **ao clicar em Salvar**: o front faz o upload, recebe a URL e envia
  essa URL junto com os demais dados do produto. Evita URLs órfãs quando o usuário desiste
  no meio e mantém consistência com o resto do projeto.

- **Remover imagem na edição:** se o vendedor remover a foto e salvar, o produto volta a
  ficar **sem imagem** (`ImagemUrl = null`) e a antiga é apagada do Cloudinary. Mesmo
  padrão do `VendedorService` (enviar URL vazia/nula sinaliza "remover a foto atual").

- **Troca de imagem (imagem antiga):** ao substituir a foto na edição, a antiga é
  **removida** do Cloudinary (`_imageStorage.DeleteAsync(ExtrairPublicId(urlAntiga))`),
  seguindo o padrão já usado em `PerfilController` e `VendedorService.RemoverImagemAntigaAsync`.

- **Placeholder (produto sem foto):** o card e o detalhe **não exibem área de imagem**
  quando o produto não tem foto — fica igual ao layout atual, sem imagem genérica.

- **Limite de tamanho da imagem:** começa com o padrão do Perfil (máx. 3 MB + tipos
permitidos). Revisar depois se produtos precisarem de imagens maiores — é um ajuste
simples (trocar a constante no front), decidido durante/após a implementação.

- **Banco/Migrations:** muda o schema → **gera migration** (`AddColumn ImagemUrl` em
  `Produtos`). ⚠ **Coordenar com o Guardião de Migrations** (seção 6 do AGENTS.md);
  não gerar sozinha. Revisar o arquivo (só `AddColumn`) e testar em branch do Neon
  antes da main.

- **Impacto em concorrência/LGPD:** nenhum. `ImagemUrl` não toca `QuantidadeDisponivel`
  (fora do fluxo de estoque atômico) nem expõe dado sensível — é uma URL pública de produto.

- **Trade-offs considerados:**
  - *Campo em `Produto` (base) vs. campos separados em cada subtipo* → escolhido na base,
    porque o comportamento é idêntico nos dois módulos e evita duplicar coluna/código.
  - *Upload embutido no Salvar vs. upload imediato ao selecionar* → embutido no Salvar
    (molde do Perfil), para não deixar imagens órfãs se o usuário desistir.
  - *1 foto vs. galeria* → 1 foto, conforme corte do planejamento da Sprint 5.

## Passos de implementação

1. **[Domain]** Adicionar `string? ImagemUrl` em `Produto` (herda para `ItemBazar` e `Alimento`).
2. **[Migration]** ⚠ Com o Guardião: gerar/aplicar a migration da coluna `ImagemUrl` em `Produtos`.
3. **[Application/DTOs]** Incluir `ImagemUrl` nos DTOs de criar/editar/resposta do Bazar
   (`CriarItemBazarDto`, `AtualizarItemBazarDto`, resposta) e da Alimentação
   (`CriarAlimentoDto`, `AtualizarAlimentoDto`, `AlimentoResumoDto`, `AlimentoRespostaDto`).
4. **[Application/Service]** Persistir e mapear `ImagemUrl` no `ItemBazarService` e no
   `AlimentoService` (criar e editar). Na edição: ao **trocar** a imagem, remover a antiga
   do Cloudinary; ao **remover** (URL nula), apagar a antiga e deixar `ImagemUrl = null`.
   Reusar `_imageStorage.DeleteAsync` / `ExtrairPublicId` (padrão do `VendedorService`).
4. **[Application/Service]** Persistir e mapear `ImagemUrl` no `AlimentoService` e no
   `ItensBazarController` (o Bazar não tem service próprio — a lógica fica no controller),
   tanto ao criar quanto ao editar. URL vazia é normalizada para `null`.
   > A **remoção da imagem antiga** do Cloudinary (ao trocar/remover) foi adiada: exige
   > injetar `IImageStorageService` no controller do Bazar e mexer em DI. Fica para o
   > final da BRV (padrão a reusar: `VendedorService.RemoverImagemAntigaAsync`).
5. **[Front/Bazar]** `FormularioItemBazar`: seletor de imagem + preview + opção de remover
   → no Salvar, faz upload (`pasta=produtos`) e envia a URL. Validação no front (máx. 3 MB
   + tipos). Molde: `Perfil.jsx`.
6. **[Front/Alimento]** `FormularioAlimento`: mesmo fluxo de upload/remoção.
7. **[Front/Cards]** Exibir imagem no `CardAnuncio` (Bazar) e no card da página de
   Alimentação; **sem imagem = sem área de imagem** (layout atual).
8. **[Front/Detalhe]** Exibir imagem em `DetalheBazar` e `DetalheAlimentacao`, com o mesmo
   comportamento de "sem foto".
9. **[Validação]** `dotnet build` + testes (back); `npm run lint` e `npm run build` (front);
   teste manual ponta a ponta nos dois módulos (criar com foto, editar trocando a foto,
   editar removendo a foto, ver card e detalhe com e sem imagem).

> Sugestão de PRs: um PR de **backend** (passos 1-4 + migration) e um PR de **frontend**
> (passos 5-8), mantendo PRs pequenos em áreas sensíveis (AGENTS seção 9).

## Critérios de aceite

- [ ] `Produto` tem `ImagemUrl` (string, opcional), herdado por `ItemBazar` e `Alimento`.
- [ ] Migration da coluna `ImagemUrl` criada com o Guardião e testada em branch do Neon.
- [ ] DTOs de criar/editar/resposta (Bazar e Alimento) aceitam e expõem `ImagemUrl`.
- [ ] Formulário do Bazar envia 1 imagem (pasta `produtos`) no Salvar e guarda a URL.
- [ ] Formulário de Alimento faz o mesmo.
- [ ] Na edição, trocar a imagem remove a antiga do Cloudinary.
- [ ] Na edição, remover a imagem deixa o produto sem foto (`ImagemUrl = null`) e apaga a antiga.
- [ ] Cards do Bazar e da Alimentação exibem a imagem quando houver; sem imagem, sem área de foto.
- [ ] `DetalheBazar` e `DetalheAlimentacao` exibem a imagem quando houver.
- [ ] Validação no front (máx. 3 MB + tipos) antes do upload.
- [ ] `dotnet build` limpo + testes passando.
- [ ] `npm run lint` e `npm run build` limpos.
- [ ] (mexe em schema) migration testada em branch do Neon.

## Pendências / dúvidas em aberto

- **Guardião de Migrations da Sprint 5:** provavelmente o **Henrique** (autor do
  planejamento) — **confirmar com ele** antes do Dia 1. A Task 2 depende dessa pessoa.
- **Colisão de arquivos no front:** as Frentes 3 e 4 também tocam formulários, cards e
  detalhe, mas em partes diferentes do arquivo (features distintas), então o Git tende a
  fazer o merge automático. Atenção leve: alinhar quem mergeia primeiro para reduzir retrabalho.
- **Remoção de imagem órfã no Cloudinary:** adiada nesta leva. Ao trocar ou remover a foto de um produto, a antiga continua no Cloudinary por enquanto. Implementar no final da BRV, idealmente junto com o Perfil (que hoje remove) para padronizar.