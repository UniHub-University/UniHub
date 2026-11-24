// Camada de acesso à criação de denúncias (B1 — UI de denúncia).
//
// Isola a chamada ao POST /api/denuncias num único lugar, seguindo o padrão do
// projeto (as telas não montam URL nem usam fetch direto — passam pelo wrapper
// `api`). Molde: services/anuncios.js e services/moderacao.js.
//
// Contrato do endpoint (ver CriarDenunciaDto / DenunciasController no backend):
//  - vendedorId: GUID do vendedor denunciado. O backend traduz para o UsuarioId
//    dono (o UsuarioId não é exposto no front — LGPD). É o que as telas têm.
//  - produtoId:  GUID do produto (opcional). Enviado quando a denúncia parte do
//    detalhe de um anúncio; ausente quando parte do perfil do vendedor.
//  - motivo:     NOME do valor do enum DenunciaMotivo (string), ex.: "Fraude".
//                O backend faz Enum.TryParse por nome (ignoreCase).
//  - descricao:  justificativa, obrigatória, 10–500 caracteres (o backend valida).
//
// Requer usuário autenticado (o wrapper injeta o Bearer). Lança ApiError em
// falha: 400 (auto-denúncia / motivo inválido / nada informado), 404 (vendedor
// não encontrado), 401 (não autenticado).

import { api } from './api.js';

export function criarDenuncia({ vendedorId, produtoId, motivo, descricao}) {
    const corpo = {
        vendedorId,
        motivo,
        descricao,
    };
    // Só inclui produtoId quando existir (denúncia a partir do detalhe do produto)
    if (produtoId) {
        corpo.produtoId = produtoId;
    }

    return api.post('/api/denuncias', corpo);
}

// Lista as denúncias feitas pelo próprio usuário logado (tela "Denúncias feitas
// por mim"). GET /api/denuncias/minhas — o backend filtra pelo denunciante do
// token (sem IDOR). Retorna uma LISTA DIRETA (array), não paginada — diferente
// do painel admin (#119). Portanto o chamador usa o array como veio.
export function listarMinhasDenuncias(){
    return api.get('/api/denuncias/minhas');
}
