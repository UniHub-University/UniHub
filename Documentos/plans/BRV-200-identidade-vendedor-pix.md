# BRV-200 — Identidade do vendedor e chave PIX

## Resumo

Esta branch implementa a exibição da identidade pública do vendedor nos cards de anúncios e uma área de gestão da chave PIX no perfil do vendedor. As listas de Bazar, Alimentação e Busca passam a devolver nome e foto do vendedor. Nos detalhes de anúncio, a identidade e o PIX são apresentados em seções distintas, com opção para copiar a chave.

## O que foi feito

### API e backend

- Os DTOs de resumo de Bazar (`ItemBazarResumoDto`) e Alimentação (`AlimentoResumoDto`) passaram a incluir `NomeVendedor` e `FotoVendedorUrl`.
- O endpoint `GET /api/itens-bazar` carrega o vendedor e o usuário associado e preenche nome e foto no mapeamento do resumo.
- A listagem de alimentos carrega vendedor e usuário no repositório; o serviço preenche nome e foto no resumo.
- A busca geral inclui `FotoVendedorUrl` no DTO e projeta a foto da loja, usando a foto de perfil do usuário como fallback.
- A resolução da foto pública prioriza a foto cadastrada no perfil do vendedor (`FotoUrl`) e recorre à foto de perfil do usuário (`FotoPerfilUrl`) se a primeira não existir.
- Foi acrescentado `GET /api/perfil/vendedor/info`, autenticado, para devolver ao próprio vendedor a chave PIX atual. O endpoint retorna `404` quando o usuário não possui perfil de vendedor.
- O `PUT /api/perfil/vendedor/info` existente permanece como endpoint de atualização; a validação da chave PIX existente é reutilizada.

### Frontend — cards

- Os cards de Bazar, Alimentação e Busca exibem foto e nome do vendedor recebidos pela API.
- Se não houver foto, o card mostra a inicial do nome. Se a API não trouxer um nome, não desenha o avatar `?` e usa o rótulo seguro “Vendedor”.
- O nome longo nos cards de Alimentação pode quebrar em múltiplas linhas dentro dos limites do cartão, sem escapar da caixa.

### Frontend — perfil e detalhes

- O Perfil mostra um formulário de chave PIX apenas para usuários com perfil de vendedor.
- O campo carrega a chave existente, permite cadastrar/editar e permite remover deixando o campo vazio e salvando.
- Erros de leitura impedem salvar uma entrada vazia por engano. Sucesso e falha têm mensagens próprias.
- Nos detalhes de Bazar e Alimentação, o componente compartilhado apresenta identidade do vendedor e PIX em blocos visualmente separados.
- O bloco PIX inclui um botão para copiar a chave. **Nesta implementação, o PIX continua visível no detalhe do anúncio; não foi movido para o fluxo de compra.**

## Rotas e contratos afetados

| Rota | Efeito |
|---|---|
| `GET /api/itens-bazar` | Inclui `nomeVendedor` e `fotoVendedorUrl` nos itens do resumo. |
| `GET /api/alimentos` | Inclui `nomeVendedor` e `fotoVendedorUrl` nos alimentos do resumo. |
| Busca geral de produtos | Inclui `fotoVendedorUrl` junto ao nome que já era retornado. |
| `GET /api/perfil/vendedor/info` | Novo endpoint autenticado para leitura da chave PIX do vendedor logado. |
| `PUT /api/perfil/vendedor/info` | Continua atualizando informações de vendedor e chave PIX, agora usado pelo formulário do Perfil. |

O formato de serialização JSON do ASP.NET Core expõe as propriedades em camelCase no cliente. Não houve mudança no schema do banco nem necessidade de migration.

## Arquivos principais

### Backend

- `Backend/src/UniHub.Application/DTOs/Bazar/ItemBazarDTOs.cs`
- `Backend/src/UniHub.Application/DTOs/Alimentacao/AlimentoDTOs.cs`
- `Backend/src/UniHub.Application/DTOs/Produtos/ProdutoResumoDto.cs`
- `Backend/src/UniHub.Application/DTOs/AtualizarInfoVendedorDto.cs`
- `Backend/src/Unihub.API/Controllers/ItensBazarController.cs`
- `Backend/src/Unihub.API/Controllers/VendedorController.cs`
- `Backend/src/UniHub.Application/Services/Alimentacao/AlimentoService.cs`
- `Backend/src/UniHub.Application/Services/Vendedor/VendedorService.cs`
- `Backend/src/UniHub.Infrastructure/Repositories/Alimentacao/AlimentoRepository.cs`
- `Backend/src/UniHub.Infrastructure/Repositories/Vendas/ProdutoBuscaRepository.cs`

### Frontend

- `Frontend/src/components/CardAnuncio.jsx` e `CardAnuncio.css`
- `Frontend/src/components/CardAlimento.jsx` e `CardAlimento.css`
- `Frontend/src/components/ProdutoCard.jsx` e `ProdutoCard.css`
- `Frontend/src/components/BlocoVendedor.jsx` e `BlocoVendedor.css`
- `Frontend/src/pages/DetalheBazar.jsx` e `DetalheAlimentacao.jsx`
- `Frontend/src/pages/Perfil.jsx` e `Perfil.css`

O plano de implementação e critérios de aceite ficam em `Documentos/plans/BRV-200-identidade-vendedor-nos-anuncios.md`.

## Validação

Validações executadas durante a implementação:

- `dotnet test Tests/UniHub.Tests/UniHub.Tests.csproj --filter "Categoria=Unit"` — 142 testes passaram, nenhum falhou ou foi ignorado.
- `npm run lint` — sucesso.
- `npm run build` — sucesso.
- `git diff --check` — sucesso.

## Teste manual recomendado

1. Na aba Network, confirmar que `GET /api/itens-bazar` e `GET /api/alimentos` retornam `nomeVendedor` e `fotoVendedorUrl`.
2. Conferir os cards na Home, Bazar, Alimentação e Busca; verificar quebra de nomes longos em Alimentação.
3. Entrar como vendedor em Meu Perfil, cadastrar ou alterar a chave PIX, recarregar e confirmar que o valor persiste; testar a remoção deixando o campo vazio.
4. Abrir um detalhe de Bazar/Alimentação, confirmar o bloco separado de PIX e testar “Copiar chave”.

## Fora do escopo

- Movimentar a exibição do PIX para a etapa/ação de compra. O PIX permanece nos detalhes do anúncio nesta entrega.

