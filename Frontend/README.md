# UniHub — Frontend

SPA do UniHub em **React 19 + Vite**, que consome a API REST do backend. Cobre as
telas dos três módulos (Alimentação, Bazar, Ação Solidária), perfil, pedidos e os
painéis de vendedor.

> As convenções detalhadas do front (wrapper de API, env, enums, padrões de hooks)
> estão no [`AGENTS.md`](../AGENTS.md) §14 — leia antes de criar telas novas.

## Stack

- **React 19 + Vite 8** (JavaScript, não TypeScript) — **requer Node 20+**
  (o Vite 8 / rolldown usa APIs que não existem no Node 18).
- **Roteamento:** `react-router-dom` 7.
- **Login Google:** `@react-oauth/google`.
- **Ícones:** `react-icons`.
- **Autenticação:** Context API (`AuthContext`), sem Redux.

## Scripts

```bash
npm run dev       # servidor de desenvolvimento (Vite)
npm run build     # build de produção
npm run preview   # serve o build localmente
npm run lint      # ESLint
```

## Como rodar localmente

```bash
cd Frontend

# 1. Configurar as variáveis de ambiente (NÃO versionadas)
cp .env.example .env.local
#   edite o .env.local e preencha:
#     VITE_API_BASE_URL    -> URL da API (local: http://localhost:5206 | prod: Render)
#     VITE_GOOGLE_CLIENT_ID -> Client ID do OAuth 2.0 (Google Cloud Console)

# 2. Instalar e rodar
npm install
npm run dev
```

> ⚠️ Se o login Google falhar com *"Missing required parameter: client_id"*, é porque
> o `VITE_GOOGLE_CLIENT_ID` não está definido no `.env.local`. Peça o Client ID ao
> squad e **reinicie** o `npm run dev` depois de preencher (o Vite só lê o `.env` na
> inicialização).

## Estrutura de pastas (`src/`)

```
src/
├── auth/         ← AuthContext, RotaProtegida, authStorage, PerfilVendedor / SomenteVendedor
├── config/       ← leitura centralizada de env (env.js)
├── services/     ← wrappers da API (api.js + um por módulo: pedidos, alimentos, itensBazar, vendedor…)
├── pages/        ← telas (uma .jsx + .css por tela)
├── components/   ← compartilhados (cards, formulários, LayoutApp, Rodapé…)
├── hooks/        ← hooks reutilizáveis
├── assets/       ← imagens/logos
├── App.jsx       ← rotas
├── main.jsx      ← providers (GoogleOAuth, Router, Auth)
└── index.css     ← tema global
```

## Padrões essenciais (resumo — ver `AGENTS.md` §14)

- **Toda chamada à API passa pelo wrapper `services/api.js`** — nunca `fetch` direto.
  Ele injeta o JWT, prefixa a `VITE_API_BASE_URL`, trata 204, suporta `AbortController`
  e padroniza erros via `ApiError`. Cada módulo tem seu service fino (ex.: `pedidos.js`).
- **Env só via `config/env.js`** — nunca ler `import.meta.env` espalhado. Variáveis
  do Vite exigem o prefixo `VITE_`.
- **Autenticação via `useAuth()`**; rotas protegidas por `<RotaProtegida>`; rotas só de
  vendedor por `<SomenteVendedor>`.
- **Enums vêm como número** do backend (`ProdutoStatus` 0/1/2, `PedidoStatus` 0..4,
  `TipoPedido` 0/1) — mapear o inteiro para o rótulo.
- **`setState` em `useEffect`:** não chamar de forma síncrona no corpo do effect
  (dispara `react-hooks/set-state-in-effect`); usar função assíncrona interna com flag
  de cancelamento.
- **Upload de imagem:** `POST /api/upload/imagem?pasta=<perfis|produtos|cardapios>`
  retorna `{ url }`; a tela salva a URL na entidade. Reusar o fluxo do `Perfil.jsx`.

## Identidade visual

Paleta verde `#2A4515` / tan `#B19563` / creme `#FEF6C7`; fontes Playfair Display +
Inter. Reusar o tema de `index.css`.

### Assets (logo)

A tela de boas-vindas (`src/pages/BoasVindas.jsx`) importa `logo-unihub.png` de
`src/assets/`. Salve a logo do UniHub (versão com o símbolo verde, da pasta
`Documentos/Identidade Visual`) com exatamente esse nome:

```
src/assets/logo-unihub.png
```

PNG com fundo transparente (recomendado) ou SVG — se usar SVG, ajuste a importação em
`BoasVindas.jsx` para `logo-unihub.svg`.

## Validação antes de abrir PR

- `npm run lint` limpo.
- `npm run build` limpo.
- Fluxo validado manualmente no `npm run dev`.
- Nenhum segredo versionado (`.env.local` é ignorado pelo `.gitignore`).
- Branch rebaseada sobre a base correta (ver o fluxo de branches em `AGENTS.md` §14).

## Deploy

O frontend é publicado na **Vercel** (SPA). A URL de produção precisa estar nas
origens de CORS da API (variável `Cors__AllowedOrigins` no Render). A `VITE_API_BASE_URL`
de produção aponta para a API no Render.
