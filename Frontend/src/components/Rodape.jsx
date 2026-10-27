// Rodapé global das telas protegidas (BRV-148).
//
// Integrado no LayoutApp (ponto único), aparece em todas as telas após o login.
// Traz a marca "UniHub" em destaque e centralizada, os links de navegação
// (mesma lista do header, via navegacao.js) e o contato institucional por e-mail.
//
// NÃO é usado na tela de login pública (BoasVindas), que tem layout próprio.

import { Link } from 'react-router-dom';

import { LINKS_NAV } from './navegacao.js';
import './Rodape.css';

const EMAIL_CONTATO = 'time.unihub@gmail.com';

export default function Rodape() {
  const anoAtual = new Date().getFullYear();

  return (
    <footer className="rodape">
      <div className="rodape__conteudo">
        <p className="rodape__marca">UniHub</p>

        <nav className="rodape__nav" aria-label="Navegação do rodapé">
          {LINKS_NAV.map(({ para, rotulo }) => (
            <Link key={para} to={para} className="rodape__link">
              {rotulo}
            </Link>
          ))}
        </nav>

        <p className="rodape__contato">
          Fale com a gente:{' '}
          <a className="rodape__email" href={`mailto:${EMAIL_CONTATO}`}>
            {EMAIL_CONTATO}
          </a>
        </p>

        <p className="rodape__direitos">
          © {anoAtual} UniHub · Comunidade acadêmica da UNIFESP
        </p>
      </div>
    </footer>
  );
}
