# Plano — BRV-156 — Formulários do Bazar e ajustes de integração

## Objetivo

Disponibilizar os formulários de gerenciamento do Bazar, uma estrutura de acesso para vendedores, navegação responsiva e pesquisa unificada. Melhorar a experiência da plataforma mantendo a autorização no servidor e a separação entre funcionalidades implementadas e integrações futuras.

## Escopo

- **Issue de referência:** BRV-156 — Frontend Bazar (formulários).
- **Organização do registro:** os formulários do Bazar constituem a frente principal. Painel, navegação, banner e pesquisa são ajustes complementares de integração documentados nesta sessão; este registro não altera automaticamente o escopo cadastrado na issue.

- **Inclui:**
  - Cadastro, edição e exclusão de anúncios do Bazar.
  - Status e condição editáveis, sem exigir sua alteração para salvar outros campos.
  - Confirmação de exclusão e tratamento destacado de acesso restrito.
  - Consulta de edição com verificação de propriedade no backend.
  - Painel do Vendedor com atalhos para Adicionar produto e Meu perfil.
  - Botão Adicionar produto no Bazar exclusivo para vendedores.
  - Perfil por ícone, botão de painel com ícone de loja e lupa global à direita.
  - Navegação responsiva, banner na Home e integração da pesquisa existente.
  - Documentação no padrão do projeto e entregas de código em Markdown.
- **NÃO inclui:**
  - Pedidos recebidos, produtos de Alimentação ou configurações da loja.
  - Mudanças de schema, migrations, Program.cs, credenciais ou regras de concorrência de estoque.

## Decisões de design

- **Entidades/Domain:** preservar as entidades e regras existentes, sem alterações nesta camada.
- **Endpoints/API:**
  - Utilizar `GET /api/itens-bazar/{id}/edicao`, implementado por `ObterParaEdicao`, para carregar o editor somente após verificar a propriedade.
  - Extrair a identidade do claim `sub` do JWT. Identidade inválida retorna 401; produto inexistente retorna 404; produto de outro proprietário retorna 403.
  - Preservar a consulta geral do anúncio e as verificações de propriedade já existentes em PUT e DELETE.
  - Reutilizar `GET /api/perfil` para verificar o perfil vendedor e `GET /api/produtos` para a pesquisa.
- **Banco/Migrations:** não modificar schema nem executar migrations para esta entrega.
- **Impacto em concorrência/LGPD:**
  - Manter as regras existentes de estoque e pedidos.
  - Derivar propriedade da identidade autenticada, não de um ID de proprietário fornecido pelo cliente.
  - Ocultar controles no frontend não substitui autorização no backend.
  - Não incluir tokens, credenciais ou cabeçalhos de autorização na documentação.
- **Formulários do Bazar:**
  - Carregar e normalizar os valores atuais de status e condição. Quando não forem alterados, preservá-los no salvamento.
  - Permitir alteração voluntária desses campos, sem convertê-los silenciosamente para valores padrão quando desconhecidos.
  - Manter dados preenchidos após falhas de salvamento.
  - Exigir confirmação antes da exclusão e bloquear o editor quando a API responder 403.
- **Painel do Vendedor:**
  - Manter a rota `/vendedor` como estrutura com acesso ao cadastro e ao perfil pessoal.
  - Confirmar `ehVendedor === true` pela API; não inferir vendedor pelo enum Role.
  - Não liberar conteúdo protegido enquanto o perfil carrega, quando a consulta falha ou quando não há perfil vendedor.
  - Preservar `/vendedor/produtos/novo` e `/vendedor/produtos/:id/editar` para os formulários.
  - Não carregar uma listagem de produtos no painel. A listagem será integrada em etapa própria.
  - Não exibir aviso de desenvolvimento nem controles de funcionalidades indisponíveis.
- **Navegação e Home:**
  - Perfil acessível por ícone à direita, com nome acessível “Meu perfil”.
  - Painel acessível apenas para vendedores, com ícone de loja e botão visualmente destacado.
  - Lupa global ao lado do perfil, com nome acessível “Pesquisar produtos”, abrindo `/produtos`.
  - Retirar o texto Pesquisar somente do cabeçalho; preservar o link textual no rodapé.
  - Menu recolhível em telas menores, com abrir/fechar, fechamento ao navegar e suporte à tecla Escape.
  - Em telas até 380px, ocultar somente o texto visual da marca, mantendo logo e identificação acessível.
  - Banner na Home antes dos anúncios recentes, com composição verde/creme, arte em CSS/ícones e atalho para o Bazar, sem dependência de imagens externas.
- **Pesquisa unificada:**
  - Reutilizar `BuscaProdutos`, `buscarProdutos` e a API existente, sem criar um segundo mecanismo de busca pública.
  - Ao pesquisar no Bazar, encaminhar o termo e `tipo=bazar` para `/produtos`.
  - Armazenar filtros aplicados e página na URL para preservar o estado ao voltar/avançar.
  - Limpar remove todos os filtros, inclusive o tipo Bazar.
  - Resultados de Bazar abrem o detalhe existente. Não criar rota de detalhe fictícia para Lanches.
  - Não exibir resultados obsoletos em falhas de consulta.
- **Trade-offs considerados:**
  - Estrutura de painel separada da listagem para evitar duplicação de funcionalidades.
  - Consulta protegida de edição em vez de depender apenas da visibilidade de botões.
  - Pesquisa única reutilizada entre acessos, em vez de filtros públicos independentes.
  - Lupa global para acesso consistente em diferentes páginas e dispositivos.

## Passos de implementação

1. Preservar uma versão recuperável dos arquivos e comparar alterações locais antes de aplicar a entrega.
2. Manter Domain, contratos e infraestrutura existentes, sem alterações de schema ou novos registros de DI.
3. Incorporar `ObterParaEdicao` em `Backend/src/Unihub.API/Controllers/ItensBazarController.cs`, preservando os demais métodos.
4. Aplicar os comportamentos de edição, exclusão e erro em `FormularioItemBazar.jsx`, `BotaoExcluirItemBazar.jsx`, `FormularioBazar.jsx`, `FormularioBazar.css` e `itensBazar.js`.
5. Usar `PerfilVendedor.jsx` para a consulta de perfil e proteção de interface; manter as rotas em `App.jsx` sem duplicar ou remover rotas existentes.
6. Manter `PainelVendedor.jsx` e `PainelVendedor.css` limitados à estrutura e aos atalhos de cadastro/perfil, sem consultas ou listagens de produtos.
7. Aplicar perfil por ícone, botão de loja, lupa e responsividade em `LayoutApp.jsx`, `LayoutApp.css` e `navegacao.js`.
8. Aplicar o banner em `Home.jsx` e `Home.css`, preservando a listagem de anúncios recentes.
9. Integrar a pesquisa em `Bazar.jsx`, `Bazar.css`, `BuscaProdutos.jsx` e `ProdutoCard.jsx`, reutilizando `produtosService.js` e as regras da API existente.
10. Executar lint/build do frontend, verificação de diff e testes de navegador dos fluxos afetados. Separar evidências de API simulada dos testes de integração real.
11. Validar localmente com backend e frontend em terminais separados. Usar comandos .NET na pasta que contém o `.csproj` e npm em Frontend, onde está `package.json`.
12. Para API local, configurar `VITE_API_BASE_URL=http://localhost:5206` em `Frontend/.env.local`, preservando outras variáveis. Reiniciar Vite após mudar a configuração e conferir a URL efetiva no Network. 

## Critérios de aceite

- [x] Edição de outros campos sem mudança obrigatória de status/condição — API simulada.
- [x] Alteração voluntária de status/condição, preservação de dados em falha e bloqueio do editor em 403 — API simulada.
- [x] Confirmação/cancelamento de exclusão — API simulada.
- [x] Painel sem listagem, busca interna, contadores, ações por item ou chamada a `/api/itens-bazar/meus` — teste de navegador.
- [x] Atalho do painel abre cadastro para vendedor; não vendedor é bloqueado — API simulada.
- [x] Banner, CTA, ícones, menu responsivo e lupa verificados no navegador com API simulada.
- [x] Pesquisa transfere termo/tipo e preserva paginação, histórico, limpeza de filtros e acesso ao detalhe — API simulada.
- [x] Ausência de overflow nas larguras verificadas de 320/768/1440px; navegação e ajustes visuais também verificados em 390px.
- [x] `npm run lint` e `npm run build` passaram na versão final da correção de escopo.
- [ ] `dotnet build` limpo da versão final — não executado por indisponibilidade do SDK neste ambiente.
- [ ] Verificar com contas distintas, na API real, o bloqueio de GET de edição, PUT e DELETE de produtos de terceiros.
- [ ] Validar o conjunto final com API real e a versão atual integrada do projeto.
- [ ] Concluir revisão independente das alterações relacionadas a permissões.

## Pendências / dúvidas em aberto

- Integrar a futura listagem Meus Produtos ao painel e aos formulário.
- Definir a integração futura de pedidos recebidos, Alimentação e configurações da loja; 
- Concluir compilação .NET, testes reais de autorização e revisão antes de publicação.
- Disponibilizar a consulta protegida de edição na API usada pelo frontend. Uma versão remota sem essa rota pode retornar 404; a URL efetiva deve ser conferida antes de diagnosticar ausência do produto.


---

## Code review — ajustes no guard de perfil de vendedor

Ajustes aplicados após o code review do PR da branch `feat/BRV-156-formularios-bazar`.
Objetivo: simplificar a sincronização do `PerfilVendedor` e reduzir chamadas à API,
mantendo o mesmo comportamento de guarda. A API pública do contexto
(`perfil`, `carregando`, `ehVendedor`, `erro`, `recarregar`) **não mudou** — nenhum
consumidor (`LayoutApp`, `Bazar`, `App`, `Perfil`) é afetado.

### Arquivos afetados
- `Frontend/src/auth/PerfilVendedor.jsx`
- `Frontend/src/services/api.js`

### Alterações

1. **`PerfilVendedor.jsx` — sincronização simplificada.**
   - Removidos o objeto `resultado` e a chave composta
     (`resultado?.token === token && pathname === pathname && tentativa === tentativa`).
   - Substituídos por estados explícitos: `carregando`, `perfil`, `erro`.
   - `AbortController` no `useEffect` cancela a chamada anterior; o cleanup chama
     `controlador.abort()`. A chamada passa o `signal`:
     `api.get('/api/perfil', { signal: controlador.signal })`. No `.catch`, erros de
     cancelamento são ignorados com `if (e?.name === 'AbortError') return;`.
   - `SomenteVendedor` mantido com o mesmo comportamento (carregando → status;
     erro → alert + "Tentar novamente"; sem perfil → "Acesso restrito"; libera só com
     `ehVendedor === true`). Comentário `eslint-disable` acima de `usePerfilVendedor`
     preservado.

2. **`PerfilVendedor.jsx` — menos chamadas à API.**
   - Removidos `pathname` e o uso de `useLocation` (de `react-router-dom`).
   - Dependências do `useEffect` mudaram de `[token, pathname, tentativa]` para
     `[token, tentativa]`: o perfil é verificado uma vez por sessão/token (e ao chamar
     `recarregar()`), não a cada troca de rota. O `token` vem do `useAuth()` e muda em
     login/logout, cobrindo a reverificação por sessão.
   - `recarregar` continua `() => setTentativa((n) => n + 1)`.

3. **`services/api.js` — suporte a cancelamento (retrocompatível).**
   - `signal` adicionado ao destructuring das opções de `request` e repassado ao `fetch`.
   - O `catch` do fetch passou de `catch {` para `catch (e) {` e relança o `AbortError`
     (`if (e?.name === 'AbortError') { throw e; }`) antes do `ApiError` genérico de rede.
   - Chamadas sem `signal` permanecem idênticas.

### Decisão tomada (lint `react-hooks/set-state-in-effect`)

O lint estrito do projeto (regra `react-hooks/set-state-in-effect`) acusou erro no
`setCarregando(true)` chamado sincronamente no corpo do `useEffect`. Esse reset é
**intencional**: reproduz o comportamento do código original (que derivava
`carregando` de `!atual` e, portanto, voltava a `true` a cada reverificação),
garantindo que a UI mostre "Verificando perfil de vendedor…" ao trocar de token ou
chamar `recarregar()`.

Decisão: manter o reset síncrono e suprimir a regra **apenas** nessa linha, com
`// eslint-disable-next-line react-hooks/set-state-in-effect` e um comentário
explicando que é uma sincronização deliberada com a API. O `setErro('')` na linha
seguinte não precisou de supressão (a regra só acusa o primeiro `setState` síncrono).
Alternativa considerada: não ressetar `carregando` no corpo do efeito — descartada por
alterar o comportamento de guarda que o review pediu para manter.

### O que NÃO foi alterado (confirmado no review)
- Status do produto nasce `Disponivel` no form de criação; só editável na edição.
- Edição preservando condição/status, confirmação antes de excluir, guarda contra
  duplo-clique.

### Validação
- [x] `npm run lint` limpo.
- [x] `npm run build` limpo.
