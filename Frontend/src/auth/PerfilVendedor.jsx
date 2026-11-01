import { createContext, useContext, useEffect, useState } from 'react';
import { useAuth } from './AuthContext.jsx';
import { api } from '../services/api.js';

const Contexto = createContext(null);
export function PerfilVendedorProvider({ children }) {
  const { token } = useAuth();
  const [tentativa, setTentativa] = useState(0);
  const [carregando, setCarregando] = useState(true);
  const [perfil, setPerfil] = useState(null);
  const [erro, setErro] = useState('');
  // Verifica o perfil uma vez por sessão/token (e ao chamar recarregar).
  // O token muda em login/logout, o que já cobre a reverificação por sessão.
  useEffect(() => {
    const controlador = new AbortController();
    // Reset síncrono ao (re)verificar: volta a UI para "carregando" e limpa erro
    // anterior antes de refazer a busca em /api/perfil (sincronização com a API).
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setCarregando(true);
    setErro('');
    api.get('/api/perfil', { signal: controlador.signal })
      .then((dados) => {
        setPerfil(dados);
        setCarregando(false);
      })
      .catch((e) => {
        // Chamada cancelada (troca de token/recarregar ou desmontagem): ignora.
        if (e?.name === 'AbortError') return;
        setErro('Não foi possível verificar seu perfil de vendedor.');
        setCarregando(false);
      });
    return () => controlador.abort();
  }, [token, tentativa]);
  return <Contexto.Provider value={{ perfil, carregando,
    ehVendedor: perfil?.ehVendedor === true, erro,
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