import { Link } from 'react-router-dom';
import { FiUser, FiPackage, FiClipboard, FiCoffee } from 'react-icons/fi';
import { FaBookOpen } from 'react-icons/fa6';
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
          <FaBookOpen aria-hidden="true" size={28} />
          <span><strong>Adicionar item</strong><span>Crie um anúncio para a aba de Bazar.</span></span>
        </Link>
        <Link className="painel-vendedor__atalho" to="/vendedor/alimentos/novo">
          <FiCoffee aria-hidden="true" size={28} />
          <span><strong>Adicionar alimento</strong><span>Crie um anúncio para a aba de Alimentação.</span></span>
        </Link>
        <Link className="painel-vendedor__atalho" to="/vendedor/pedidos">
          <FiClipboard aria-hidden="true" size={28} />
          <span><strong>Pedidos recebidos</strong><span>Veja quem comprou ou reservou e conclua os pedidos.</span></span>
        </Link>
        <Link className="painel-vendedor__atalho" to="/perfil">
          <FiUser aria-hidden="true" size={28} />
          <span><strong>Meu perfil</strong><span>Consulte e atualize seus dados pessoais.</span></span>
        </Link>
      </nav>
    </section>
  );
}