# Plano — #104 (H2): Robustez da remoção de imagem no Cloudinary

> Extensão da Sprint 5 · Trilha 2 (Hardening) · issue #104.
> Documentado após a implementação (a parte técnica surgiu de uma conversa de
> design durante a execução; este `.md` registra a decisão para quem assumir depois).

## Objetivo
A remoção da imagem antiga no Cloudinary é um efeito colateral **best-effort**. Uma
falha de infraestrutura do storage (timeout, Cloudinary fora do ar, credencial
expirada) NÃO pode derrubar a operação de negócio (editar/atualizar produto ou
perfil). Antes desta correção, a exceção do storage subia e retornava 500 ao usuário.

## Escopo
- **Inclui:** tratamento de erro best-effort na remoção de imagem antiga, em **defesa
  de profundidade** (borda + chamadores), com teste de regressão.
- **NÃO inclui:** limpeza de imagens órfãs já existentes no Cloudinary; remoção de
  imagem ao **inativar** produto (o registro é mantido para reativação).

## Decisão de design — defesa em profundidade (2 camadas)

**Camada 1 — Borda** (`CloudinaryImageStorageService.DeleteAsync`):
try/catch envolvendo `_cloudinary.DestroyAsync(...)`. Uma exceção de infraestrutura
passa a ser logada (`ILogger`) e vira `return false`. Protege **todos** os chamadores —
inclusive os usos diretos de `DeleteAsync` em `PerfilController` e `UploadController`,
que não passam por um `RemoverImagemAntigaAsync`.

**Camada 2 — Chamadores** (`RemoverImagemAntigaAsync`):
try/catch best-effort nos **3 locais** que removem imagem antiga:
`AlimentoService`, `VendedorService` e `ItensBazarController`. A remoção nunca
derruba a atualização.

**Por que as duas camadas (trade-off):**
A borda sozinha (opção "só no `DeleteAsync`") protege em produção, mas **não é
verificável por unit test** — o mock do `IImageStorageService` substitui a borda real,
então o try/catch do `DeleteAsync` não executa no teste. O try/catch no **chamador** é
o que torna a proteção testável (o teste mocka o storage para lançar e verifica que a
atualização sobrevive). Mantivemos a borda como defesa real + os chamadores como
camada testável. Alinhado ao AGENTS §7.1 ("falha em serviço externo nunca derruba a
operação de negócio").

## Arquivos alterados
- `Backend/src/UniHub.Infrastructure/Service/CloudinaryImageStorageService.cs` — borda.
- `Backend/src/UniHub.Application/Services/Alimentacao/AlimentoService.cs` — chamador.
- `Backend/src/UniHub.Application/Services/Vendedor/VendedorService.cs` — chamador.
- `Backend/src/Unihub.API/Controllers/ItensBazarController.cs` — chamador.
- `Tests/UniHub.Tests/Application/AlimentoServiceRobustezImagemTests.cs` — teste.

## Teste
`AlimentoServiceRobustezImagemTests`: mocka o `IImageStorageService` para **lançar**
na remoção da imagem antiga e verifica que `AtualizarAsync` conclui com sucesso e
persiste a nova URL. Salvaguarda de regressão. Bazar e Vendedor não têm camada de
service unit-testável (débito conhecido do projeto); cobertos pelo mesmo princípio,
com o `AlimentoService` como representante.

## Critérios de aceite
- [x] Falha do Cloudinary na remoção não derruba a atualização (produto/perfil).
- [x] Teste cobre o cenário (`dotnet test --filter "Categoria=Unit"` — 143/143).
- [x] `dotnet build` limpo.

## Referência
Origem: `Documentos/Analise-de-Seguranca.md` + issue #104. Convenção registrada no AGENTS §7.1.
