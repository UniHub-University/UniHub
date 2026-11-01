// Bazar — feed completo de anúncios do Bazar Acadêmico.
//
// Antes esta tela era a /home; na BRV-148 foi separada: a Home virou uma
// pré-visualização dos mais recentes e este feed completo (com busca) passou a
// viver na aba Bazar (/bazar). Consome o único endpoint de produtos existente
// hoje (GET /api/itens-bazar) e trata os estados carregando/erro/vazio.
// A pesquisa é encaminhada para a tela unificada /produtos, com tipo=bazar.

import { useEffect, useState } from 'react';

import { listarItensBazar } from '../services/anuncios.js';
import { ApiError } from '../services/api.js';
import { ordenarPorMaisRecente } from '../services/anunciosUtils.js';
import CardAnuncio from '../components/CardAnuncio.jsx';
import './Bazar.css';
import { Link, useNavigate } from 'react-router-dom';
import { usePerfilVendedor } from '../auth/PerfilVendedor.jsx';

export default function Bazar() {
  const { ehVendedor } = usePerfilVendedor();
  const navigate = useNavigate();
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

  const itensFiltrados = ordenarPorMaisRecente(itens);
  function pesquisar(evento) {
    evento.preventDefault();
    const parametros = new URLSearchParams({ tipo: 'bazar' });
    if (busca.trim()) parametros.set('termo', busca.trim());
    navigate(`/produtos?${parametros.toString()}`);
  }

  // Texto do contador de resultados (reflete a lista filtrada e exibida).
  const totalExibido = itensFiltrados.length;
  const textoContador =
    totalExibido === 1 ? '1 anúncio' : `${totalExibido} anúncios`;

  return (
    <section className="bazar">
      <header className="bazar__cabecalho">
        <div>
          <h1 className="bazar__titulo">Bazar Acadêmico</h1>
          <p className="bazar__subtitulo">
            Explore todos os anúncios do Bazar Acadêmico. Os produtos de
            Alimentação serão integrados em breve.
          </p>
        </div>

        {ehVendedor && <Link className="bazar__adicionar" to="/vendedor/produtos/novo">Adicionar produto</Link>}

        <form className="bazar__pesquisa" onSubmit={pesquisar} role="search" aria-label="Pesquisar no Bazar">
          <input type="search" className="bazar__busca" placeholder="Buscar anúncios…"
            value={busca} onChange={(e) => setBusca(e.target.value)} aria-label="Buscar anúncios" />
          <button className="bazar__pesquisar" type="submit">Pesquisar</button>
        </form>
      </header>

      {carregando && <p className="bazar__status">Carregando anúncios…</p>}

      {!carregando && erro && (
        <p className="bazar__erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && !erro && itens.length === 0 && (
        <p className="bazar__status">
          Ainda não há anúncios publicados. Volte em breve!
        </p>
      )}

      {!carregando && !erro && itensFiltrados.length > 0 && (
        <>
          <p className="bazar__contador" aria-live="polite">
            {textoContador}
          </p>

          <ul className="bazar__grade">
            {itensFiltrados.map((item) => (
              <CardAnuncio key={item.id} item={item} />
            ))}
          </ul>
        </>
      )}
    </section>
  );
}