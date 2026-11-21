// Rótulos amigáveis dos enums de denúncia (BRV-232).
//
// Molde: rotulosProduto.js. Os valores do backend chegam como inteiros
// (DenunciaStatus: 0..3; DenunciaMotivo: 0..5) porque a API não usa
// JsonStringEnumConverter. Mantemos também as chaves string como fallback
// defensivo, caso a serialização mude, e um fallback final para valor
// desconhecido (não quebra a tela).

const STATUS = {
  0: 'Aberta',
  1: 'Em análise',
  2: 'Resolvida',
  3: 'Rejeitada',
  Aberta: 'Aberta',
  EmAnalise: 'Em análise',
  Resolvida: 'Resolvida',
  Rejeitada: 'Rejeitada',
};

const MOTIVO = {
  0: 'Conteúdo ofensivo',
  1: 'Comportamento inadequado',
  2: 'Assédio',
  3: 'Fraude',
  4: 'Produto inadequado',
  5: 'Outro',
  ConteudoOfensivo: 'Conteúdo ofensivo',
  ComportamentoInadequado: 'Comportamento inadequado',
  Assedio: 'Assédio',
  Fraude: 'Fraude',
  ProdutoInadequado: 'Produto inadequado',
  Outro: 'Outro',
};

// Traduz o status (inteiro ou string) da denúncia para um rótulo amigável.
export function rotuloStatusDenuncia(status) {
  return STATUS[status] ?? String(status ?? '');
}

// Traduz o motivo (inteiro ou string) da denúncia para um rótulo amigável.
export function rotuloMotivoDenuncia(motivo) {
  return MOTIVO[motivo] ?? String(motivo ?? '');
}

// Opções de status para o seletor de "mudar status" na UI (valor inteiro + rótulo).
export const OPCOES_STATUS_DENUNCIA = [
  { valor: 0, rotulo: 'Aberta' },
  { valor: 1, rotulo: 'Em análise' },
  { valor: 2, rotulo: 'Resolvida' },
  { valor: 3, rotulo: 'Rejeitada' },
];

// Opções de motivo para o seletor ao CRIAR uma denúncia (B1).
// O `valor` é o NOME do enum DenunciaMotivo: o POST /api/denuncias recebe o
// motivo como string e faz Enum.TryParse por nome (ao contrário do GET do
// painel, que devolve inteiro). Os rótulos reaproveitam o mapa MOTIVO acima.
export const OPCOES_MOTIVO_DENUNCIA = [
  {valor: 'ConteudoOfensivo', rotulo: MOTIVO.ConteudoOfensivo},
  {valor: 'ComportamentoInadequado', rotulo: MOTIVO.ComportamentoInadequado},
  {valor: 'Assedio', rotulo: MOTIVO.Assedio},
  {valor: 'Fraude', rotulo: MOTIVO.Fraude},
  {valor: 'ProdutoInadequado', rotulo: MOTIVO.ProdutoInadequado},
  {valor: 'Outro', rotulo: MOTIVO.Outro},
];
