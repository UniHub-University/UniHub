// Home — vitrine de entrada com duas linhas: Bazar e Alimentação.
//
// Cada linha mostra os anúncios mais recentes da categoria e tem um link
// "Ver todos" para a página específica (/bazar e /alimentacao), onde ficam o
// feed completo e a busca. As duas linhas carregam de forma independente: se
// uma API falhar, a outra continua aparecendo (cada uma trata seu próprio
// carregando/erro/vazio).

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiBookOpen, FiShoppingBag, FiHeart, FiCoffee } from 'react-icons/fi';

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
    <section className="inicio">
      <header className="home__banner">
        <div className="home__banner-conteudo">
          <span className="home__banner-etiqueta">FEITO PELA COMUNIDADE, PARA A COMUNIDADE</span>
          <h1 className="home__banner-titulo">Seu campus.<br />Novas conexões.<br /><em>Mais possibilidades.</em></h1>
          <p className="home__banner-descricao">Dê uma nova história ao que você não usa mais e descubra achados de quem compartilha a vida universitária com você.</p>
          <Link className="home__banner-botao" to="/bazar">Explorar o Bazar <span aria-hidden="true">↗</span></Link>
        </div>
        <div className="home__banner-arte" aria-hidden="true">
          <span className="home__banner-orbita" />
          <div className="home__banner-cartao home__banner-cartao--livro"><FiBookOpen /><span>Uma nova leitura.<br /><strong>Outra descoberta.</strong></span></div>
          <div className="home__banner-cartao home__banner-cartao--sacola"><FiShoppingBag /><span>Seu próximo achado<br /><strong>pode estar aqui.</strong></span></div>
          <span className="home__banner-selo"><FiHeart /> Conexões que circulam</span>
        </div>
      </header>

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
    </section>
  );
}
