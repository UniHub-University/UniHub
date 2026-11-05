// Tela de entrada do módulo de Ação Solidária.
//
// Direciona o usuário para os dois lados do fluxo:
//  - Solicitante: pedir apoio e acompanhar suas solicitações (Lucas).
//  - Voluntário: oferecer ajuda, ver sugestões, aceitar, doar (Gabriel).
//
// Tela compartilhada (base da infraestrutura). Os cards levam às rotas que cada
// dupla vai desenvolver. Visual no padrão de cards de atalho (igual PainelVendedor).

import { Link } from 'react-router-dom';
import { FaHandHoldingHeart, FaHandsHelping } from 'react-icons/fa';
import './SolidariedadeHub.css';

export default function SolidariedadeHub() {
  return (
    <section className="solidariedade-hub" aria-labelledby="solidariedade-hub-titulo">
      <header className="solidariedade-hub__cabecalho">
        <h1 id="solidariedade-hub-titulo">Ação Solidária</h1>
        <p>
          Conecte-se à comunidade: peça apoio quando precisar ou ajude quem precisa.
          As solicitações são anônimas — a identidade só é revelada com autorização.
        </p>
      </header>

      <nav className="solidariedade-hub__atalhos" aria-label="Áreas da Ação Solidária">
        <Link className="solidariedade-hub__atalho" to="/solidariedade/solicitar">
          <FaHandHoldingHeart aria-hidden="true" size={28} />
          <span>
            <strong>Preciso de apoio</strong>
            <span>Crie uma solicitação e acompanhe o andamento.</span>
          </span>
        </Link>

        <Link className="solidariedade-hub__atalho" to="/solidariedade/voluntario">
          <FaHandsHelping aria-hidden="true" size={28} />
          <span>
            <strong>Quero ajudar</strong>
            <span>Torne-se voluntário, veja sugestões e faça doações.</span>
          </span>
        </Link>
      </nav>
    </section>
  );
}