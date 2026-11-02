import { Link } from 'react-router-dom';
import { FiPlusCircle, FiUser, FiPackage } from 'react-icons/fi';
import './PainelVendedor.css';

// Estrutura do painel. A listagem Meus Produtos será integrada pela equipe
// responsável por essa entrega; não duplicar a implementação aqui.
export default function PainelVendedor() {
  return (
    <section className="painel-vendedor" aria-labelledby="painel-vendedor-titulo">
      <header className="painel-vendedor__cabecalho">
        <div>
          <h1 id="painel-vendedor-titulo">Painel do Vendedor</h1>
          <p>Seu espaço para anunciar e acessar seu perfil.</p>
        </div>
      </header>
      <nav className="painel-vendedor__atalhos" aria-label="Ações do vendedor">
        <Link className="painel-vendedor__atalho" to="/vendedor/produtos">
          <FiPackage aria-hidden="true" size={28} />
          <span><strong>Meus produtos</strong><span>Visualize e gerencie seus anúncios.</span></span>
        </Link>
        <Link className="painel-vendedor__atalho" to="/vendedor/produtos/novo">
          <FiPlusCircle aria-hidden="true" size={28} />
          <span><strong>Adicionar produto</strong><span>Crie um anúncio no Bazar.</span></span>
        </Link>
        <Link className="painel-vendedor__atalho" to="/perfil">
          <FiUser aria-hidden="true" size={28} />
          <span><strong>Meu perfil</strong><span>Consulte e atualize seus dados pessoais.</span></span>
        </Link>
      </nav>
    </section>
  );
}