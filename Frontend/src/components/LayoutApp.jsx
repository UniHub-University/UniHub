// Layout comum das telas protegidas (BRV-147).
//
// Renderiza um header fixo com a logo, a navegação entre as áreas iniciais da
// plataforma e o botão "Sair". O conteúdo de cada rota filha entra no <Outlet />.
//
// As áreas que ainda não têm tela/endpoint (Alimentação, Ação Solidária, Perfil)
// apontam para rotas de placeholder "em breve" — a navegação já fica pronta para
// quando o squad desenvolver cada uma, sem quebrar o fluxo atual.

import { NavLink, Outlet, useNavigate } from 'react-router-dom';

import { useAuth } from '../auth/AuthContext.jsx';
import logoUniHub from '../assets/logo-unihub.png';
import './LayoutApp.css';

const LINKS_NAV = [
  { para: '/home', rotulo: 'Bazar' },
  { para: '/alimentacao', rotulo: 'Alimentação' },
  { para: '/solidariedade', rotulo: 'Ação Solidária' },
  { para: '/perfil', rotulo: 'Perfil' },
];

export default function LayoutApp() {
  const navigate = useNavigate();
  const { usuario, logout } = useAuth();

  function aoSair() {
    logout();
    navigate('/', { replace: true });
  }

  const nomeUsuario = usuario?.nome?.trim();

  return (
    <div className="layout">
      <header className="layout__header">
        <div className="layout__marca">
          <img
            className="layout__logo"
            src={logoUniHub}
            alt="UniHub"
            width={36}
            height={36}
          />
          <span className="layout__nome-app">UniHub</span>
        </div>

        <nav className="layout__nav" aria-label="Navegação principal">
          {LINKS_NAV.map(({ para, rotulo }) => (
            <NavLink
              key={para}
              to={para}
              className={({ isActive }) =>
                isActive ? 'layout__link layout__link--ativo' : 'layout__link'
              }
            >
              {rotulo}
            </NavLink>
          ))}
        </nav>

        <div className="layout__usuario">
          {nomeUsuario && (
            <span className="layout__saudacao" title={nomeUsuario}>
              Olá, {nomeUsuario.split(' ')[0]}
            </span>
          )}
          <button type="button" className="layout__sair" onClick={aoSair}>
            Sair
          </button>
        </div>
      </header>

      <main className="layout__conteudo">
        <Outlet />
      </main>
    </div>
  );
}
