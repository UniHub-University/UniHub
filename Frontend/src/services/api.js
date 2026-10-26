// Wrapper fino sobre fetch para falar com a API UniHub.
//
// Responsabilidades:
//  - prefixar todas as rotas com a VITE_API_BASE_URL;
//  - injetar automaticamente o header Authorization: Bearer <jwt> quando houver;
//  - desserializar JSON e padronizar o tratamento de erro.
//
// Assim, as telas chamam api.post('/api/auth/google-login', {...}) sem repetir
// baseURL, headers nem parsing em cada lugar.

import { API_BASE_URL } from '../config/env.js';
import { obterToken } from '../auth/authStorage.js';

// Erro customizado que carrega o status HTTP e o corpo da resposta, para a UI
// decidir a mensagem (ex.: 401 -> "use seu e-mail @unifesp.br").
export class ApiError extends Error {
  constructor(message, status, body) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
    this.body = body;
  }
}

async function request(caminho, { method = 'GET', body, autenticado = true } = {}) {
  const enviandoFormulario = body instanceof FormData;
  const headers = {};

  if (!enviandoFormulario) {
    headers['Content-Type'] = 'application/json';
  }
  
  if (autenticado) {
    const token = obterToken();
    if (token) {
      headers.Authorization = `Bearer ${token}`;
    }
  }

  let resposta;
  try {
    resposta = await fetch(`${API_BASE_URL}${caminho}`, {
      method,
      headers,
      body:
        body === undefined
          ? undefined
          : enviandoFormulario
            ? body
            : JSON.stringify(body),
    });
  } catch {
    // Falha de rede / CORS / API fora do ar (cold start do Render, por ex.).
    throw new ApiError('Não foi possível conectar ao servidor. Tente novamente.', 0, null);
  }

  // 204 No Content não tem corpo para desserializar.
  const temCorpo = resposta.status !== 204;
  const dados = temCorpo ? await resposta.json().catch(() => null) : null;

  if (!resposta.ok) {
    // A API devolve mensagens em campos como "Erro" ou "mensagem".
    const mensagem =
      dados?.Erro || dados?.erro || dados?.mensagem || dados?.Mensagem || 'Erro ao processar a requisição.';
    throw new ApiError(mensagem, resposta.status, dados);
  }

  return dados;
}

export const api = {
  get: (caminho, opcoes) => request(caminho, { ...opcoes, method: 'GET' }),
  post: (caminho, body, opcoes) => request(caminho, { ...opcoes, method: 'POST', body }),
  put: (caminho, body, opcoes) => request(caminho, { ...opcoes, method: 'PUT', body }),
  patch: (caminho, body, opcoes) => request(caminho, { ...opcoes, method: 'PATCH', body }),
  delete: (caminho, opcoes) => request(caminho, { ...opcoes, method: 'DELETE' }),
};