# Deploy da API UniHub — Render + Neon

Runbook do **Squad 4 (Deploy)**. Descreve como publicar a API .NET 10 no
**Render** (plano free), mantendo o banco no **Neon** (Postgres serverless),
conforme decisão do time.

> **Por que Render e não AWS EC2 / RDS?** O enunciado original mencionava
> EC2 + RDS, mas o time optou por **Neon** (Postgres serverless, scale-to-zero,
> branching por squad) e **Render** para a API (sem cartão de crédito, HTTPS
> automático, deploy a partir do repositório). Vale registrar essa justificativa
> na apresentação final para antecipar a pergunta da banca.

---

## 1. Arquitetura de deploy

```
  Frontend (React + Vite, hospedado à parte)
            │  HTTPS
            ▼
  Render  ──►  Container Docker da API (.NET 10, plano free)
            │  Postgres via SSL (connection string com pooling)
            ▼
  Neon (Postgres serverless)
            ▲
  Cloudinary (armazenamento de imagens, chamado pela API)
```

- A API sobe como **container Docker** (`Backend/Dockerfile`).
- O Render fornece **HTTPS automático** em `https://<nome>.onrender.com`.
- Nenhum segredo fica no repositório — tudo vem de **variáveis de ambiente**
  no painel do Render (seção 4).

---

## 2. Arquivos de deploy neste repositório

| Arquivo | Papel |
|---|---|
| `Backend/Dockerfile` | Build multi-stage da API (.NET 10), imagem enxuta. |
| `Backend/.dockerignore` | Evita enviar segredos/lixo para a imagem. |
| `Backend/appsettings.Production.json` | Overrides de produção (logging). **Sem segredos.** |
| `render.yaml` | Blueprint do serviço Render (infra como código). |
| `.github/workflows/ci.yml` | CI: build + checagem de migrations + build Docker. |
| `Backend/Program.cs` | Lê `PORT`; expõe `/health`; CORS por env var; log sensível só em Development. |
| `Backend/appsettings.json` | Chave JWT como placeholder (segredo nunca versionado). |

---

## 3. Pré-requisitos

- Conta no [Render](https://render.com) (login com o GitHub da organização).
- Acesso ao projeto no **Neon** e à *connection string com pooling*.
- Credenciais do **Cloudinary** (CloudName, ApiKey, ApiSecret).
- Uma chave para o **JWT** com **pelo menos 32 caracteres** (gere uma nova —
  a antiga, que esteve versionada, é considerada comprometida).

---

## 4. Variáveis de ambiente de produção

O ASP.NET Core mapeia variáveis de ambiente para a configuração usando `__`
(duplo underline) no lugar de `:`. Defina todas no painel do Render
(**Environment**) ou deixe o `render.yaml` solicitá-las na criação do Blueprint.

| Variável | Exemplo / Observação |
|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` (ativa CORS restrito e desliga Swagger). |
| `ConnectionStrings__DefaultConnection` | String do Neon **com pooling e SSL** (seção 5). |
| `JwtSettings__SecureKey` | Chave secreta ≥ 32 caracteres (nova). |
| `Cloudinary__CloudName` | Do painel do Cloudinary. |
| `Cloudinary__ApiKey` | Do painel do Cloudinary. |
| `Cloudinary__ApiSecret` | Do painel do Cloudinary. |
| `Cors__AllowedOrigins__0` | URL do frontend, ex.: `https://unihub-app.vercel.app`. |
| `Cors__AllowedOrigins__1` | (Opcional) origem adicional. |
| `PORT` | **Injetada automaticamente pelo Render** — não definir manualmente. |

> ⚠️ **Nunca** cole a connection string, a chave JWT ou segredos do Cloudinary
> em arquivos versionados. O `appsettings.json` só tem placeholders.

---

## 5. Connection string do Neon (pooling + SSL)

Use **sempre** o endpoint com pooling (o host costuma ter `-pooler` no nome).
A conexão direta esgota o limite de conexões sob carga.

Formato (Npgsql):

```
Host=<endpoint>-pooler.<regiao>.aws.neon.tech;Database=unihub;Username=<user>;Password=<senha>;SSL Mode=Require;Channel Binding=Require;Pooling=true
```

- `SSL Mode=Require` + `Channel Binding=Require`: o Neon exige TLS.
- O Neon aceita conexões de qualquer IP com SSL por padrão, então **não é
  necessário liberar IP** para o Render. (O IP de saída do plano free do Render
  não é fixo, então restrição por IP não é recomendada aqui.)

---

## 6. Migrations desta sprint (aplicar ANTES do primeiro deploy)

O banco de produção precisa ter **todas** as migrations do projeto aplicadas,
nesta ordem:

1. `InicialConsolidada` — schema base (usuários, produtos, pedidos, e as tabelas
   de Ação Solidária no singular).
2. `AdicionaChavePixVendedor` (#35) — coluna `ChavePix` em `Vendedores`.
3. `AdicionaModuloAcaoSolidaria` (#38) — ajusta o módulo de Ação Solidária:
   renomeia tabelas para o plural, converte `Categoria`/`AreasDeAtuacao` de
   texto para enum (via `USING`), e adiciona as FKs com `Restrict`.

> ⚠️ **Atenção à migration #38:** ela faz conversão de tipo (`text` → `integer`)
> usando cláusula `USING`. Isso só roda sem erro se as tabelas de Ação Solidária
> estiverem **vazias** ou com dados convertíveis. Em produção limpa, é seguro.

**A API NÃO aplica migrations automaticamente no startup** (proposital, para um
deploy nunca rodar uma migration incompleta sob concorrência). O responsável
(Guardião de Migrations) aplica manualmente:

```bash
cd Backend
# Aponte a connection string para o Neon de produção nesta sessão:
export ConnectionStrings__DefaultConnection="Host=...-pooler...;Database=unihub;...;SSL Mode=Require;Pooling=true"

# Confira o que será aplicado (sem executar)
dotnet ef migrations list

# Aplique
dotnet ef database update
```

> 💡 **Fluxo seguro (recomendado):** use o **branching do Neon** — crie uma
> branch do banco, aplique e teste a migration nela primeiro, e só então aplique
> na branch de produção. Se algo der errado, descarte a branch sem afetar nada.

---

## 7. Passo a passo do deploy

### Opção A — via Blueprint (recomendada, usa `render.yaml`)

1. No Render: **New** → **Blueprint**.
2. Conecte o repositório `UniHub-University/UniHub` e selecione a branch `main`.
3. O Render lê o `render.yaml` e cria o serviço `unihub-api`.
4. Preencha os segredos marcados como `sync: false` (connection string, JWT,
   Cloudinary) quando solicitado.
5. Confirme e aguarde o primeiro build/deploy.

### Opção B — serviço manual

1. **New** → **Web Service** → conecte o repositório.
2. **Runtime:** Docker. **Dockerfile Path:** `Backend/Dockerfile`.
   **Docker Context:** `Backend`.
3. **Plan:** Free.
4. **Health Check Path:** `/health`.
5. Adicione as variáveis de ambiente da seção 4.
6. **Create Web Service**.

---

## 8. Cold start (Neon + Render free) — relevante para testes de carga

Dois cold starts se somam no plano free:

1. **Render** derruba o serviço após **15 min** sem tráfego; a próxima
   requisição espera o container subir (~dezenas de segundos).
2. **Neon** hiberna quando ocioso; a primeira query tem latência extra.

Antes de medir o teste de concorrência **em produção**, "aqueça" os dois:

```bash
# Aquece o Render (sobe o container) e o Neon (primeira query).
curl -s https://<nome>.onrender.com/health
# Faça mais 1-2 chamadas que toquem o banco antes de iniciar a medição.
```

Para evitar o spin down durante uma demonstração, aponte um cron externo
(ex.: cron-job.org) para `GET /health` a cada ~10 minutos. Lembrete: o plano
free do Render tem cota de ~750h/mês por workspace — manter o serviço acordado
24/7 consome quase toda a cota.

---

## 9. Teste de fumaça pós-deploy (fazer em produção, não só local)

```bash
BASE=https://<nome>.onrender.com

# 1. Health check
curl -s $BASE/health                                   # espera {"status":"ok",...}

# 2. Rota protegida sem token deve dar 401
curl -s -o /dev/null -w "%{http_code}
" $BASE/api/perfil   # espera 401
```

Checklist de validação em produção:

- [ ] `/health` responde 200.
- [ ] Swagger **não** aparece em produção (só em Development) — esperado.
- [ ] Rota `[Authorize]` rejeita chamada sem token (401).
- [ ] Login Google emite JWT e rota protegida aceita o token.
- [ ] CORS libera o domínio real do frontend e bloqueia os demais.
- [ ] **Alimentação:** criar pedido decrementa estoque; concorrência não deixa estoque negativo.
- [ ] **Bazar:** CRUD de item respeita o dono (não dá para editar item de outro vendedor).
- [ ] **Ação Solidária:** criar/cancelar solicitação; listagem **nunca** expõe identidade do solicitante.
- [ ] **PIX:** chave do vendedor aparece no detalhe do item, não na listagem.
- [ ] **Moderação:** admin suspende conta e o usuário suspenso é bloqueado no login.

---

## 10. Troubleshooting

| Sintoma | Causa provável | Ação |
|---|---|---|
| Deploy sobe mas health check falha / "no open ports" | App não escutou em `PORT`/`0.0.0.0` | Conferir o trecho de `PORT` no `Program.cs`; não sobrescrever `ASPNETCORE_URLS`. |
| 500 no primeiro acesso a rota de banco | Connection string errada / sem pooling / sem SSL | Revisar `ConnectionStrings__DefaultConnection` (seção 5). |
| 401 em toda rota protegida mesmo com token válido | `JwtSettings__SecureKey` diferente da usada para assinar | Garantir a mesma chave; ≥ 32 chars. |
| CORS bloqueando o frontend | `Cors__AllowedOrigins__0` não bate com a URL real | Ajustar a origem (sem barra no final). |
| Erro de migration / "cannot be cast automatically" | Conversão de tipo com dados existentes | Ver seção 6; tabelas devem estar vazias ou usar `USING`. Testar em branch do Neon. |
| Primeira requisição do dia muito lenta | Cold start Render + Neon | Esperado no free; aquecer antes de medir (seção 8). |
