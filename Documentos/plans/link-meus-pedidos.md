\# Plano — Link "Meus pedidos" na navegação



\## Objetivo

Dar acesso, pelo menu e pelo rodapé, à tela de histórico de pedidos (`/pedidos`, BRV-89), que hoje só abre digitando o endereço.



\## Escopo

\- \*\*Inclui:\*\* adicionar o item "Meus pedidos" em `LINKS\\\_NAV` (`Frontend/src/components/navegacao.js`), lista compartilhada pelo header (`LayoutApp`) e pelo rodapé (`Rodape`).

\- \*\*NÃO inclui:\*\* mudança de rota, de tela, de backend ou de estilo; novos componentes.



\## Decisões de design

\- \*\*Entidades/Domain, Endpoints, Banco:\*\* nenhuma alteração (somente frontend).

\- \*\*Impacto em concorrência/LGPD:\*\* nenhum.

\- \*\*Trade-offs:\*\* o item entra na lista única já existente, para header e rodapé não divergirem. Posição logo após "Bazar", por ser uma ação ligada às compras.



\## Passos de implementação

1\. Adicionar `{ para: '/pedidos', rotulo: 'Meus pedidos' }` em `LINKS\\\_NAV`.

2\. Rodar `npm run build` no Frontend e conferir o menu e o rodapé no navegador.



\## Critérios de aceite

\- \[ ] "Meus pedidos" aparece no menu e no rodapé

\- \[ ] O link abre `/pedidos` e destaca o item ativo

\- \[ ] `npm run build` limpo

\- \[ ] Menu continua legível em tela estreita



\## Pendências / dúvidas em aberto

\- O arquivo é do squad de navegação (BRV-147/148): pedir revisão a quem o criou.

