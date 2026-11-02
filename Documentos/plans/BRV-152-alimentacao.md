# Plano — BRV-152: Módulo de Alimentação

## Objetivo

Disponibilizar o módulo de Alimentação de ponta a ponta. Usuários autenticados poderão visualizar alimentos disponíveis na aba Alimentação e na Home e comprar alimentos para retirada imediata. Vendedores poderão cadastrar, consultar, editar e remover seus próprios anúncios de alimentos.

A entrega será feita em uma única branch, começando pelo backend e seguindo com o frontend, reaproveitando o contrato e o padrão visual do PR da Frente B quando estiver disponível.

## Escopo

### Inclui

- DTOs específicos de `Alimento`.

- `AlimentoService` com as regras de negócio.

- `AlimentoController` com CRUD e checagem de dono.

- Listagem pública de alimentos disponíveis e com estoque.

- Criação, edição e exclusão de anúncios pelo vendedor dono.

- Validação de quantidade, status e `HorarioDisponibilidade`.

- Página `/alimentacao` consumindo a API.

- Formulário de criação e edição de alimentos para vendedores.

- Ações de editar, excluir e repor estoque para o vendedor, usando a listagem geral de produtos do vendedor do Squad C.

- Botão `Comprar` para alimentos, sem opção, campo ou fluxo de `Reserva`.

- Exibição de alimentos recentes na Home.

- Separação entre os módulos: alimentos não aparecem no Bazar nem na Ação Solidária.

- Integração com o fluxo de pedidos existente, sem duplicar o `PedidoService`.

### NÃO inclui

- Reserva de alimentos.

- Alteração da regra de reservas de `ItemBazar`.

- Criação de um novo fluxo de pedidos dentro do módulo de Alimentação.

- Implementação do `PedidoService`, histórico de pedidos do vendedor ou conclusão de pedidos, que permanecem sob responsabilidade da Frente B.

- Alimentos dentro do endpoint ou da tela do Bazar.

- Nova migration: o mapeamento TPH de `Alimento` e a migration B1 já estão na `main`.

- Sistema de pagamento online, notificações em tempo real ou carrinho complexo.

## Decisões de design

### Entidades e banco

- Reutilizar `Alimento : Produto`, já existente no domínio.

- Manter em `Alimento` os campos `Categoria`, `Restricoes` e `HorarioDisponibilidade`.

- Não criar uma nova entidade para o anúncio de alimentação.

- O TPH já está configurado com `DbSet<Alimento>` e `.HasValue<Alimento>("Alimento")`.

- Nenhuma migration é esperada nesta task. Se surgir necessidade de alterar o schema, parar e alinhar com o Guardião de Migrations.

### API e autorização

Proposta inicial de endpoints:

```
GET    /api/alimentos
GET    /api/alimentos/{id}
POST   /api/alimentos
PUT    /api/alimentos/{id}
DELETE /api/alimentos/{id}
```

Todos os endpoints serão autenticados. O usuário autenticado será obtido pelo claim `sub`. O `VendedorId` nunca virá do corpo da requisição: será descoberto a partir do usuário autenticado.

`GET /api/alimentos` deve retornar somente alimentos que possam ser comprados:

```
Status == Disponivel
QuantidadeDisponivel > 0
```

A administração dos anúncios próprios usará o endpoint geral de produtos do vendedor, previsto na tarefa C1 do Squad C. Esse endpoint deverá incluir alimentos e itens do bazar, identificar o tipo do produto e permitir que o frontend filtre/mostre os alimentos na área de administração.

`GET /api/alimentos/{id}` poderá retornar o alimento mesmo quando estiver esgotado, exibindo seu status ao comprador e mantendo a ação de compra indisponível. A listagem/recomendação pública continua ocultando esgotados. Alimentos inativos devem retornar 404/NotFound para compradores, inclusive no acesso direto por ID. A busca por nome de esgotados só existirá se for implementado um endpoint de busca que inclua essa regra; não será presumida automaticamente pelo GET por ID.

### Compra imediata, sem reserva

Alimentos serão comprados para retirada imediata. Não haverá reserva de alimento nem escolha de tipo na tela. O usuário verá somente o botão `Comprar`; ao clicar, o frontend enviará uma compra direta para o fluxo de pedidos.

O backend da Alimentação não precisa expor `TipoPedido` no seu CRUD. A integração com o `PedidoService` da Frente B deve criar o pedido como `TipoPedido.Compra`, sem oferecer `Reserva` ao usuário. O decremento atômico de estoque, o rollback e o histórico continuam sob responsabilidade da Frente B.

O PR #60 do Nicolas mantém `TipoPedido.Compra` como padrão e bloqueia reservas de `Alimento`. Para a BRV-152, isso significa que o contrato utilizado pelo botão Comprar deve representar somente uma compra. A Alimentação não deve implementar outra versão dessas regras nem enviar `TipoPedido.Reserva`.

### Status e estoque

A quantidade determina o status do anúncio:

```
QuantidadeDisponivel > 0 → ProdutoStatus.Disponivel
QuantidadeDisponivel == 0 → ProdutoStatus.Esgotado
```

O frontend não deve ser a autoridade para o status. O service deve impedir quantidades negativas e calcular o status no backend.

Para compradores, alimentos esgotados desaparecem da listagem pública. Para vendedores, alimentos esgotados continuam visíveis na listagem geral de produtos do vendedor, com indicação de esgotado e possibilidade de editar a quantidade.

Ficar esgotado não remove um alimento dos pedidos anteriores. O histórico usa os dados persistidos no `ItemPedido`, incluindo a quantidade e o preço no momento da compra. O produto pode deixar de aparecer na vitrine sem apagar o registro histórico.

`ProdutoStatus.Inativo` é diferente de `Esgotado`: esgotado significa falta de estoque; inativo significa anúncio desativado pelo vendedor ou bloqueado por um administrador. O DELETE lógico deve marcar o alimento como `Inativo`, mantendo o registro no Neon para preservar referências históricas e permitir auditoria. Para compradores, alimento inativo deve ser tratado como inexistente: não aparece em anúncios, buscas ou recomendações, e o acesso direto deve retornar 404/NotFound. A visão administrativa do vendedor exibirá seus alimentos disponíveis, esgotados e inativos, com o status claramente identificado e possibilidade de reativação, usando o endpoint geral do vendedor do Squad C. Administradores também poderão consultar os registros inativos conforme suas permissões.

### Horário de disponibilidade

`HorarioDisponibilidade` será tratado como `TimeSpan` e permanecerá em `Alimento`. Nesta versão, será apenas informativo: o backend valida que o valor é um horário válido e o frontend o exibe como horário de retirada. O horário não bloqueará a compra nesta task.

### Frontend

A rota `/alimentacao` deixará de usar o componente `EmBreve` e terá uma página própria com:

- carregamento;

- erro;

- estado vazio;

- cards de alimentos;

- preço, categoria, restrições e horário;

- botão `Comprar`;

- formulário de criação e edição para vendedores;

- ações de excluir e repor estoque;

- indicação clara de alimento esgotado na visão do vendedor.

A Home combinará anúncios recentes do Bazar e da Alimentação. Cada fonte continuará separada: alimentos não serão enviados para `/api/itens-bazar` e não serão exibidos na tela do Bazar. A Home juntará os resultados, normalizará a origem de cada anúncio, ordenará por `DataPublicacao` decrescente e exibirá os mais recentes; o anúncio adicionado por último ficará primeiro na grade. O card compartilhado deve distinguir a origem para não enviar um alimento para `/bazar/{id}`.

O padrão visual e o comportamento de mensagens do botão Comprar devem ser reaproveitados do PR frontend da Frente B, desenvolvido pelo Gabriel, assim que ele estiver disponível.

## Passos de implementação

1. Criar DTOs em `Backend/src/UniHub.Application/DTOs` seguindo o padrão por módulo.

1. Criar `AlimentoService` com validação de entrada, posse, quantidade, status e horário.

1. Criar `AlimentoController`, usando o claim `sub` e registrando o service no DI do `Program.cs`.

1. Implementar e testar os endpoints públicos e o CRUD de Alimentação; a listagem geral do vendedor será consumida como dependência do Squad C.

1. Criar testes unitários para sucesso, validações, posse e estoque/status.

1. Rodar `dotnet build` e os testes do backend.

1. Conferir o PR da Frente B e alinhar o payload do botão Comprar.

1. Criar a página `/alimentacao`, seus serviços, componentes e estilos.

1. Integrar alimentos na Home sem alterar o endpoint do Bazar para retornar alimentos.

1. Rodar `npm run lint` e `npm run build`.

1. Testar o fluxo completo: anúncio → visualização → compra → estoque → histórico.

1. Abrir um Pull Request pequeno e revisar o checklist do `AGENTS.md`.

## Critérios de aceite

### Backend

- [ ] Usuário autenticado consulta alimentos sem receber `ItemBazar`.

- [ ] Alimento com quantidade zero não aparece na listagem pública.

- [ ] O frontend da BRV-152 consegue consumir a listagem geral de produtos do vendedor, entregue pelo Squad C, e identificar os alimentos próprios, inclusive esgotados e inativos.

- [ ] Vendedor cria alimento sem enviar `VendedorId`.

- [ ] Vendedor edita e exclui apenas o próprio alimento.

- [ ] Usuário não consegue alterar alimento de outro vendedor.

- [ ] Quantidade negativa é recusada.

- [ ] Quantidade positiva resulta em `Disponivel`.

- [ ] Quantidade zero resulta em `Esgotado`.

- [ ] A tela e o contrato de Alimentação não oferecem Reserva.

- [ ] O contrato usado pelo botão Comprar representa uma compra (`TipoPedido.Compra`); a implementação do pedido permanece na Frente B.

- [ ] Alimento `Inativo` não aparece para compradores e o acesso direto retorna 404/NotFound.

- [ ] Pedidos anteriores continuam no histórico quando o alimento fica esgotado.

- [ ] `dotnet build` limpo.

- [ ] Testes unitários passando.

### Frontend

- [ ] `/alimentacao` não exibe mais o placeholder `EmBreve`.

- [ ] A aba de Alimentação mostra apenas alimentos.

- [ ] A Home pode mostrar alimentos recentes.

- [ ] O Bazar continua mostrando apenas `ItemBazar`.

- [ ] O vendedor consegue cadastrar e editar alimentos.

- [ ] O vendedor consegue inativar e reativar seus alimentos, mantendo o registro no Neon.

- [ ] Alimentos esgotados ficam ocultos para compradores e visíveis ao dono.

- [ ] A tela mostra apenas o botão Comprar, sem botão ou seletor de Reserva.

- [ ] Mensagens de sucesso e erro seguem o padrão da Frente B.

- [ ] `npm run lint` e `npm run build` limpos.

### Integração

- [ ] Na integração final, o botão Comprar da Alimentação consegue usar o fluxo de pedidos da Frente B para criar uma compra; a BRV-152 não duplica nem reimplementa o `PedidoService`.

- [ ] O estoque é decrementado pelo fluxo atômico existente.

- [ ] A compra aparece no histórico do comprador.

- [ ] A compra pode ser consultada pelo fluxo do vendedor da Frente B.

- [ ] Um alimento não aparece no histórico do Bazar como se fosse `ItemBazar`.

## Pendências / dúvidas em aberto

- Alinhar o formato final do payload de compra com o PR da Frente B do Gabriel; para a Alimentação, ele deve representar somente `Compra`.

- Alinhar com Lucas o contrato do endpoint geral de produtos do vendedor, incluindo alimentos, esgotados e inativos.

- O histórico do vendedor e a consulta de pedidos ficam sob responsabilidade da Frente B; esta branch apenas integra o alimento como `ProdutoId` comprável.

- A Home combina endpoints separados de Bazar e Alimentação, sem alterar `/api/itens-bazar` para retornar alimentos.