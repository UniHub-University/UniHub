# Plano: busca padronizada (Bazar e Alimentacao) e historico de pedidos (Perfil)

Frente 3 da Sprint 5 ("Frontend: Correcoes e Detalhes"), itens "conectar a busca
completa e os filtros aos anuncios" e "conectar o historico de pedidos ao perfil".
Os dois ja funcionam; este PR remove inconsistencias e duplicacao. So frontend.

## Escopo
1. **Busca igual no Bazar e na Alimentacao.** O Bazar tinha formulario com botao
   "Pesquisar" que levava a `/produtos`; a Alimentacao tinha so um filtro local por
   texto (sem preco, campus nem horario). Novo componente `BuscaAnuncios` (uma caixa
   + botao, `tipo` bazar ou lanche) usado nas duas abas; ambas encaminham para
   `/produtos?tipo=...&termo=...`. A Alimentacao perde o filtro local instantaneo.
2. **Rotulos de pedido num lugar so.** `services/rotulosPedido.js` (status e tipo)
   substitui as copias de `Perfil.jsx` e `HistoricoPedidos.jsx`.
3. **Chamada de pedidos via service.** `listarMeusPedidos` em `services/pedidos.js`,
   usada pelo Perfil e pelo Historico (antes, `api.get` direto nas telas).
4. **Erro visivel no resumo do Perfil.** Falha ao carregar o resumo mostrava "Voce
   ainda nao fez pedidos" (enganoso); agora mostra mensagem de erro propria.

## Cuidado com outras frentes
- Nos arquivos `Bazar.jsx` e `Alimentacao.jsx` so o trecho da busca muda; a grade de
  cards (onde a Frente 5 vai exibir a imagem) nao e tocada.
- A busca antiga saiu dos CSS das duas paginas e foi para `BuscaAnuncios.css`.

## Fora do escopo
- Backend, migrations e novos endpoints.
- Destaque de expiracao de reserva no Perfil (melhoria futura).

## Passos
1. `rotulosPedido.js` + `listarMeusPedidos` (service); Historico e Perfil usam os dois.
2. Componente `BuscaAnuncios` (+ CSS); Bazar e Alimentacao passam a usa-lo.

## Criterios
- `npm run lint` e `npm run build` limpos.
- Bazar e Alimentacao: caixa de pesquisa visualmente igual; pesquisar leva a
  `/produtos` com a categoria certa e o termo digitado.
- Perfil: resumo igual ao anterior; com a API fora do ar, aparece a mensagem de erro.
- Historico: lista, paginacao e cancelamento de reserva sem mudanca de comportamento.
