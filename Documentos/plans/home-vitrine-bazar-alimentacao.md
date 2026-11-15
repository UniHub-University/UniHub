# Plano: Home com uma linha de Bazar e uma de Alimentacao

Pedido do orientador: hoje a Home mostra so os anuncios mais recentes do Bazar.
A ideia e separar por categoria, com uma linha para itens do Bazar e outra para
alimentos, cada uma com acesso a sua pagina especifica. So frontend.

## Escopo
1. **Home em duas linhas.** "Bazar" (`/bazar`) e "Alimentacao" (`/alimentacao`),
   cada uma com os 6 anuncios mais recentes lado a lado (rolagem horizontal quando
   nao cabem) e um link "Ver todos" para a pagina da categoria.
2. **Estados independentes.** Cada linha tem seu proprio carregando/erro/vazio: se
   a API de alimentos falhar, o Bazar continua aparecendo (e vice-versa).
3. **Card de alimento reutilizavel.** O card que estava dentro de `Alimentacao.jsx`
   vira `components/CardAlimento.jsx` (+ CSS), no mesmo molde do `CardAnuncio` do
   Bazar. A Alimentacao e a Home usam o mesmo componente.

## Decisoes
- Os mais recentes vem de `GET /api/itens-bazar` e `GET /api/alimentos`, ordenados
  por `dataPublicacao` no cliente (`ordenarPorMaisRecente`); sem endpoint novo.
- O CSS do card saiu de `Alimentacao.css` e foi para `CardAlimento.css`
  (classes `cartao-alimento__*`). O contador de unidades do card ganhou classe
  propria (antes reaproveitava `alimentacao__status`, regra da mensagem de pagina).
- Cada linha e um painel com icone, titulo, subtitulo e botao "Ver todos", em tons
  da paleta (verde para o Bazar, marrom/dourado para a Alimentacao), com sombra.
- Banner da Home inalterado.

## Fora do escopo
- Backend, migrations e endpoints.
- Linha de Acao Solidaria na Home.

## Passos
1. `CardAlimento` (+ CSS); `Alimentacao.jsx` passa a usa-lo.
2. `Home.jsx`: componente `LinhaVitrine` usado para Bazar e Alimentacao.
3. `Home.css`: linha horizontal com rolagem e espacamento entre secoes.

## Criterios
- `npm run lint` e `npm run build` limpos.
- Home: duas secoes, cada uma com seus cards e link "Ver todos" certo.
- Erro/vazio em uma categoria nao afeta a outra.
- Alimentacao: lista igual a anterior (mesmo card, via componente).
- Celular: as linhas rolam na horizontal sem quebrar a pagina.
