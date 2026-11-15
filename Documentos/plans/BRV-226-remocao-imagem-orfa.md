# Plano — BRV-226: Remoção de imagem órfã no Cloudinary

> Subtask da BRV-173 (imagem nos produtos). Trabalho extra — sem issue no GitHub.

## Objetivo

Ao trocar ou remover a foto de um produto (Bazar e Alimentação), apagar a imagem
antiga do Cloudinary, evitando arquivos órfãos acumulados no armazenamento. Padroniza
o comportamento que o Perfil já adota.

## Escopo

- **Inclui:**
  - Ao **editar e trocar** a imagem de um produto → remover a URL antiga do Cloudinary.
  - Ao **editar e remover** a imagem (enviar nula) → remover a antiga do Cloudinary.
  - Nos dois módulos: Alimentação (`AlimentoService`) e Bazar (`ItensBazarController`).

- **NÃO inclui:**
  - Remoção da imagem ao **inativar** o produto (DELETE lógico): o registro é mantido
    para histórico/auditoria e pode ser reativado, então a foto é preservada.
  - Backfill/limpeza de imagens órfãs já existentes no Cloudinary (geradas antes desta task).
  - Qualquer mudança no módulo de Perfil (ele já remove; aqui só seguimos o mesmo padrão).
  - Mudança de frontend (o front já envia a URL nova ou nula; a remoção é só no backend).

## Decisões de design

- **Entidades/Domain:** nenhuma mudança. Sem novo campo.
- **Endpoints/API:** nenhuma rota nova. A remoção é interna, disparada no fluxo de atualização que já existe (`PUT /api/alimentos/{id}` e `PUT /api/itens-bazar/{id}`).
- **Banco/Migrations:** **não há migration** — não muda schema.
- **Como remover:** reaproveitar o `IImageStorageService` já existente
  (`ExtrairPublicId(url)` + `DeleteAsync(publicId)`), seguindo o molde de
  `VendedorService.RemoverImagemAntigaAsync`.
- **Regra (igual ao Perfil):** só remove a antiga quando a URL **mudou** em relação à salva. Se a nova URL for igual à atual, não faz nada. Se vier vazia/nula, remove a antiga e salva `null`.
- **Ordem segura:** remover a imagem antiga **somente depois** de confirmar a nova URL a ser salva, para não apagar a antiga caso a atualização falhe antes.
- **Impacto em concorrência/LGPD:** nenhum. Não toca `QuantidadeDisponivel` nem dados sensíveis.
- **Trade-offs:** remover na inativação foi descartado (produto pode ser reativado e ficaria sem foto); mantém a imagem até troca/remoção explícita.

## Passos de implementação

1. **[Alimentação]** Injetar `IImageStorageService` no `AlimentoService`. No
   `AtualizarAsync`, antes de salvar a nova `ImagemUrl`: se a URL mudou, remover a
   antiga do Cloudinary (método auxiliar no padrão do `RemoverImagemAntigaAsync`).
2. **[Bazar]** Injetar `IImageStorageService` no `ItensBazarController`. No `Atualizar`
   (PUT), aplicar a mesma lógica de remoção da imagem antiga quando a URL mudar.
3. **[DI]** Conferir que `IImageStorageService` está registrado no `Program.cs` (já é usado pelo upload/perfil, então deve estar — apenas confirmar).
4. **[Testes]** Testes unitários: ao editar trocando a imagem, `DeleteAsync` é chamado com o publicId da antiga; ao manter a mesma URL, `DeleteAsync` NÃO é chamado; ao
   remover (nula), remove a antiga e salva null. Usar mock do `IImageStorageService`.
5. **[Validação]** `dotnet build` + `dotnet test --filter "Categoria=Unit"` verdes;
   teste manual: cadastrar com foto → editar trocando a foto → conferir que a antiga sumiu do Cloudinary; editar removendo → conferir remoção.

## Critérios de aceite

- [ ] Ao trocar a imagem de um alimento, a antiga é removida do Cloudinary.
- [ ] Ao trocar a imagem de um item do bazar, a antiga é removida do Cloudinary.
- [ ] Ao remover a imagem (URL nula), a antiga é removida e o produto fica sem foto.
- [ ] Manter a mesma imagem (URL inalterada) NÃO dispara remoção.
- [ ] Inativar um produto NÃO remove a imagem (preserva para possível reativação).
- [ ] `IImageStorageService` registrado no DI (confirmado).
- [ ] `dotnet build` limpo + testes unitários passando.

## Decisões confirmadas

- **Inativação NÃO remove imagem:** confirmado — ao inativar o produto, a imagem é
  preservada (o produto pode ser reativado e ficaria sem foto). Remoção só ao trocar/remover.
- **Imagens órfãs já existentes:** fora do escopo — não haverá limpeza das imagens
  geradas antes desta task.
