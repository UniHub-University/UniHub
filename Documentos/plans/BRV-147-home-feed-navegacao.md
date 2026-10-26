## Objetivo
Entregar a Home com anúncios reais do Bazar, busca local e acesso aos detalhes, dentro da navegação autenticada. Manter uma apresentação provisória para as áreas ainda não integradas, sem ampliar o escopo do backend.

## Escopo
- **Inclui:** layout compartilhado nas rotas protegidas; feed do Bazar; busca local por nome, descrição e categoria; estados de carregamento, erro e lista vazia; navegação para detalhes; ajustes visuais; apresentação provisória de Alimentação.
- **NÃO inclui:** catálogo integrado de Alimentação; criação ou edição de produtos; associação de fotos; dados fictícios; busca ou paginação no servidor; interface de pedidos; processamento ou confirmação de Pix; alterações no backend ou banco. Perfil e Ação Solidária permanecem com telas provisórias.

## Decisões de design
- **Entidades/Domain:** nenhuma entidade criada ou alterada. A interface utiliza os dados existentes de ItemBazar.
- **Endpoints/API:** nenhuma rota nova. Consumo autenticado de `GET /api/itens-bazar` e `GET /api/itens-bazar/{id}` pelo wrapper `services/api.js`, com consultas organizadas em `services/anuncios.js`.
- **Banco/Migrations:** sem mudança de schema e sem migration nesta tarefa. Uma futura associação persistente de imagens deverá ser alinhada com o backend e, se alterar o schema, com o Guardião de Migrations.
- **Impacto em concorrência/LGPD:** consultas somente de leitura, sem reserva ou alteração de estoque. As telas ficam em rotas protegidas; isso não substitui a autorização da API. Não há novo armazenamento local de fotos ou dados pessoais. Revisar os campos exibidos para evitar exposição desnecessária de dados do vendedor.
- **Trade-offs considerados:** busca local permite filtrar os itens carregados sem mudar o contrato da API, mas não substitui busca global caso exista paginação. Alimentação usa o componente EmBreve existente para evitar novos módulos antes do alinhamento com o time. Foram descartados catálogo fictício e armazenamento local de fotos; ImagemProduto não integra esta entrega.

## Passos de implementação
1. **Domain:** sem alterações; utilizar as entidades existentes por meio dos contratos da API.
2. **Application:** sem alterações em interfaces, services ou DTOs do backend. Identificar os campos disponíveis para listagem e detalhe.
3. **Infrastructure:** sem alterações em repositórios ou persistência.
4. **API:** sem alterações em controllers ou DI no Program.cs. Consumir os endpoints existentes do Bazar.
5. **Frontend:**
   - Configurar LayoutApp nas rotas protegidas, preservando a página pública de boas-vindas/login.
   - Implementar o serviço de consulta de anúncios em services/anuncios.js.
   - Exibir o feed na Home, com busca local e estados de carregamento, erro e lista vazia.
   - Conectar os cards à rota /bazar/:id e exibir os detalhes do anúncio.
   - Tratar falhas de consulta, sessão inválida, acesso negado e anúncio não encontrado, conforme as respostas da API.
   - Ajustar tipografia, espaçamento e organização visual da Home e do detalhe.
   - Personalizar a apresentação provisória de Alimentação no EmBreve, preservando Perfil e Ação Solidária.
   - Validar lint, build e navegação; registrar os resultados neste documento.

## Critérios de aceite
- [x] Usuário autenticado acessa a Home e visualiza os anúncios retornados pela API do Bazar.
- [x] Busca local filtra os itens carregados por nome, descrição e categoria.
- [x] Carregamento, falha de consulta, lista vazia e busca sem resultados recebem apresentação adequada.
- [x] Cada card abre o detalhe correspondente em /bazar/:id.
- [x] Detalhe exibe preço, descrição, categoria, condição, status e quantidade conforme os dados disponíveis na API.
- [x] Sessão inválida, acesso negado e anúncio inexistente são tratados sem quebrar a tela.
- [x] Voltar ao feed e atualizar a página de detalhe funcionam.
- [x] Logout impede novo acesso às telas protegidas até a autenticação.
- [x] Alimentação exibe somente sua apresentação provisória, sem catálogo fictício ou chamadas a endpoints ainda não integrados.
- [x] Perfil e Ação Solidária mantêm o comportamento anterior.
- [x] `npm run lint` passa na pasta Frontend.
- [x] `npm run build` passa na pasta Frontend.

**Verificações do modelo não aplicáveis ao escopo:**
- `dotnet build`: sem alteração .NET nesta tarefa; executar se exigido pelo fluxo de integração do time e registrar o resultado.
- Migration testada em branch do Neon: não aplicável, pois não há alteração de schema.


## Pendências / dúvidas em aberto
- **Alimentação:** confirmar disponibilidade e contratos dos endpoints de listagem e detalhe. Na main consultada anteriormente, a entidade existia, mas essas rotas não foram identificadas.
- **Imagens:** definir uma ou várias fotos por produto e como persistir e retornar o vínculo da URL. O upload existente, isoladamente, não resolve essa associação.
- **Busca e paginação:** alinhar parâmetros e formato de resposta com o Squad 2 antes de substituir a busca local.
- **Visibilidade:** definir a regra para anúncios inativos no feed e em qual camada ela será aplicada.
- **Pedidos/Pix:** definir responsáveis pela interface de pedido, geração do QR Code/payload e confirmação de pagamento. Reserva de estoque não representa pagamento confirmado.
- **Integração:** confirmar a branch de destino do PR com o squad, sem merge direto na main.

As integrações futuras acima ficam fora desta entrega. A regra de visibilidade e o destino do PR precisam ser alinhados para a integração da tarefa.