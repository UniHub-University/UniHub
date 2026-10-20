// Estado global de autenticação do UniHub.
//
// Expõe: usuario, estaAutenticado, login(), logout().
// As telas consomem via o hook useAuth(). A persistência real fica no
// authStorage; aqui só mantemos o estado em memória sincronizado com ele.

import { createContext, useContext, useMemo, useState } from 'react';
import {
  obterToken,
  obterUsuario,
  removerToken,
  salvarToken,
  salvarUsuario,
} from './authStorage.js';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [token, setToken] = useState(() => obterToken());
  const [usuario, setUsuario] = useState(() => obterUsuario());

  // Chamado após o login bem-sucedido na API: guarda o JWT (e os dados
  // básicos do usuário, se vierem) tanto na memória quanto no storage.
  function login({ token: novoToken, usuario: dadosUsuario }) {
    salvarToken(novoToken);
    salvarUsuario(dadosUsuario);
    setToken(novoToken);
    setUsuario(dadosUsuario ?? null);
  }

  function logout() {
    removerToken();
    setToken(null);
    setUsuario(null);
  }

  const valor = useMemo(
    () => ({
      token,
      usuario,
      estaAutenticado: Boolean(token),
      login,
      logout,
    }),
    [token, usuario],
  );

  return <AuthContext.Provider value={valor}>{children}</AuthContext.Provider>;
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const contexto = useContext(AuthContext);
  if (!contexto) {
    throw new Error('useAuth deve ser usado dentro de <AuthProvider>.');
  }
  return contexto;
}