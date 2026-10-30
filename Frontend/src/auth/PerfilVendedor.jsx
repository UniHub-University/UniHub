import { createContext, useContext, useEffect, useState } from 'react';
import { useLocation } from 'react-router-dom';
import { useAuth } from './AuthContext.jsx';
import { api } from '../services/api.js';

const Contexto = createContext(null);
export function PerfilVendedorProvider({ children }) {
  const { token } = useAuth();
  const { pathname } = useLocation();
  const [tentativa, setTentativa] = useState(0);
  const [resultado, setResultado] = useState(null);
  useEffect(() => {
    let ativo = true;
    api.get('/api/perfil').then((perfil) => {
      if (ativo) setResultado({ token, pathname, tentativa, perfil });
    }).catch(() => {
      if (ativo) setResultado({ token, pathname, tentativa, erro: 'Não foi possível verificar seu perfil de vendedor.' });
    });
    return () => { ativo = false; };
  }, [token, pathname, tentativa]);
  const atual = resultado?.token === token && resultado?.pathname === pathname && resultado?.tentativa === tentativa;
  const perfil = atual ? resultado.perfil : null;
  return <Contexto.Provider value={{ perfil, carregando: !atual,
    ehVendedor: perfil?.ehVendedor === true, erro: atual ? resultado.erro : '',
    recarregar: () => setTentativa((n) => n + 1) }}>{children}</Contexto.Provider>;
}
// eslint-disable-next-line react-refresh/only-export-components
export function usePerfilVendedor() { return useContext(Contexto); }
export function SomenteVendedor({ children }) {
  const { carregando, erro, ehVendedor, recarregar } = usePerfilVendedor();
  if (carregando) return <p role="status">Verificando perfil de vendedor…</p>;
  if (erro) return <section role="alert"><p>{erro}</p><button onClick={recarregar}>Tentar novamente</button></section>;
  if (!ehVendedor) return <section role="alert"><h1>Acesso restrito</h1><p>Esta área está disponível apenas para quem possui perfil de vendedor.</p></section>;
  return children;
}