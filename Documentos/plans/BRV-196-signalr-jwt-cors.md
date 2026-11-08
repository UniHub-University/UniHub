# Plano — BRV-196 (Frente 2, Sprint 5, task A2): JWT via query string + CORS para SignalR

> Continuação da Frente 2 (Painel do Vendedor em Tempo Real). A task A1 (BRV-185)
> entregou o `PainelHub`, o `AddSignalR()` e o `MapHub("/hubs/painel")`. Esta task
> (A2) ajusta a **infra de autenticação e CORS** para que o handshake do SignalR
> por WebSocket funcione com JWT. Só o `Program.cs` muda.

## Objetivo
Permitir que uma conexão SignalR autenticada feche o handshake ponta-a-ponta:
(1) o JWT precisa ser aceito pela query string nos paths de hub, porque o
WebSocket não envia o header `Authorization`; (2) o CORS precisa permitir
credenciais, exigência do SignalR autenticado. Sem a A2, o `PainelHub` existe mas
não dá para conectar autenticado a partir do navegador.

## Escopo
- **Inclui:**
  - `JwtBearerEvents.OnMessageReceived` lendo `access_token` da query string
    apenas para paths iniciando em `/hubs/`.
  - CORS com `AllowCredentials()` nos dois ambientes (Development e Produção),
    ajustando o ramo de Development para ser compatível com credenciais.
- **NÃO inclui:**
  - Lógica de eventos (`EstoqueAtualizado`/`NovoPedido`) → tasks A3/A4.
  - Mudança no `PainelHub` (já entregue na A1).
  - Definição da origem exata de produção no painel do Render (ação de deploy,
    fora do código — registrada como pendência).

## Relação entre branches (PR empilhado / stacked — AGENTS §9)
- Branch desta task: **`feat/BRV-196-signalr-jwt-cors`**.
- **Base:** `feat/BRV-185-painel-hub-signalr` (NÃO a `main`), porque a A2 depende
  da A1 (o hub e o `MapHub` precisam existir para a mudança de JWT/CORS fazer
  sentido).
- **Ordem de merge:** primeiro o PR da A1 (BRV-185), depois o da A2 (BRV-196).
  Deixar essa dependência explícita na descrição do PR.

## Decisões de design

### Alteração 1 — JWT via `access_token` na query string
No `.AddJwtBearer(...)`, adicionar `options.Events = new JwtBearerEvents { ... }`
com `OnMessageReceived`. O handler copia `Request.Query["access_token"]` para
`context.Token` **somente** quando o path começa com `/hubs`.

Racional:
- O handshake do SignalR por WebSocket não manda o header `Authorization`; o
  cliente envia o JWT via `?access_token=...`. Esse é o padrão oficial do
  SignalR para autenticação com Bearer token.
- A restrição por path (`/hubs`) é deliberada: as rotas REST continuam
  autenticando **só** pelo header `Authorization`. Não queremos abrir um segundo
  canal de autenticação (token na URL) para a API REST — token em query string
  tende a vazar em logs de servidor/proxy, então limitamos ao estritamente
  necessário (os hubs).
- `JwtBearerEvents` já está disponível: o `using
  Microsoft.AspNetCore.Authentication.JwtBearer;` já existe no `Program.cs`.

Trade-off considerado: ler `access_token` para qualquer path (mais simples) vs.
só para `/hubs` (escolhido). Optamos por restringir para reduzir a superfície de
exposição do token na URL.

### Alteração 2 — CORS com `AllowCredentials()`
Adicionar `.AllowCredentials()` nos dois ramos (Development e Produção) e, **em
Development, trocar `AllowAnyOrigin()` por `SetIsOriginAllowed(_ => true)`**.

Racional (o ponto mais delicado da task):
- O SignalR autenticado exige `AllowCredentials()` no CORS.
- `AllowCredentials()` é **incompatível** com `AllowAnyOrigin()`: o ASP.NET Core
  lança exceção em runtime se os dois forem combinados. Logo, não dá para apenas
  "adicionar `AllowCredentials()`" sobre o `AllowAnyOrigin()` atual de Development.
- A correção padrão é `SetIsOriginAllowed(_ => true)` em Development: continua
  liberando qualquer origem (comportamento de dev preservado), mas de forma
  compatível com credenciais (ecoa a origem específica no header
  `Access-Control-Allow-Origin` em vez de `*`).
- Em Produção, o ramo já usa `WithOrigins(allowedOrigins)` (lista fechada, origem
  do front oficial). Adicionar `.AllowCredentials()` aí é seguro justamente
  porque a lista é restrita — não é origem aberta.

Trade-offs considerados:
- **Em vez de `SetIsOriginAllowed(_ => true)` em dev, listar `localhost:5173`
  explicitamente:** mais restritivo, porém frágil (a porta do Vite pode variar;
  outros devs usam hosts diferentes). Para o ambiente local, manter o
  comportamento "qualquer origem" (agora compatível com credenciais) é o que menos
  atrita no fluxo de desenvolvimento. Escolhido `SetIsOriginAllowed`.
- **Criar uma política de CORS separada só para os hubs:** mais isolado, porém
  adiciona complexidade (duas políticas, decidir qual aplica onde) sem ganho real
  nesta fase. A política única `PermitirFrontend` já cobre REST + hub. Descartado
  por ora (reavaliar se surgir necessidade).

## Mudança de comportamento registrada
- **Development:** `AllowAnyOrigin()` → `SetIsOriginAllowed(_ => true)` +
  `AllowCredentials()`. Na prática continua liberando tudo em dev, mas agora o
  `Access-Control-Allow-Origin` reflete a origem da requisição (não `*`), que é o
  requisito para credenciais.
- **Produção:** acrescenta `.AllowCredentials()` ao `WithOrigins(...)` existente.
  A lista de origens permitidas não muda; é a mesma de antes
  (`Cors:AllowedOrigins`, com fallback para os domínios de exemplo).

## Passos de implementação
1. Bloco JWT (`.AddJwtBearer`): adicionar `options.Events` com
   `OnMessageReceived` lendo `access_token` para paths `/hubs`.
2. Bloco CORS (`PermitirFrontend`): em Development usar
   `SetIsOriginAllowed(_ => true)...AllowCredentials()`; em Produção acrescentar
   `.AllowCredentials()` ao `WithOrigins(...)`.
3. `dotnet build` limpo.

## Critérios de aceite (A2)
- [ ] JWT aceito via `?access_token=` apenas em paths `/hubs/` (REST continua só
      pelo header).
- [ ] CORS permite credenciais sem lançar exceção em runtime (dev e prod).
- [ ] Development libera qualquer origem de forma compatível com credenciais.
- [ ] Produção mantém a lista fechada de origens + credenciais.
- [ ] `dotnet build` limpo (0 avisos, 0 erros).
- [ ] PR isolado do `Program.cs` (AGENTS §9), empilhado sobre a BRV-185.

## Pendências / dúvidas em aberto
- **Origem exata de produção (Render):** combinar com quem cuida do deploy a(s)
  origem(ns) a configurar em `Cors:AllowedOrigins` (via `Cors__AllowedOrigins__0`,
  ...). A mudança de código é compatível; falta a config de ambiente.
- **Teste real de conexão:** com a A2 aplicada, dá para validar a conexão
  autenticada do front (trilha B) contra o hub stub da A1 — fica para a
  integração/A5.
- **Convenção nova de SignalR:** se consolidar (ex.: padrão de `access_token` +
  CORS), adicionar ao `AGENTS.md` no mesmo PR que a introduz (AGENTS §16).
