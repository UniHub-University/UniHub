# Fluxo de Desenvolvimento — UniHub

Este documento descreve como o time organiza e desenvolve o projeto, desde o
planejamento de uma tarefa até sua publicação em produção. O fluxo foi
construído e refinado ao longo das sprints, com base em metodologia ágil e em
boas práticas de engenharia de software.

---

## 1. Visão geral

O desenvolvimento segue **Scrum**, com sprints de 2 semanas, e combina três
dimensões:

- **Processo** — como o time se organiza (planejamento, squads, revisão).
- **Arquitetura** — como o código é estruturado (camadas).
- **Entrega** — como o código chega ao usuário (PR, CI, deploy).

```
PLANEJAMENTO → DESENVOLVIMENTO → REVISÃO (PR) → INTEGRAÇÃO (CI) → DEPLOY
   (Scrum)      (camadas)         (code review)   (automação)      (Render)
```

---

## 2. Processo de equipe (Scrum adaptado)

- **Sprints de 2 semanas**, cada uma iniciada com um **planejamento** que prioriza
  user stories em um board (YouTrack), estimadas em *story points*.
- **Divisão em squads por domínio de negócio** (fatiamento vertical): cada squad é
  dono de um módulo de ponta a ponta (ex.: Ação Solidária, Alimentação), em vez de
  dividir por camada técnica. Isso reduz dependências e conflitos entre equipes.
- A alocação de pessoas por squad é feita **por criticidade da tarefa**, não por
  um tamanho fixo de equipe.
- **Papéis transversais:** um **Guardião de Migrations** centraliza as alterações de
  banco de dados (evitando inconsistências de schema entre squads), e uma frente de
  **Deploy/Qualidade** cuida da publicação e dos testes.

---

## 3. Arquitetura — como o código é organizado

O backend segue uma **Clean Architecture simplificada**, com quatro camadas em que
cada uma só conhece a camada abaixo:

```
┌────────────────┐────────────────────────────────────────┐
│  API           │Controllers, autenticação, injeção de   │
│                │dependência                             │
├────────────────┤────────────────────────────────────────┤
│  Application   │Services (regra de negócio), Interfaces,│
│                │DTOs                                    │
├────────────────┤────────────────────────────────────────┤
│  Infrastructure│EF Core, repositórios, Cloudinary       │
├────────────────┤────────────────────────────────────────┤
│  Domain        │Entidades e regras de negócio puras     │
└────────────────┘────────────────────────────────────────┘
```

Essa separação permite, por exemplo, trocar o banco de dados sem alterar uma linha
de regra de negócio.

---

## 4. Ciclo de vida de uma funcionalidade (passo a passo)

Quando uma user story é desenvolvida, ela percorre as camadas de dentro para fora:

| # | Etapa | Camada | O que acontece |
|---|---|---|---|
| 1 | **Branch** | — | Cria-se uma branch de feature (`feat/...`) a partir da `main` atualizada. |
| 2 | **Modelagem** | Domain | Cria/ajusta a entidade, com as regras de negócio como métodos nela. |
| 3 | **Contrato** | Application | Define a interface do repositório, os DTOs e o Service. |
| 4 | **Implementação** | Infrastructure | Implementa o repositório (EF Core). Mudou o schema? Aciona o Guardião de Migrations. |
| 5 | **Exposição** | API | Cria o Controller (usuário extraído do token JWT), registra o serviço na injeção de dependência. |
| 6 | **Validação local** | — | `dotnet build` e teste manual via Swagger. |
| 7 | **Pull Request** | — | Abre o PR; o revisor aplica o checklist de qualidade. |
| 8 | **Ajustes** | — | O autor resolve os comentários da revisão. |
| 9 | **Merge** | — | Aprovado, integra na `main`. Migrations são aplicadas no banco de forma controlada. |
| 10 | **Deploy** | — | A `main` é publicada automaticamente no Render e validada em produção. |

---

## 5. Fluxo de revisão (Pull Requests)

**Nenhuma alteração entra na `main` sem revisão.** Toda mudança passa por um Pull
Request, revisado antes do merge. *(Ao longo do projeto já foram integrados mais de
30 PRs seguindo este processo.)*

O revisor aplica um **checklist** focado nos riscos recorrentes do projeto:
- Extração correta do usuário autenticado (claim do token JWT).
- Injeção de dependência registrada (erros de DI só aparecem em tempo de execução).
- Ausência de vazamento de dados sensíveis (privacidade/LGPD).
- Autorização por posse (prevenção de acesso indevido — IDOR).
- Coordenação de migrations com o Guardião.
- Compilação limpa (`dotnet build`).

**Controle de versão:** branches de feature são mantidas atualizadas com a `main`
via *rebase*; a autoria dos commits é preservada (relevante para a avaliação
individual do time).

---

## 6. Integração contínua (CI)

Um workflow de **GitHub Actions** (`.github/workflows/ci.yml`) roda automaticamente
em cada PR e valida:
- Compilação do backend (.NET 10).
- Verificação de migrations pendentes (schema sincronizado com o modelo).
- Build da imagem Docker de produção.

Isso garante que código quebrado seja detectado **antes** do merge.

---

## 7. Fluxo técnico — o caminho de uma requisição

Em tempo de execução, uma requisição percorre o sistema assim:

```
  Frontend (React + Vite)
       │  1. Login via Google OAuth → recebe o JWT
       │  2. Chama a API REST enviando o token
       ▼
  API (.NET) — Controller
       │  3. Valida o JWT e extrai o usuário autenticado
       ▼
  Application — Service
       │  4. Aplica a regra de negócio
       ▼
  Infrastructure — Repositório (EF Core)
       │  5. Persiste/consulta
       ▼
  Neon (PostgreSQL)         Cloudinary (imagens)

  → Resposta volta como JSON, respeitando as regras de
    privacidade (DTOs não expõem dados sensíveis).
```

---

## 8. Deploy e ambiente

- A API é publicada no **Render** (container Docker), com **HTTPS automático**.
- O banco de dados é o **Neon** (PostgreSQL serverless).
- Segredos (string de conexão, chave JWT, credenciais do Cloudinary) ficam em
  **variáveis de ambiente**, nunca versionados no repositório.
- Mudanças integradas na `main` disparam a publicação; o endpoint `/health`
  permite monitorar a disponibilidade.

---

## 9. Decisões de engenharia documentadas

O time registra as decisões técnicas relevantes para dar rastreabilidade, por exemplo:
- **Neon em vez de RDS:** Postgres serverless com *scale-to-zero* e *branching* de
  banco por squad, reduzindo custo e fricção.
- **Controle de concorrência por UPDATE atômico** (em vez de bloqueio em memória ou
  `RowVersion`): garante consistência do estoque sob acessos simultâneos, mesmo com
  múltiplas instâncias da API.
- **Privacy by design** no módulo de Ação Solidária: identidade do solicitante
  pseudonimizada, revelada apenas mediante autorização explícita.

Essas convenções e lições estão consolidadas no arquivo `AGENTS.md`, que orienta
tanto novos desenvolvedores quanto assistentes de IA a seguirem o mesmo padrão.
