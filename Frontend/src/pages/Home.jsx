// Home — vitrine de entrada da plataforma.
//
// Banner reposicionado para representar os 3 módulos do UniHub (Bazar,
// Alimentação e Ação Solidária), seguido das linhas de vitrine de Bazar e
// Alimentação e de um convite institucional para a Ação Solidária.
//
// ⚠️ LGPD (AGENTS §8): a Ação Solidária NÃO entra como vitrine de solicitações
// (exporia pessoas em necessidade). É um bloco de CONVITE que leva ao módulo —
// sem fetch de solicitações, sem dados reais.
//
// Cada LinhaVitrine carrega de forma independente: se uma API falhar, a outra
// continua aparecendo (cada uma trata seu próprio carregando/erro/vazio).

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  FiShoppingBag,
  FiHeart,
  FiCoffee,
  FiArrowRight,
} from 'react-icons/fi';
import { FaHandHoldingHeart } from 'react-icons/fa';

import { listarItensBazar } from '../services/anuncios.js';
import { listarAlimentos } from '../services/alimentos.js';
import { ApiError } from '../services/api.js';
import { ordenarPorMaisRecente } from '../services/anunciosUtils.js';
import CardAnuncio from '../components/CardAnuncio.jsx';
import CardAlimento from '../components/CardAlimento.jsx';
import './Home.css';

// Quantidade de anúncios exibidos em cada linha da vitrine.
const QTD_POR_LINHA = 6;

// Uma linha da vitrine: título, link "Ver todos" e os cards mais recentes.
// Recebe a função de busca e o componente de card, então serve para qualquer
// categoria (hoje Bazar e Alimentação).
function LinhaVitrine({
  id,
  tom,
  icone,
  titulo,
  descricao,
  rotuloLink,
  destino,
  buscar,
  renderizarCard,
  mensagemVazia,
  mensagemErro,
}) {
  const [itens, setItens] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      setCarregando(true);
      setErro('');
      try {
        const dados = await buscar();
        if (ativo) {
          setItens(ordenarPorMaisRecente(dados).slice(0, QTD_POR_LINHA));
        }
      } catch (e) {
        if (!ativo) return;
        setErro(e instanceof ApiError ? e.message : mensagemErro);
      } finally {
        if (ativo) setCarregando(false);
      }
    }

    carregar();
    return () => {
      ativo = false;
    };
  }, [buscar, mensagemErro]);

  return (
    <section
      className={`home__secao home__secao--${tom}`}
      aria-labelledby={id}
    >
      <div className="home__secao-topo">
        <div className="home__secao-identidade">
          <span className="home__secao-icone" aria-hidden="true">
            {icone}
          </span>
          <div>
            <h2 className="home__secao-titulo" id={id}>
              {titulo}
            </h2>
            <p className="home__secao-descricao">{descricao}</p>
          </div>
        </div>
        <Link className="home__ver-todos" to={destino}>
          {rotuloLink} <span aria-hidden="true">→</span>
        </Link>
      </div>

      {carregando && <p className="home__status">Carregando anúncios…</p>}

      {!carregando && erro && (
        <p className="home__erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && !erro && itens.length === 0 && (
        <p className="home__status">{mensagemVazia}</p>
      )}

      {!carregando && !erro && itens.length > 0 && (
        <ul className="home__linha">{itens.map(renderizarCard)}</ul>
      )}
    </section>
  );
}

function cardBazar(item) {
  return <CardAnuncio key={item.id} item={item} />;
}

function cardAlimento(alimento) {
  return <CardAlimento key={alimento.id} alimento={alimento} />;
}

export default function Home() {
  return (
    <section className="home">
      {/* Banner da plataforma: fala dos 3 módulos (não só do Bazar). */}
      <header className="home__banner">
        <div className="home__banner-conteudo">
          <span className="home__banner-etiqueta">FEITO PELA COMUNIDADE, PARA A COMUNIDADE</span>
          <h1 className="home__banner-titulo">
            Seu campus.<br />Novas conexões.<br /><em>Mais possibilidades.</em>
          </h1>
          <p className="home__banner-descricao">
            Um só lugar para circular o que você não usa, compartilhar comida da
            comunidade e apoiar quem precisa. O UniHub reúne Bazar, Alimentação e
            Ação Solidária do seu campus.
          </p>
          <a className="home__banner-botao" href="#home-vitrine">
            Explorar o UniHub <span aria-hidden="true">↓</span>
          </a>
        </div>
        <div className="home__banner-arte" aria-hidden="true">
          <span className="home__banner-orbita" />
          <div className="home__banner-cartao home__banner-cartao--livro">
            <FiShoppingBag /><span>Bazar<br /><strong>Dê um novo uso.</strong></span>
          </div>
          <div className="home__banner-cartao home__banner-cartao--sacola">
            <FiCoffee /><span>Alimentação<br /><strong>Comida da comunidade.</strong></span>
          </div>
          <div className="home__banner-cartao home__banner-cartao--solidariedade">
            <FaHandHoldingHeart /><span>Ação Solidária<br /><strong>Apoie quem precisa.</strong></span>
          </div>
          <span className="home__banner-selo"><FiHeart /> Conexões que circulam</span>
        </div>
      </header>

      {/* Âncora de rolagem do CTA do banner. */}
      <div id="home-vitrine">
        <LinhaVitrine
          id="home-bazar"
          tom="bazar"
          icone={<FiShoppingBag />}
          titulo="Bazar"
          descricao="Livros, materiais e achados de quem vive o campus."
          rotuloLink="Ver todos no Bazar"
          destino="/bazar"
          buscar={listarItensBazar}
          renderizarCard={cardBazar}
          mensagemVazia="Ainda não há itens no Bazar. Volte em breve!"
          mensagemErro="Não foi possível carregar os itens do Bazar. Tente novamente."
        />

        <LinhaVitrine
          id="home-alimentacao"
          tom="alimentacao"
          icone={<FiCoffee />}
          titulo="Alimentação"
          descricao="Lanches e comidinhas feitos pela comunidade."
          rotuloLink="Ver todos em Alimentação"
          destino="/alimentacao"
          buscar={listarAlimentos}
          renderizarCard={cardAlimento}
          mensagemVazia="Ainda não há alimentos disponíveis. Volte em breve!"
          mensagemErro="Não foi possível carregar os alimentos. Tente novamente."
        />

        {/* Convite institucional para a Ação Solidária (NÃO é vitrine — LGPD). */}
        <section
          className="home__secao home__secao--solidariedade home__convite"
          aria-labelledby="home-solidariedade"
        >
          <div className="home__secao-identidade">
            <span className="home__secao-icone" aria-hidden="true">
              <FaHandHoldingHeart />
            </span>
            <div>
              <h2 className="home__secao-titulo" id="home-solidariedade">
                Ação Solidária
              </h2>
              <p className="home__secao-descricao">
                Peça apoio quando precisar ou ajude quem precisa — com privacidade.
                As solicitações são anônimas por padrão.
              </p>
            </div>
          </div>
          <Link className="home__ver-todos" to="/solidariedade">
            Ver Ação Solidária <FiArrowRight aria-hidden="true" />
          </Link>
        </section>
      </div>
    </section>
  );
}
