// Camada de acesso aos dados de moderação (BRV-232, painel do admin).
//
// Isola as chamadas do painel de moderação num único lugar, seguindo o padrão
// do projeto (as telas não montam URL nem usam fetch direto — passam pelo
// wrapper `api`). Veja services/vendedor.js como molde.
//
// Todas as chamadas do painel usam os endpoints reais do AdminController:
//  - listar denúncias: GET /api/admin/denuncias (#119)
//  - alterar status:   PATCH /api/admin/denuncias/{id}/status (#124)
//  - suspender/reativar usuário e inativar produto: PATCH existentes

import { api } from './api.js';

// ----------------------- Denúncias -----------------------

// Lista as denúncias para o painel de moderação.
// Real (B2): return api.get('/api/admin/denuncias');
export async function listarDenuncias() {
  const dados = await api.get('/api/admin/denuncias');
  return Array.isArray(dados?.items) ? dados.items : [];
}



// Altera o status de uma denúncia (status: inteiro de DenunciaStatus).
// Real (B2): return api.patch(`/api/admin/denuncias/${id}/status`, { status });
export async function alterarStatusDenuncia(id, status) {
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
