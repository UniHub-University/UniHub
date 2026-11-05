// Rótulos amigáveis dos enums do módulo de Ação Solidária.
//
// Fonte única compartilhada pelos dois lados (solicitante e voluntário), para
// não divergir. Os valores do backend chegam como inteiros (a API não usa
// JsonStringEnumConverter). Mantemos também as chaves string como fallback
// defensivo, caso a serialização mude, e um fallback final para desconhecido.
//
// Valores conferidos no backend (ordem dos enums):
//   CategoriaApoio:       AulasDeReforco=0, DoacaoDeRoupas=1, DoacaoDeAlimentos=2, MaterialAcademico=3, Outros=4
//   StatusSolicitacao:    Aberta=0, EmAndamento=1, Concluida=2, Cancelada=3
//   StatusCorrespondencia: Sugerida=0, AceitaPeloVoluntario=1, Concluida=2, Cancelada=3
//   StatusDoacao:         Pendente=0, Concluida=1, Cancelada=2

const CATEGORIA_APOIO = {
  0: 'Aulas de reforço',
  1: 'Doação de roupas',
  2: 'Doação de alimentos',
  3: 'Material acadêmico',
  4: 'Outros',
  AulasDeReforco: 'Aulas de reforço',
  DoacaoDeRoupas: 'Doação de roupas',
  DoacaoDeAlimentos: 'Doação de alimentos',
  MaterialAcademico: 'Material acadêmico',
  Outros: 'Outros',
};

const STATUS_SOLICITACAO = {
  0: 'Aberta',
  1: 'Em andamento',
  2: 'Concluída',
  3: 'Cancelada',
  Aberta: 'Aberta',
  EmAndamento: 'Em andamento',
  Concluida: 'Concluída',
  Cancelada: 'Cancelada',
};

const STATUS_CORRESPONDENCIA = {
  0: 'Sugerida',
  1: 'Aceita',
  2: 'Concluída',
  3: 'Cancelada',
  Sugerida: 'Sugerida',
  AceitaPeloVoluntario: 'Aceita',
  Concluida: 'Concluída',
  Cancelada: 'Cancelada',
};

const STATUS_DOACAO = {
  0: 'Pendente',
  1: 'Concluída',
  2: 'Cancelada',
  Pendente: 'Pendente',
  Concluida: 'Concluída',
  Cancelada: 'Cancelada',
};

// Lista de opções de categoria para selects/formulários (value = inteiro do enum).
export const OPCOES_CATEGORIA = [
  { valor: 0, rotulo: 'Aulas de reforço' },
  { valor: 1, rotulo: 'Doação de roupas' },
  { valor: 2, rotulo: 'Doação de alimentos' },
  { valor: 3, rotulo: 'Material acadêmico' },
  { valor: 4, rotulo: 'Outros' },
];

export function rotuloCategoria(categoria) {
  return CATEGORIA_APOIO[categoria] ?? String(categoria ?? '');
}

export function rotuloStatusSolicitacao(status) {
  return STATUS_SOLICITACAO[status] ?? String(status ?? '');
}

export function rotuloStatusCorrespondencia(status) {
  return STATUS_CORRESPONDENCIA[status] ?? String(status ?? '');
}

export function rotuloStatusDoacao(status) {
  return STATUS_DOACAO[status] ?? String(status ?? '');
}