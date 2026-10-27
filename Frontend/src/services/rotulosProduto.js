// Rótulos amigáveis dos enums de produto (BRV-148).
//
// Fonte única dos mapas que antes estavam duplicados em Home.jsx e
// DetalheBazar.jsx. Os valores do backend chegam como inteiros
// (ProdutoStatus: 0/1/2; CondicaoItem: 0/1/2) porque a API não usa
// JsonStringEnumConverter. Mantemos também as chaves string como fallback
// defensivo, caso a serialização mude, e um fallback final para valor
// desconhecido (não quebra a tela).

const STATUS = {
  0: 'Disponível',
  1: 'Esgotado',
  2: 'Inativo',
  Disponivel: 'Disponível',
  Esgotado: 'Esgotado',
  Inativo: 'Inativo',
};

const CONDICAO = {
  0: 'Novo',
  1: 'Seminovo',
  2: 'Usado',
  Novo: 'Novo',
  Seminovo: 'Seminovo',
  Usado: 'Usado',
};

// Traduz o status (inteiro ou string) do produto para um rótulo amigável.
export function rotuloStatus(status) {
  return STATUS[status] ?? String(status ?? '');
}

// Traduz a condição (inteiro ou string) do item para um rótulo amigável.
// Retorna null quando a condição não é informada, para a UI decidir se exibe.
export function rotuloCondicao(condicao) {
  if (condicao === null || condicao === undefined) return null;
  return CONDICAO[condicao] ?? null;
}
