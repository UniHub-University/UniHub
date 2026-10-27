// Lista única de navegação das telas protegidas (BRV-148).
//
// Extraída do LayoutApp para ser compartilhada entre o header (LayoutApp) e o
// rodapé global (Rodape), evitando divergência entre os dois conjuntos de links.
// "/home" é a vitrine (pré-visualização dos mais recentes); "/bazar" é o feed
// completo do Bazar; as demais áreas ainda são placeholders "em breve" até o
// squad desenvolver cada módulo.

export const LINKS_NAV = [
  { para: '/home', rotulo: 'Home' },
  { para: '/bazar', rotulo: 'Bazar' },
  { para: '/pedidos', rotulo: 'Meus pedidos' },
  { para: '/alimentacao', rotulo: 'Alimentação' },
  { para: '/solidariedade', rotulo: 'Ação Solidária' },
  { para: '/perfil', rotulo: 'Perfil' },
];