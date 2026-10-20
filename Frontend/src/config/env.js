// Centraliza a leitura das variáveis de ambiente do Vite num único lugar.
// Assim, se o nome de uma variável mudar, só este arquivo precisa ser ajustado,
// e o resto do código importa valores já tratados.

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';
const googleClientId = import.meta.env.VITE_GOOGLE_CLIENT_ID ?? '';

// Remove uma eventual barra final para evitar URLs com "//" ao concatenar rotas.
export const API_BASE_URL = apiBaseUrl.replace(/\/+$/, '');
export const GOOGLE_CLIENT_ID = googleClientId;

// Aviso em desenvolvimento se algo essencial não foi configurado.
if (import.meta.env.DEV) {
  if (!API_BASE_URL) {
    console.warn('[UniHub] VITE_API_BASE_URL não definido. Configure o .env.local.');
  }
  if (!GOOGLE_CLIENT_ID) {
    console.warn('[UniHub] VITE_GOOGLE_CLIENT_ID não definido. Configure o .env.local.');
  }
}