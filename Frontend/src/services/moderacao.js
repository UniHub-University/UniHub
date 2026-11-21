// Camada de acesso aos dados de moderação (BRV-232, painel do admin).
//
// Isola as chamadas do painel de moderação num único lugar, seguindo o padrão
// do projeto (as telas não montam URL nem usam fetch direto — passam pelo
// wrapper `api`). Veja services/vendedor.js como molde.
//
// IMPORTANTE — estado atual (B3 consome; B2 ainda não existe):
//  - As AÇÕES de usuário/produto (suspender/reativar/inativar) usam os endpoints
//    REAIS do AdminController, que já existem.
//  - As consultas/ações de DENÚNCIA (listar, alterar status) ficam atrás de um
//    MOCK local, porque o backend de denúncias (frente B2) ainda não existe.
//    Quando a B2 entregar GET /api/admin/denuncias e PATCH
//    /api/admin/denuncias/{id}/status, basta inverter USAR_MOCK para false
//    (ou remover o ramo do mock) — a página NÃO muda, pois consome só estas
//    funções.

import { api } from './api.js';

// Vire para false quando o backend da frente B2 estiver disponível.
const USAR_MOCK = false;

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
  if (USAR_MOCK) {
    // Clona para a UI não mutar o mock por referência.
    return DENUNCIAS_MOCK.map((d) => ({ ...d }));
  }
  const dados = await api.get('/api/admin/denuncias');
  return Array.isArray(dados) ? dados : [];
}

// Altera o status de uma denúncia (status: inteiro de DenunciaStatus).
// Real (B2): return api.patch(`/api/admin/denuncias/${id}/status`, { status });
export async function alterarStatusDenuncia(id, status) {
  if (USAR_MOCK) {
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
