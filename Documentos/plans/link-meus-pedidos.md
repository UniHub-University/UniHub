# Plano — Acesso ao histórico de pedidos pelo Perfil

## Objetivo
Dar acesso, a partir da tela de Perfil, à tela de histórico de pedidos (`/pedidos`, BRV-89), que hoje só abre digitando o endereço.

## Escopo
- **Inclui:** botão "Meus pedidos" no fim do cartão do Perfil (`Perfil.jsx`), com um bloco de estilo no `Perfil.css` para o botão com aparência de link.
- **NÃO inclui:** mudança de rota, de backend ou da tela de histórico; novo item no menu de navegação.

## Decisões de design
- **Entidades/Domain, Endpoints, Banco:** nenhuma alteração (somente frontend).
- **Impacto em concorrência/LGPD:** nenhum.
- **Trade-offs considerados:**
  - A primeira versão adicionava "Meus pedidos" em `LINKS_NAV` (menu e rodapé). Foi revertida (commit de revert na branch) porque o produto prefere o acesso dentro do Perfil, sem uma aba própria no menu.
  - O botão reaproveita a classe `perfil__botao`; a classe `perfil__botao--link` só ajusta o que muda em um link (`display`, margem, alinhamento e sem sublinhado).
  - O botão fica fora do `<form>` para não disparar o salvamento do perfil.

## Passos de implementação
1. `Perfil.jsx`: importar `Link` do `react-router-dom` e adicionar o botão para `/pedidos` depois do formulário.
2. `Perfil.css`: estilo `.perfil__botao--link`.
3. `npm run build` no Frontend e conferência no navegador.

## Critérios de aceite
- [x] O Perfil mostra o botão "Meus pedidos" e ele abre `/pedidos`
- [x] O menu de navegação não ganha item novo
- [x] `npm run build` limpo

## Pendências / dúvidas em aberto
- `Perfil.jsx` é da BRV-115: pedir revisão a quem o criou.