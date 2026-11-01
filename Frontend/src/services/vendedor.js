// Camada de acesso aos dados do vendedor autenticado (BRV-164).
//
// Isola as rotas do módulo de vendedor num único lugar, seguindo o padrão do
// projeto (as telas não montam URL nem usam fetch direto — passam pelo wrapper api).

import { api } from './api.js';

// Lista os produtos do vendedor autenticado, em qualquer status
// (Disponível, Esgotado, Inativo). O vendedor é derivado do token no backend.
export async function listarMeusProdutos() {
  const dados = await api.get('/api/perfil/vendedor/produtos');
  return Array.isArray(dados) ? dados : [];
}
