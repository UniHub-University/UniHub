// Camada de acesso aos dados de moderação (BRV-232, painel do admin).
//
// Isola as chamadas do painel de moderação num único lugar, seguindo o padrão
// do projeto (as telas não montam URL nem usam fetch direto — passam pelo
// wrapper `api`). Veja services/vendedor.js como molde.
//
// IMPORTANTE — estado atual (B2 entregue parcialmente):
//  - As AÇÕES de usuário/produto (suspender/reativar/inativar) usam os endpoints
//    REAIS do AdminController, que já existem.
//  - LISTAR denúncias usa o endpoint REAL: GET /api/admin/denuncias (#119) já está
//    na main.
//  - ALTERAR STATUS ainda usa MOCK local: o PATCH /api/admin/denuncias/{id}/status
//    (outra metade do B2/#114) ainda NÃO existe. Quando entrar, vire
//    USAR_MOCK_STATUS para false (e remova o ramo do mock) — a página NÃO muda.



import { api } from './api.js';

// Listagem: o GET real já existe (#119) -> sem mock.
const USAR_MOCK_LISTAGEM = false;
// Alterar status: o PATCH ainda não existe (B2/#114 parcial) -> mock até chegar.
const USAR_MOCK_STATUS = true;

// Base do mock em memória. Nomes já RESOLVIDOS (a UI não resolve id→nome; isso
// é responsabilidade do backend/B2). Enums como INTEIRO (ver DenunciaStatus e
// DenunciaMotivo no domínio): status 0..3, motivo 0..5.
let DENUNCIAS_MOCK = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    denuncianteNome: 'João Silva',
    denunciadoId: 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    denunciadoNome: 'Maria Souza',
    produtoId: '99999999-9999-9999-9999-999999999999',
    produtoNome: 'Camiseta UNIFESP (usada)',
    motivo: 4, // ProdutoInadequado
    descricao: 'O produto anunciado não corresponde à descrição e parece falsificado.',
    status: 0, // Aberta
    criadaEm: '2026-11-10T14:30:00Z',
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    denuncianteNome: 'Ana Pereira',
    denunciadoId: 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    denunciadoNome: 'Carlos Lima',
    produtoId: null,
    produtoNome: null,
    motivo: 2, // Assedio
    descricao: 'Mensagens de assédio durante a negociação de um pedido.',
    status: 1, // EmAnalise
    criadaEm: '2026-11-09T09:15:00Z',
  },
  {
    id: '33333333-3333-3333-3333-333333333333',
    denuncianteNome: 'Beatriz Nunes',
    denunciadoId: 'cccccccc-cccc-cccc-cccc-cccccccccccc',
    denunciadoNome: 'Diego Rocha',
    produtoId: null,
    produtoNome: null,
    motivo: 0, // ConteudoOfensivo
    descricao: 'Conteúdo ofensivo no nome de exibição do perfil.',
    status: 2, // Resolvida
    criadaEm: '2026-11-05T18:45:00Z',
  },
  {
    id: '44444444-4444-4444-4444-444444444444',
    denuncianteNome: 'Rafael Costa',
    denunciadoId: 'dddddddd-dddd-dddd-dddd-dddddddddddd',
    denunciadoNome: 'Fernanda Dias',
    produtoId: '88888888-8888-8888-8888-888888888888',
    produtoNome: 'Apostila de Cálculo I',
    motivo: 3, // Fraude
    descricao: 'Cobrou o pagamento e não entregou o material combinado.',
    status: 3, // Rejeitada
    criadaEm: '2026-11-02T11:00:00Z',
  },
];

// ----------------------- Denúncias (MOCK por enquanto) -----------------------

// Lista as denúncias para o painel de moderação.
// Real (B2): return api.get('/api/admin/denuncias');
export async function listarDenuncias() {
  if (USAR_MOCK_LISTAGEM) {
    return DENUNCIAS_MOCK.map((d) => ({ ...d }));
  }
  // O backend (#119) devolve um PaginatedResultDto: a lista vem em `.items`.
  const dados = await api.get('/api/admin/denuncias');
  return Array.isArray(dados?.items) ? dados.items : [];
}


// Altera o status de uma denúncia (status: inteiro de DenunciaStatus).
// Real (B2): return api.patch(`/api/admin/denuncias/${id}/status`, { status });
export async function alterarStatusDenuncia(id, status) {
  if (USAR_MOCK_STATUS) {
    DENUNCIAS_MOCK = DENUNCIAS_MOCK.map((d) =>
      d.id === id ? { ...d, status } : d,
    );
    return;
  }
  return api.patch(`/api/admin/denuncias/${id}/status`, { status });
}

// ------------------ Ações de usuário/produto (endpoints REAIS) ----------------
// Todas retornam 204 sem corpo (o wrapper já trata). Lançam ApiError em falha.

// Inativa (exclusão lógica) o produto denunciado.
export async function inativarProduto(id) {
  return api.patch(`/api/admin/produtos/${id}/inativar`);
}

// Suspende a conta do usuário denunciado.
export async function suspenderUsuario(id) {
  return api.patch(`/api/admin/usuarios/${id}/suspender`);
}

// Reativa a conta do usuário denunciado.
export async function reativarUsuario(id) {
  return api.patch(`/api/admin/usuarios/${id}/reativar`);
}
