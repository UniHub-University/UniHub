// Hook que atualiza o título da aba do navegador (document.title) conforme a
// página atual. Como o UniHub é uma SPA, o <title> do index.html é fixo; este
// hook é o que dá a cada tela um título próprio.
//
// Padrão de título: "Nome da Página • UniHub" (convenção comum em web apps).
// A tela pública de boas-vindas usa o título de marca completo.

import { useEffect } from 'react';

const SUFIXO = 'UniHub';
const TITULO_MARCA = 'UniHub - Troca e conexão para estudantes';

export function useTituloPagina(titulo) {
  useEffect(() => {
    document.title = titulo ? `${titulo} • ${SUFIXO}` : TITULO_MARCA;
  }, [titulo]);
}