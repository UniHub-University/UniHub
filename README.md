# UniHub 🎓

O **UniHub** é um ecossistema digital desenvolvido para centralizar e otimizar as interações socioeconômicas no campus universitário da UNIFESP. O projeto combate a fragmentação de informações (hoje espalhadas em grupos de WhatsApp) através de uma plataforma unificada, de acesso restrito à comunidade acadêmica (`@unifesp.br`), organizada em três pilares: **comércio de alimentos**, **revenda de materiais acadêmicos** e **impacto social**.

> Projeto desenvolvido na disciplina de **Engenharia de Software**, com metodologia Scrum (sprints de 2 semanas) por uma equipe de ~7-8 integrantes.

---

## → Módulos do Sistema

* 🍔 **Alimentação:** painel de pedidos em tempo real — vendedores gerenciam o estoque de lanches e alunos reservam, evitando filas e incertezas no intervalo. É o módulo que concentra o maior desafio técnico (concorrência de estoque).
* ▪ **Bazar Acadêmico:** marketplace de compra e venda de materiais universitários entre alunos (livros, calculadoras, equipamentos), reaproveitando a mesma modelagem de produto/estoque.
* 🤝 **Ação Solidária:** conecta alunos em situação de vulnerabilidade a voluntários, com **pseudonimização rigorosa** da identidade do solicitante (privacy by design / LGPD).

---

## 🏛️ Arquitetura

O backend segue uma **Clean Architecture simplificada** (projeto único, camadas separadas por pastas), em que cada camada só conhece a camada abaixo:

```
API  →  Application  →  Infrastructure  →  Domain
```

* **Domain** — entidades puras, sem dependência de nada (regras e invariantes de negócio).
* **Application** — serviços (regra de negócio), interfaces de repositório e DTOs, organizados por módulo.
* **Infrastructure** — EF Core, `AppDbContext`, repositórios concretos e integrações externas (Cloudinary).
* **API** — Controllers, autenticação JWT e composição via injeção de dependência.

![Diagrama de Arquitetura do UniHub](Documentos/UniHub%20-%20Diagrama%20Arquitetural.png)

O diagrama acima mostra o fluxo completo: o **Frontend (React + Vite)** se autentica via **Google OAuth**, consome a **API REST (.NET)** enviando o **JWT**, e a API orquestra as entidades de domínio, persistindo em **PostgreSQL (Neon)** via Entity Framework Core e armazenando imagens no **Cloudinary**.

---

## 🛠️ Stack Tecnológica

| Camada | Tecnologia |
|---|---|
| **Frontend** | React + Vite (JavaScript, SPA) |
| **Backend** | C# / **.NET 10** (ASP.NET Core Web API) |
| **ORM** | Entity Framework Core 10 (provider `Npgsql.EntityFrameworkCore.PostgreSQL`) |
| **Banco de Dados** | **Neon** (PostgreSQL serverless) |
| **Autenticação** | Google OAuth (login institucional) + JWT emitido pela própria API |
| **Armazenamento de Imagens** | Cloudinary |
| **Documentação de API** | Swagger / Swashbuckle (ambiente de Development) |
| **Deploy** | Render (container Docker) — ver [`DEPLOY.md`](DEPLOY.md) |

---

## 🧠 Decisões de Engenharia e Diferenciais Técnicos

* **Controle de Concorrência Otimista (alta carga):** para suportar os picos do intervalo, o estoque **não** usa locks em memória (que não funcionam com múltiplas instâncias atrás de um load balancer). O decremento é um **UPDATE condicional atômico** no banco:
  ```sql
  UPDATE "Produtos"
  SET "QuantidadeDisponivel" = "QuantidadeDisponivel" - @quantidade
  WHERE "Id" = @id AND "QuantidadeDisponivel" >= @quantidade
  ```
  Se duas requisições disputam a última unidade, apenas uma afeta uma linha; a outra recebe zero linhas afetadas e sabe, de forma confiável, que perdeu a corrida.

* **Privacy by Design (LGPD):** no módulo de Ação Solidária, a identidade do solicitante é separada da solicitação por meio de um `CodigoPublico` pseudonimizado. A identidade real só pode ser revelada após `Correspondencia.AutorizarRevelacaoIdentidade()` explícito — **nunca** automaticamente.

* **Autenticação Institucional:** o acesso é restrito ao domínio `@unifesp.br`, validado no login via Google OAuth antes da emissão do JWT.

* **Banco serverless (Neon):** escolhido no lugar de uma instância sempre ligada por oferecer *scale-to-zero* (economia quando ocioso) e *branching* de banco por squad (ambientes de teste isolados). Usa-se sempre a connection string **com pooling** e **SSL**.

---

## 📁 Estrutura do Repositório

```
UniHub/
├── Backend/                 # API .NET 10 (ASP.NET Core)
│   ├── src/
│   │   ├── UniHub.Domain/           # Entidades
│   │   ├── UniHub.Application/       # Serviços, Interfaces, DTOs
│   │   ├── UniHub.Infrastructure/    # EF Core, repositórios, Cloudinary
│   │   └── Unihub.API/               # Controllers
│   ├── Migrations/
│   ├── Dockerfile                   # Imagem de produção (deploy no Render)
│   └── Program.cs
├── Frontend/                # SPA React + Vite
├── Documentos/              # Diagramas, identidade visual e documentos
├── DEPLOY.md                # Runbook de deploy (Render + Neon)
└── render.yaml              # Blueprint do serviço no Render
```

---

## ⚙️ Como rodar o projeto localmente

### Pré-requisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/download)
* [Node.js e npm](https://nodejs.org/)
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
Em ambiente **Development**, o Swagger fica disponível na URL informada pelo `dotnet run` (ex.: `http://localhost:5206/swagger`).

### 3. Frontend
```bash
cd Frontend
npm install
npm run dev
```

---

## 🚢 Deploy

O deploy da API é feito no **Render**, a partir do `Backend/Dockerfile`, mantendo o banco no **Neon**. O passo a passo completo (variáveis de ambiente, migrations em produção, cold start e testes pós-deploy) está documentado em **[`DEPLOY.md`](DEPLOY.md)**.

---

## 👥 Equipe

Projeto desenvolvido em ciclos ágeis (Scrum) com fatiamento vertical por domínio de negócio. Na Sprint 3, a divisão de squads foi: **Ação Solidária**, **Moderação e Gestão**, **Perfil do Vendedor (PIX)** e **Deploy**.
