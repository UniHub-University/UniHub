// Utilitários de apresentação dos anúncios (BRV-148).
//
// Compartilhados entre a Home (pré-visualização dos mais recentes) e o Bazar
// (feed completo), para a regra de ordenação ficar num lugar só.

// Ordena por data de publicação (mais recente primeiro). Itens sem data válida
// vão para o fim, preservando uma ordem estável entre eles. Não muta a lista
// original (retorna uma cópia ordenada).
export function ordenarPorMaisRecente(itens) {
  return [...itens].sort((a, b) => {
    const dataA = new Date(a?.dataPublicacao ?? 0).getTime() || 0;
    const dataB = new Date(b?.dataPublicacao ?? 0).getTime() || 0;
    return dataB - dataA;
  });
}
