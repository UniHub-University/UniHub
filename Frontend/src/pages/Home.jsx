// Home — pré-visualização rápida dos anúncios mais recentes (BRV-148).
//
// A ideia da Home é dar um panorama: os produtos mais recentes publicados,
// independentemente da categoria, como uma vitrine de entrada. O feed completo
// (com busca) vive na aba Bazar (/bazar).
//
// Hoje só existe o endpoint do Bazar (GET /api/itens-bazar); quando surgirem os
// endpoints de Alimentação/Ação Solidária, esta pré-visualização pode agregar
// as outras fontes sem mudar a estrutura da tela (ver plano BRV-148).

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiBookOpen, FiShoppingBag, FiHeart } from 'react-icons/fi';

import { listarItensBazar } from '../services/anuncios.js';
import { ApiError } from '../services/api.js';
import { ordenarPorMaisRecente } from '../services/anunciosUtils.js';
import CardAnuncio from '../components/CardAnuncio.jsx';
import './Home.css';

// Quantidade de anúncios exibidos na vitrine de pré-visualização.
const QTD_PREVIA = 6;

export default function Home() {
  const [recentes, setRecentes] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      setCarregando(true);
      setErro('');
      try {
        const dados = await listarItensBazar();
        if (ativo) {
          setRecentes(ordenarPorMaisRecente(dados).slice(0, QTD_PREVIA));
        }
      } catch (e) {
        if (!ativo) return;
        setErro(
          e instanceof ApiError
            ? e.message
            : 'Não foi possível carregar os anúncios. Tente novamente.',
        );
      } finally {
        if (ativo) setCarregando(false);
      }
    }

    carregar();
    return () => {
      ativo = false;
    };
  }, []);

  return (
    <section className="home">
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

      <div className="home__secao">
        <div className="home__secao-topo">
          <h2 className="home__secao-titulo">Adicionados recentemente</h2>
          <Link className="home__ver-todos" to="/bazar">
            Ver todos no Bazar →
          </Link>
        </div>

        {carregando && <p className="home__status">Carregando anúncios…</p>}

        {!carregando && erro && (
          <p className="home__erro" role="alert">
            {erro}
          </p>
        )}

        {!carregando && !erro && recentes.length === 0 && (
          <p className="home__status">
            Ainda não há anúncios publicados. Volte em breve!
          </p>
        )}

        {!carregando && !erro && recentes.length > 0 && (
          <ul className="home__grade">
            {recentes.map((item) => (
              <CardAnuncio key={item.id} item={item} />
            ))}
          </ul>
        )}
      </div>
    </section>
  );
}