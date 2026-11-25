# UniHub 🎓

O **UniHub** é um ecossistema digital desenvolvido para centralizar e otimizar as interações socioeconômicas no campus universitário da UNIFESP. O projeto combate a fragmentação de informações (hoje espalhadas em grupos de WhatsApp) através de uma plataforma unificada, de acesso restrito à comunidade acadêmica (`@unifesp.br`), organizada em três pilares: **comércio de alimentos**, **revenda de materiais acadêmicos** e **impacto social**.

> Projeto desenvolvido na disciplina de **Engenharia de Software**, com metodologia Scrum (sprints de 2 semanas) por uma equipe de 7 integrantes.

---

## → Módulos do Sistema

* 🍔 **Alimentação:** vendedores anunciam e gerenciam o estoque de lanches, e alunos compram pela plataforma — evitando filas e incertezas no intervalo. É um dos módulos que concentram o maior desafio técnico (concorrência de estoque).
* ▪ **Bazar Acadêmico:** marketplace de compra e venda de materiais universitários entre alunos (livros, calculadoras, equipamentos), reaproveitando a mesma modelagem de produto/estoque. Suporta **compra** e **reserva temporária** (que expira e devolve o estoque automaticamente).
* 🤝 **Ação Solidária:** conecta alunos em situação de vulnerabilidade a voluntários, com **pseudonimização rigorosa** da identidade do solicitante (privacy by design / LGPD).
* 🛡 **Moderação:** qualquer usuário pode denunciar um anúncio ou um vendedor (com motivo e justificativa); administradores acompanham as denúncias em um painel restrito (role `Admin`), mudam o status e aplicam ações (suspender/reativar conta, inativar produto). O denunciante acompanha o desfecho das próprias denúncias.

---

## 🏛 Arquitetura

O backend segue uma **Clean Architecture simplificada** (projeto único, camadas separadas por pastas), em que cada camada só conhece a camada abaixo:

```
API  →  Application  →  Infrastructure  →  Domain
```

* **Domain** — entidades puras, sem dependência de nada (regras e invariantes de negócio).
* **Application** — serviços (regra de negócio), interfaces de repositório e DTOs, organizados por módulo.
* **Infrastructure** — EF Core, `AppDbContext`, repositórios concretos e integrações externas (Cloudinary).
* **API** — Controllers, autenticação JWT, Hubs (SignalR) e composição via injeção de dependência.

![Diagrama de Arquitetura do UniHub](Documentos/UniHub%20-%20Diagrama%20Arquitetural.png)

O diagrama acima mostra o fluxo completo: o **Frontend (React + Vite)** se autentica via **Google OAuth**, consome a **API REST (.NET)** enviando o **JWT**, e a API orquestra as entidades de domínio, persistindo em **PostgreSQL (Neon)** via Entity Framework Core e armazenando imagens no **Cloudinary**. Eventos de tempo real (painel do vendedor) trafegam por um **hub SignalR**.

---

## 🛠 Stack Tecnológica

| Camada | Tecnologia |
|---|---|
| **Frontend** | React + Vite (JavaScript, SPA) — requer Node 20+ |
| **Backend** | C# / **.NET 10** (ASP.NET Core Web API) |
| **ORM** | Entity Framework Core 10 (provider `Npgsql.EntityFrameworkCore.PostgreSQL`) |
| **Banco de Dados** | **Neon** (PostgreSQL serverless) |
| **Autenticação** | Google OAuth (login institucional) + JWT emitido pela própria API |
| **Tempo real** | SignalR (painel do vendedor ao vivo) |
| **Armazenamento de Imagens** | Cloudinary |
| **Borda / rede** | Cloudflare (HTTPS e mitigação de DDoS na borda) |
| **Testes** | xUnit + NSubstitute (unidade, integração e concorrência) |
| **Documentação de API** | Swagger / Swashbuckle (apenas em ambiente de Development) |
| **Deploy** | Render (container Docker) — ver [`DEPLOY.md`](DEPLOY.md) |

---

## 🧠 Decisões de Engenharia e Diferenciais Técnicos

* **Controle de Concorrência Otimista (alta carga):** para suportar os picos do intervalo, o estoque **não** usa locks em memória (que não funcionam com múltiplas instâncias atrás de um load balancer). O decremento é um **UPDATE condicional atômico** no banco:
  ```sql
  UPDATE "Produtos"
  SET "QuantidadeDisponivel" = "QuantidadeDisponivel" - @quantidade
  WHERE "Id" = @id AND "QuantidadeDisponivel" >= @quantidade
  ```
  Se duas requisições disputam a última unidade, apenas uma afeta uma linha; a outra recebe zero linhas afetadas e sabe, de forma confiável, que perdeu a corrida. O mesmo padrão rege a conclusão e a expiração de pedidos (UPDATE condicional por `Status`), com o efeito colateral (ex.: devolver estoque) aplicado só por quem ganha a corrida. O comportamento é **provado por teste de concorrência real** (N compradores simultâneos disputando 1 unidade → exatamente 1 vence, estoque nunca fica negativo).

* **Painel do vendedor em tempo real (SignalR):** o estoque e os novos pedidos refletem no painel do vendedor **ao vivo, sem refresh**. Um hub SignalR isola os eventos por vendedor (`vendedor:{id}`), e a emissão é best-effort — uma falha no tempo real **nunca** derruba a venda (o painel degrada para o estado inicial via REST).

* **Reserva temporária com expiração automática:** no Bazar, além de comprar, o aluno pode **reservar** um item por tempo limitado. Reservas vencidas têm o estoque devolvido e o status alterado para `Expirado` — por um `BackgroundService` em tempo real, com uma checagem *lazy* de segurança nos acessos (robusta mesmo que o serviço hiberne no plano free do Render).

* **Privacy by Design (LGPD):** no módulo de Ação Solidária, a identidade do solicitante é separada da solicitação por meio de um `CodigoPublico` pseudonimizado. A identidade real só pode ser revelada após `Correspondencia.AutorizarRevelacaoIdentidade()` explícito — **nunca** automaticamente.

* **Segurança e hardening:** autenticação por JWT assinado, autorização por *role* (o painel administrativo é restrito a `Admin`), *rate limiting* em endpoints sensíveis (login, denúncia), Swagger exposto apenas em Development e upload de imagem validado por *magic bytes* (não só pela extensão). O escalonamento de privilégio foi analisado e fechado — ver [`Analise-de-Seguranca.md`](Documentos/Analise-de-Seguranca.md).

* **Autenticação Institucional:** o acesso é restrito ao domínio `@unifesp.br`, validado no login via Google OAuth antes da emissão do JWT.

* **Banco serverless (Neon):** escolhido no lugar de uma instância sempre ligada por oferecer *scale-to-zero* (economia quando ocioso) e *branching* de banco por squad (ambientes de teste isolados — usados pelos testes de integração). Usa-se sempre a connection string **com pooling** e **SSL**.

---

## 📁 Estrutura do Repositório

```
UniHub/
├── Backend/                 # API .NET 10 (ASP.NET Core)
│   ├── src/
│   │   ├── UniHub.Domain/           # Entidades
│   │   ├── UniHub.Application/       # Serviços, Interfaces, DTOs
│   │   ├── UniHub.Infrastructure/    # EF Core, repositórios, Cloudinary
│   │   └── Unihub.API/               # Controllers (inclui painel admin), BackgroundServices, Hubs (SignalR)
│   ├── Migrations/
│   ├── Dockerfile                   # Imagem de produção (deploy no Render)
│   └── Program.cs
├── Frontend/                # SPA React + Vite
├── Tests/                   # Suíte de testes (xUnit): unidade, integração, concorrência
├── Documentos/              # Diagramas, identidade visual, planos e documentos de engenharia
├── AGENTS.md                # Guia de convenções para devs e IAs (ler antes de codar)
├── DEPLOY.md                # Runbook de deploy (Render + Neon)
└── render.yaml              # Blueprint do serviço no Render
```

---

## ⚙ Como rodar o projeto localmente

### Pré-requisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Node.js 20+](https://nodejs.org/) e npm (o Vite 8 exige Node 20 ou superior)
* Uma connection string de um banco **PostgreSQL** (recomendado: criar um projeto gratuito no [Neon](https://neon.tech))

### 1. Clonar o repositório
```bash
git clone https://github.com/UniHub-University/UniHub.git
cd UniHub
```

### 2. Backend
Os segredos **nunca** ficam versionados — configure-os via `dotnet user-secrets` (localmente) ou variáveis de ambiente (produção).

```bash
cd Backend

# Configure os segredos locais
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sua-connection-string-do-neon-com-pooling-e-ssl>"
dotnet user-secrets set "JwtSettings:SecureKey" "<chave-com-no-minimo-32-caracteres>"
dotnet user-secrets set "Cloudinary:CloudName" "<cloud-name>"
dotnet user-secrets set "Cloudinary:ApiKey" "<api-key>"
dotnet user-secrets set "Cloudinary:ApiSecret" "<api-secret>"

# Restaure e aplique as migrations
dotnet restore
dotnet ef database update

# Rode a API
dotnet run
```
Em ambiente **Development**, o Swagger fica disponível na URL informada pelo `dotnet run` (ex.: `http://localhost:5000/swagger`).

### 3. Frontend
```bash
cd Frontend
cp .env.example .env.local   # preencha VITE_API_BASE_URL e VITE_GOOGLE_CLIENT_ID
npm install
npm run dev
```

### 4. Testes
```bash
# unidade (rápido, sem banco)
dotnet test --filter "Categoria=Unit"

# integração/concorrência (exige a env var UNIHUB_TEST_DB apontando para
# uma branch de teste do Neon; cada teste roda em transação + rollback)
dotnet test --filter "Categoria=Integracao"
```

---

## 🚢 Deploy

O deploy da API é feito no **Render**, a partir do `Backend/Dockerfile`, mantendo o banco no **Neon**. O passo a passo completo (variáveis de ambiente, migrations em produção, cold start e testes pós-deploy) está documentado em **[`DEPLOY.md`](DEPLOY.md)**.

---

## ▪ Documentação de Engenharia

Antes de escrever código ou abrir um PR, leia o **[`AGENTS.md`](AGENTS.md)** — fonte de verdade das convenções (claim `"sub"`, concorrência de estoque, LGPD, fluxo de migrations, checklist de review e padrões do frontend). Vale tanto para desenvolvedores quanto para assistentes de IA.

Outros documentos de engenharia do projeto:
- **[Fluxo de Desenvolvimento](Documentos/Fluxo_desenvolvimento.md)** — do planejamento de uma tarefa ao deploy (Scrum, BRV → Issue → PR → review).
- **[Estratégia de Testes](Documentos/Estrategia-de-Testes.md)** — o que e por que testamos (domínio, service, integração e concorrência).
- **[Análise de Segurança](Documentos/Analise-de-Seguranca.md)** — hardening, LGPD e os vetores de segurança verificados.
- **[Melhorias Futuras](Documentos/Melhorias-Futuras.md)** — itens de evolução priorizáveis.

---

## 👥 Equipe

Projeto desenvolvido em ciclos ágeis (Scrum) com fatiamento vertical por domínio de negócio. O time entregou os **3 módulos de ponta a ponta** (Alimentação, Bazar e Ação Solidária), o fluxo de Comprar/Reservar com expiração automática de reservas, os perfis de Vendedor e Voluntário, o **módulo de Moderação com painel administrativo**, o **painel do vendedor em tempo real (SignalR)**, a suíte de **testes automatizados** (unidade, integração e concorrência), um **hardening de segurança** (análise documentada, rate limiting, Swagger restrito a dev) e o **deploy contínuo** no Render + Neon.

---

## 📄 Licença

Distribuído sob a **Apache License 2.0** — veja [`LICENSE`](LICENSE) para os termos
e [`NOTICE`](NOTICE) para a atribuição aos autores.
