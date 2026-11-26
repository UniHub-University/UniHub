# Diagramas arquiteturais (Mermaid) — UniHub

> Fonte de verdade para regenerar os diagramas. Cole cada bloco em [https\://mermaid.live](https://mermaid.live) (ou no draw.io / Markdown do GitHub) e exporte como PNG. Atualizado para o estado final: módulo Moderação, tempo real (SignalR), hardening de segurança, deploy Vercel \+ Cloudflare.

---

## 1\. Diagrama de arquitetura (camadas e componentes)

```mermaid
flowchart LR
  %% ---------- Cliente ----------
  subgraph CLIENT["Client (Frontend) — Vercel"]
    USER["Usuário<br/>(Aluno, Vendedor,<br/>Voluntário, Admin)"]
    SPA["SPA<br/>React + Vite"]
    USER --- SPA
  end

  %% ---------- Nuvem / borda ----------
  subgraph CLOUD["Serviços de Nuvem / Borda"]
    CF["Cloudflare<br/>(borda / mitigação DDoS)"]
    GOOGLE["Google Identity<br/>OAuth"]
    CLOUDINARY["Cloudinary<br/>(Image Storage)"]
  end

  %% ---------- Backend ----------
  subgraph API["API / Backend (.NET 10) — Render"]
    direction TB

    subgraph PRES["Apresentação (API)"]
      CTRL["Controllers<br/>(Auth, Perfil, Produtos,<br/>Pedidos, Denúncias,<br/>Admin, Upload...)"]
      HUBS["Hubs — SignalR<br/>(PainelHub /hubs/painel)"]
    end

    subgraph APP["Aplicação (Application)"]
      SVC["Services por módulo<br/>(Seguranca, Vendas, Vendedor,<br/>Voluntario, Alimentacao)"]
      DTO["DTOs"]
      INOT["INotificadorPainel<br/>(interface tempo real)"]
    end

    subgraph INFRA["Infraestrutura (Infrastructure)"]
      EF["AppDbContext<br/>(Entity Framework Core)"]
      REPOS["Repositories por módulo"]
      CLOUDSVC["CloudinaryImageStorageService"]
      NOTIF["NotificadorPainelSignalR"]
    end

    DOMAIN["Camada de Domínio<br/>(entidades + regras de negócio)<br/>— ver diagrama 2"]
  end

  %% ---------- Persistência ----------
  DB[("Neon<br/>PostgreSQL")]

  %% ---------- Fluxos ----------
  SPA -->|"Login OAuth"| GOOGLE
  SPA -->|"HTTPS / REST (JSON)"| CF
  SPA -->|"WebSocket (SignalR)<br/>/hubs/painel"| CF
  CF --> CTRL
  CF --> HUBS
  SPA -->|"Upload multipart → URL"| CLOUDINARY

  CTRL -->|"chama"| SVC
  HUBS -. implementa .-> INOT
  SVC --> DTO
  SVC -->|"usa interfaces"| INFRA
  SVC -.->|"orquestra"| DOMAIN
  INOT -. implementado por .-> NOTIF

  EF -->|"Entity Framework Core"| DB
  CLOUDSVC --> CLOUDINARY
  NOTIF --> HUBS
  INFRA --> DOMAIN
```

&nbsp;

---

## 2. Diagrama de domínio (entidades e relacionamentos)

> Entidades e relacionamentos do núcleo do domínio. Identificadores em MAIÚSCULAS
> (estilo ER); o nome legível de cada entidade aparece na primeira linha da caixa.

```mermaid
erDiagram
  USUARIO ||--o| VENDEDOR : "é (0..1)"
  USUARIO ||--o| VOLUNTARIO : "é (0..1)"

  VENDEDOR ||--o{ PRODUTO : publica
  VENDEDOR ||--o{ LOCALVENDA : tem
  VENDEDOR ||--o{ HORARIOVENDA : tem

  PRODUTO ||--o| ALIMENTO : "TPH"
  PRODUTO ||--o| ITEMBAZAR : "TPH"
  ALIMENTO ||--o{ RESTRICAOALIMENTAR : tem

  USUARIO ||--o{ PEDIDO : "compra (comprador)"
  PEDIDO ||--|{ ITEMPEDIDO : contém
  PRODUTO ||--o{ ITEMPEDIDO : "referenciado em"

  USUARIO ||--o{ SOLICITACAOAPOIO : "solicita (solicitante)"
  SOLICITACAOAPOIO ||--o{ CORRESPONDENCIA : gera
  VOLUNTARIO ||--o{ CORRESPONDENCIA : atende
  SOLICITACAOAPOIO ||--o{ DOACAO : recebe
  VOLUNTARIO ||--o{ DOACAO : faz

  USUARIO ||--o{ DENUNCIA : "faz (denunciante)"
  USUARIO ||--o{ DENUNCIA : "alvo (denunciado)"
  PRODUTO ||--o{ DENUNCIA : "alvo opcional (0..1)"

  USUARIO {
    string nome "Usuário"
    guid Id
    string NomeCompleto
    string EmailInstitucional
    enum Role "Aluno=0, Admin=2"
    bool IsActive
  }
  VENDEDOR {
    string nome "Vendedor"
    guid Id
    guid UsuarioId
    string ChavePix
  }
  VOLUNTARIO {
    string nome "Voluntário"
    guid Id
    guid UsuarioId
    list AreasDeAtuacao
    bool Disponivel
  }
  PRODUTO {
    string nome "Produto (abstrato, TPH)"
    guid Id
    guid VendedorId
    int QuantidadeDisponivel
    enum ProdutoStatus
    string Discriminator
  }
  ALIMENTO {
    string nome "Alimento (Produto)"
  }
  ITEMBAZAR {
    string nome "ItemBazar (Produto)"
  }
  RESTRICAOALIMENTAR {
    string nome "RestriçãoAlimentar"
  }
  LOCALVENDA {
    string nome "LocalVenda"
  }
  HORARIOVENDA {
    string nome "HorárioVenda"
  }
  PEDIDO {
    string nome "Pedido"
    guid Id
    guid CompradorId
    enum PedidoStatus
    enum TipoPedido "Compra/Reserva"
    datetime ExpiraEm
  }
  ITEMPEDIDO {
    string nome "ItemPedido"
    guid ProdutoId
    int Quantidade
    decimal PrecoUnitarioNaCompra "preço congelado na compra"
  }
  SOLICITACAOAPOIO {
    string nome "SolicitaçãoApoio"
    guid Id
    guid SolicitanteId
    string CodigoPublico "pseudonimizado"
    enum StatusSolicitacao
  }
  CORRESPONDENCIA {
    string nome "Correspondência"
    guid Id
    guid SolicitacaoId
    guid VoluntarioId
    enum StatusCorrespondencia
    bool IdentidadeRevelada
  }
  DOACAO {
    string nome "Doação"
    guid Id
    guid SolicitacaoId
    guid VoluntarioId
    enum StatusDoacao
  }
  DENUNCIA {
    string nome "Denúncia"
    guid Id
    guid DenuncianteId
    guid DenunciadoId
    guid ProdutoId "opcional"
    enum DenunciaMotivo
    enum DenunciaStatus
  }
```