# Plano — BRV-115 — Perfil de usuário

## Objetivo

Entregar uma página autenticada para o usuário visualizar seus dados principais e editar as informações permitidas do próprio perfil. A página deve consumir a API existente, fornecer estados claros de carregamento, sucesso e erro, e respeitar a identidade visual do frontend.

## Escopo

- **Inclui:**
  - Criar a rota protegida `/perfil` no frontend.
  - Buscar os dados do usuário autenticado por `GET /api/perfil`.
  - Exibir nome completo, e-mail institucional, telefone, foto, papel e indicadores de perfil de vendedor/voluntário.
  - Permitir edição do nome completo e telefone por `PUT /api/perfil`.
  - Permitir seleção e envio de uma nova foto de perfil usando `POST /api/upload/imagem?pasta=perfis` e salvar a URL retornada por `PUT /api/perfil`.
  - Validar nome obrigatório com 2 a 150 caracteres.
  - Validar telefone opcional com 10 ou 11 dígitos, considerando DDD mais número, persistindo somente os dígitos.
  - Exibir máscara de telefone apenas na interface, sem persistir a máscara na API ou no banco.
  - Manter o e-mail institucional somente para leitura.
  - Reutilizar o `AuthContext`, o `RotaProtegida` e o wrapper `api` criados no PR #45.
  - Tratar carregamento, erro de consulta, erro de atualização e sucesso da atualização.
  - Disponibilizar a rota `/perfil` para ser acessada pela página principal.
  - Alinhar com Pedro e Maria a inclusão de um link ou botão na página `/home` apontando para `/perfil`, usando `Link` ou `navigate` do React Router.

- **NÃO inclui:**
  - Alteração de outras entidades além de `Usuario`.
  - Alteração do schema do banco de dados ou criação de migrations.
  - Criação de novos endpoints no backend, salvo se o contrato atual se mostrar insuficiente durante a implementação.
  - Edição de e-mail, `GoogleId`, papel do usuário ou status da conta.
  - Implementação da tela principal da plataforma, anúncios ou módulos de negócio.
  - Criação ou alteração estrutural da página `/home`; essa responsabilidade pertence a Pedro e Maria.
  - Gestão administrativa de perfis de outros usuários.

## Decisões de design

- **Entidades/Domain:** alterar `Backend/src/UniHub.Domain/Entities/Usuario.cs`, especialmente `AtualizarDados(...)`, para aplicar as regras de nome e telefone. Essa alteração é de regra de negócio e não altera o schema do banco.

- **Endpoints/API:**
  - `GET /api/perfil` para carregar o perfil do usuário autenticado.
  - `PUT /api/perfil` com `{ nomeCompleto, telefone, fotoUrl? }` para atualizar os dados editáveis.
  - O usuário será identificado exclusivamente pelo JWT enviado pelo wrapper `api`; nenhum `usuarioId` será recebido do frontend.

- A API deve aceitar telefone vazio, mas rejeitar telefone preenchido que não tenha 10 ou 11 dígitos.

- **Banco/Migrations:** nenhuma mudança prevista. O backend já persiste nome, telefone e foto.

- **Impacto em concorrência/LGPD:** não há impacto de estoque. A página deve exibir somente os dados do próprio usuário retornados pelo endpoint autenticado. O e-mail institucional é exibido, mas não editável. `GoogleId` não deve aparecer na resposta nem na interface.

- **Trade-offs considerados:**
  - Reutilizar a API existente é preferível a duplicar lógica de usuário no frontend.
  - O estado local do formulário será separado do usuário salvo no `AuthContext`, evitando alterar a sessão antes de a API confirmar a atualização.
  - A edição da foto entra nesta história, apesar de exigir o fluxo adicional `multipart/form-data` em `/api/upload/imagem?pasta=perfis`.
  - A data de cadastro não será exibida na interface nesta primeira versão.
  - A máscara será somente visual; o valor persistido conterá apenas dígitos.
  - A validação será feita no frontend e no backend: o frontend fornece feedback rápido, enquanto o backend protege a integridade dos dados para qualquer cliente da API.

## Passos de implementação

1. **Domain:** alterar `Usuario.cs` para validar e normalizar nome e telefone dentro de `AtualizarDados(...)`. O nome será obrigatório e terá entre 2 e 150 caracteres; o telefone vazio será aceito e, quando preenchido, deverá resultar em 10 ou 11 dígitos persistidos sem máscara.

1. **Application:** revisar `AtualizarPerfilDto` para manter o contrato de entrada claro. A validação definitiva ficará protegida pela entidade; o DTO não deverá substituir a regra de domínio.

1. **Infrastructure:** nenhuma alteração.

1. **API:** ajustar o tratamento de `PUT /api/perfil` para rejeitar nome vazio, nome com menos de 2 ou mais de 150 caracteres e telefone preenchido fora do formato normalizado. Confirmar manualmente o comportamento de `GET /api/perfil` e `PUT /api/perfil` com JWT válido.

1. **Frontend:**
    1. Criar `src/pages/Perfil.jsx`.
    1. Criar `src/pages/Perfil.css`.
    1. Adicionar a rota protegida `/perfil` em `App.jsx`.
    1. Carregar os dados no `useEffect` usando `api.get('/api/perfil')`.
    1. Preencher o formulário com os dados recebidos.
    1. Enviar alterações usando `api.put('/api/perfil', payload)`.
    1. Adicionar validação local do nome: obrigatório, entre 2 e 150 caracteres.
    1. Adicionar validação local do telefone: opcional, mas com 10 ou 11 dígitos quando preenchido.
    1. Aplicar máscara de telefone somente na apresentação e enviar à API apenas os dígitos.
    1. Adicionar estados de carregamento, salvamento, erro e sucesso.
    1. Disponibilizar a rota `/perfil` para ser consumida pela página `/home`, sem criar uma segunda aplicação ou um segundo site.
    1. Alinhar o contrato de navegação com Pedro e Maria: a página principal deverá renderizar um `Link` ou executar `navigate('/perfil')`.
    1. Validar responsividade e acessibilidade básica dos campos, labels, mensagens e botões.

## Critérios de aceite

- [ ] Usuário não autenticado não acessa `/perfil` e é redirecionado para `/`.

- [ ] Usuário autenticado consegue abrir `/perfil`.

- [ ] A página `/home` possui um link ou botão que aponta para `/perfil` usando o React Router; a implementação desse link fica sob responsabilidade de Pedro e Maria.

- [ ] A página exibe os dados retornados por `GET /api/perfil`, exceto a data de cadastro.

- [ ] Nome completo e telefone aparecem como campos editáveis.

- [ ] Nome é obrigatório e aceita entre 2 e 150 caracteres.

- [ ] Telefone vazio é aceito.

- [ ] Telefone preenchido deve conter 10 ou 11 dígitos, incluindo DDD.

- [ ] Telefone é persistido somente com dígitos.

- [ ] Máscara de telefone aparece somente na interface.

- [ ] Frontend impede o envio quando os dados são inválidos.

- [ ] Backend rejeita dados inválidos mesmo que a validação do frontend seja ignorada.

- [ ] E-mail institucional aparece como somente leitura.

- [ ] Foto atual é exibida quando houver uma URL válida.

- [ ] Usuário consegue selecionar uma nova imagem, enviá-la para a pasta `perfis` e salvar a URL no perfil.

- [ ] Erros de formato, tamanho, upload ou atualização da foto são exibidos sem perder os demais dados do formulário.

- [ ] O formulário envia apenas os dados permitidos para `PUT /api/perfil`.

- [ ] Após salvar com sucesso, a interface informa o resultado e reflete os dados atualizados.

- [ ] Erros de API e falhas de rede são exibidos sem quebrar a página.

- [ ] A tela possui estado de carregamento durante a consulta e o salvamento.

- [ ] Nenhum `usuarioId` vindo da interface é usado para determinar o perfil consultado ou alterado.

- [ ] `npm run lint` limpo.

- [ ] `npm run build` limpo.

- [ ] `dotnet build` limpo, quando executado em ambiente com o SDK .NET instalado.

## Pendências / dúvidas em aberto

1. Pedro e Maria devem incluir na implementação da página principal um link ou botão para `/perfil`. A BRV-115 não implementará a página `/home`, apenas fornecerá a rota protegida e a tela de perfil.
