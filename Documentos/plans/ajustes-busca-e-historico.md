# Plano: busca padronizada (Bazar e Alimentacao) e erro no resumo do Perfil

Frente 3 da Sprint 5 ("Frontend: Correcoes e Detalhes"), itens "conectar a busca
completa e os filtros aos anuncios" e "conectar o historico de pedidos ao perfil".
Os dois ja funcionam; este PR remove duas inconsistencias. So frontend.

## Escopo
1. **Busca igual no Bazar e na Alimentacao.** O Bazar tinha formulario com botao
   "Pesquisar" que levava a `/produtos`; a Alimentacao tinha so um filtro local por
   texto (sem preco, campus nem horario). Novo componente `BuscaAnuncios` (uma caixa
   + botao, `tipo` bazar ou lanche) usado nas duas abas; ambas encaminham para
   `/produtos?tipo=...&termo=...`. A Alimentacao perde o filtro local instantaneo.
2. **Erro visivel no resumo do Perfil.** Falha ao carregar "Ultimas compras" mostrava
   "Voce ainda nao fez pedidos" (enganoso); agora mostra mensagem de erro propria.

## Cuidado com outras frentes
- Nos arquivos `Bazar.jsx` e `Alimentacao.jsx` so o trecho da busca muda; a grade de
  cards (onde a Frente 5 vai exibir a imagem) nao e tocada.
- A busca antiga saiu dos CSS das duas paginas e foi para `BuscaAnuncios.css`.
- No `Perfil.jsx` so o card de resumo de pedidos muda.

## Fora do escopo
- Backend, migrations e novos endpoints.
- Centralizar rotulos de status e service de pedidos (Perfil/Historico): outro PR.

## Passos
1. Componente `BuscaAnuncios` (+ CSS); Bazar e Alimentacao passam a usa-lo.
2. Estado de erro no card de resumo de pedidos do Perfil.

## Criterios
- `npm run lint` e `npm run build` limpos.
- Bazar e Alimentacao: caixa de pesquisa igual; pesquisar leva a `/produtos` com a
  categoria certa e o termo digitado; em tela estreita o botao fica abaixo da caixa.
- Perfil: resumo igual ao anterior; com a API de pedidos fora do ar, aparece a
  mensagem de erro em vez de "ainda nao fez pedidos".
