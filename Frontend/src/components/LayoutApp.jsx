import { useRef, useState } from 'react';
import { NavLink, Outlet, useNavigate, useLocation } from 'react-router-dom';
import { FiUser, FiMenu, FiX, FiSearch } from 'react-icons/fi';
import { LuStore } from 'react-icons/lu';
import { useAuth } from '../auth/AuthContext.jsx';
import { PerfilVendedorProvider, usePerfilVendedor } from '../auth/PerfilVendedor.jsx';
import { LINKS_NAV } from './navegacao.js';
import Rodape from './Rodape.jsx';
import logoUniHub from '../assets/logo-unihub.png';
import './LayoutApp.css';

function Navegacao() {
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const { logout } = useAuth();
  const { ehVendedor } = usePerfilVendedor();
  const [menuRota, setMenuRota] = useState(null);
  const aberto = menuRota === pathname;
  const botaoMenu = useRef(null);
  const classe = ({ isActive }) => isActive ? 'layout__link layout__link--ativo' : 'layout__link';
  function fechar() { setMenuRota(null); }
  function teclado(e) {
    if (e.key === 'Escape' && aberto) { fechar(); botaoMenu.current?.focus(); }
  }
  return <header className="layout__header" onKeyDown={teclado}>
    <NavLink to="/home" className="layout__marca" onClick={fechar} aria-label="UniHub — início">
      <img className="layout__logo" src={logoUniHub} alt="" width={36} height={36}/><span className="layout__nome-app">UniHub</span>
    </NavLink>
    <nav id="navegacao-principal" className={`layout__nav ${aberto ? 'layout__nav--aberta' : ''}`} aria-label="Navegação principal">
      {LINKS_NAV.filter(({ para }) => para !== '/perfil' && para !== '/produtos').map(({ para, rotulo }) => <NavLink key={para} to={para} className={classe} onClick={fechar}>{rotulo}</NavLink>)}
    </nav>
    <div className="layout__usuario">
      {ehVendedor && (
        <NavLink to="/vendedor"
          className={({ isActive }) => `layout__painel-botao${isActive ? ' layout__painel-botao--ativo' : ''}`}
          aria-label="Painel do Vendedor" title="Painel do Vendedor" onClick={fechar}>
          <LuStore aria-hidden="true" size={21} />
          <span className="layout__texto-painel">Painel do Vendedor</span>
        </NavLink>
      )}
      <NavLink to="/produtos" className={classe} aria-label="Pesquisar produtos" title="Pesquisar produtos" onClick={fechar}>
        <FiSearch aria-hidden="true" size={22} />
      </NavLink>
      <NavLink to="/perfil" className={classe} aria-label="Meu perfil" title="Meu perfil" onClick={fechar}><FiUser aria-hidden="true" size={22}/></NavLink>
      <button type="button" className="layout__sair" onClick={() => { logout(); navigate('/', { replace: true }); }}>Sair</button>
      <button ref={botaoMenu} type="button" className="layout__menu" aria-label={aberto ? 'Fechar menu' : 'Abrir menu'} aria-expanded={aberto} aria-controls="navegacao-principal" onClick={() => setMenuRota(aberto ? null : pathname)}>{aberto ? <FiX/> : <FiMenu/>}</button>
    </div>
  </header>;
}
export default function LayoutApp() {
  return <PerfilVendedorProvider><div className="layout"><Navegacao/><main className="layout__conteudo"><Outlet/></main><Rodape/></div></PerfilVendedorProvider>;
}