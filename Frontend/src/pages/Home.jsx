// Home / Feed de anúncios.
//
// Substitui o PlaceholderHome. Lista os anúncios do Bazar consumindo o único
// endpoint de produtos existente hoje (GET /api/itens-bazar) e trata os três
// estados essenciais: carregando, erro e vazio. Inclui uma busca textual
// client-side (a lista ainda não é paginada no backend — débito conhecido).

import { useEffect, useMemo, useState } from 'react';

import { Link } from 'react-router-dom';

import { listarItensBazar } from '../services/anuncios.js';
import { ApiError } from '../services/api.js';
import './Home.css';


// Formata o preço (number) no padrão brasileiro de moeda.
const formatadorMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

function formatarPreco(valor) {
  if (typeof valor !== 'number' || Number.isNaN(valor)) return '—';
  return formatadorMoeda.format(valor);
}

// Traduz o status numérico/string do produto para um rótulo amigável.
function rotuloStatus(status) {
  const mapa = {
    0: 'Disponível',
    1: 'Esgotado',
    2: 'Inativo',
    Disponivel: 'Disponível',
    Esgotado: 'Esgotado',
    Inativo: 'Inativo',
  };
  return mapa[status] ?? String(status ?? '');
}

export default function Home() {
  const [itens, setItens] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [busca, setBusca] = useState('');

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      setCarregando(true);
      setErro('');
      try {
        const dados = await listarItensBazar();
        if (ativo) setItens(dados);
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

  // Filtro textual client-side por nome, descrição e categoria.
  const itensFiltrados = useMemo(() => {
    const termo = busca.trim().toLowerCase();
    if (!termo) return itens;
    return itens.filter((item) => {
      const alvo = [item.nome, item.descricao, item.categoria]
        .filter(Boolean)
        .join(' ')
        .toLowerCase();
      return alvo.includes(termo);
    });
  }, [itens, busca]);

  return (
    <section className="home">
      <header className="home__cabecalho">
        <div>
          <h1 className="home__titulo">Anúncios da comunidade</h1>
          <p className="home__subtitulo">
            Explore os anúncios do Bazar Acadêmico.
            Os produtos de Alimentação serão integrados em breve.
        </p>
        </div>

        <input
          type="search"
          className="home__busca"
          placeholder="Buscar anúncios…"
          value={busca}
          onChange={(e) => setBusca(e.target.value)}
          aria-label="Buscar anúncios"
        />
      </header>

      {carregando && <p className="home__status">Carregando anúncios…</p>}

      {!carregando && erro && (
        <p className="home__erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && !erro && itens.length === 0 && (
        <p className="home__status">
          Ainda não há anúncios publicados. Volte em breve!
        </p>
      )}

      {!carregando && !erro && itens.length > 0 && itensFiltrados.length === 0 && (
        <p className="home__status">
          Nenhum anúncio encontrado para “{busca}”.
        </p>
      )}

      {!carregando && !erro && itensFiltrados.length > 0 && (
        <ul className="home__grade">
          {itensFiltrados.map((item) => (
            <li key={item.id} className="cartao-anuncio">
              <div className="cartao-anuncio__topo">
                <h2 className="cartao-anuncio__nome">
                    <Link
                        className="cartao-anuncio__link"
                        to={`/bazar/${item.id}`}
                    >
                        {item.nome}
                    </Link>
                </h2>
                {item.categoria && (
                  <span className="cartao-anuncio__categoria">
                    {item.categoria}
                  </span>
                )}
              </div>

              {item.descricao && (
                <p className="cartao-anuncio__descricao">{item.descricao}</p>
              )}

              <div className="cartao-anuncio__rodape">
                <span className="cartao-anuncio__preco">
                  {formatarPreco(item.preco)}
                </span>
                <span className="cartao-anuncio__status">
                  {rotuloStatus(item.status)}
                </span>
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
