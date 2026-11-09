import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';

import { listarVendedores } from '../services/vendedoresPublico.js';
import './ListaVendedores.css';

const TAMANHO_PAGINA = 12;

const formatadorNota = new Intl.NumberFormat('pt-BR', {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
});

// Lista publica de vendedores (/vendedores): quem tem anuncio disponivel agora,
// dos mais bem avaliados para os menos. A pagina fica na URL (?page=) e o filho e
// remontado a cada troca de pagina (key), assim "carregando" nasce ligado.
export default function ListaVendedores() {
  const [parametros, setParametros] = useSearchParams();
  const numero = Number(parametros.get('page') ?? 1);
  const pagina = Number.isSafeInteger(numero) && numero > 0 ? numero : 1;

  function irParaPagina(nova) {
    setParametros(nova > 1 ? { page: String(nova) } : {});
  }

  return (
    <main className="vendedores">
      <header className="vendedores__cabecalho">
        <h1 className="vendedores__titulo">Vendedores</h1>
        <p className="vendedores__subtitulo">
          Conheça quem está vendendo no campus e veja os anúncios de cada um.
        </p>
      </header>

      <PaginaDeVendedores key={pagina} pagina={pagina} irParaPagina={irParaPagina} />
    </main>
  );
}

function PaginaDeVendedores({ pagina, irParaPagina }) {
  const [resultado, setResultado] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    let cancelado = false;

    listarVendedores({ page: pagina, pageSize: TAMANHO_PAGINA })
      .then((dados) => {
        if (!cancelado) setResultado(dados);
      })
      .catch((e) => {
        if (!cancelado) setErro(e.message || 'Erro ao carregar os vendedores.');
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, [pagina]);

  if (carregando) {
    return <p className="vendedores__status" role="status">Carregando...</p>;
  }

  if (erro) {
    return <p className="vendedores__erro" role="alert">{erro}</p>;
  }

  const itens = resultado?.items ?? [];
  const totalPaginas = resultado?.totalPages ?? 0;

  if (itens.length === 0) {
    return <p className="vendedores__vazio">Nenhum vendedor com anúncios disponíveis no momento.</p>;
  }

  return (
    <>
      <section className="vendedores__grade" aria-label="Lista de vendedores">
        {itens.map((vendedor) => (
          <CartaoVendedor key={vendedor.id} vendedor={vendedor} />
        ))}
      </section>

      {totalPaginas > 1 && (
        <nav className="vendedores__paginacao" aria-label="Paginação dos vendedores">
          <button
            type="button"
            className="vendedores__botao"
            onClick={() => irParaPagina(pagina - 1)}
            disabled={pagina <= 1}
          >
            Anterior
          </button>
          <span className="vendedores__pagina-info">
            Página {pagina} de {totalPaginas}
          </span>
          <button
            type="button"
            className="vendedores__botao"
            onClick={() => irParaPagina(pagina + 1)}
            disabled={pagina >= totalPaginas}
          >
            Próxima
          </button>
        </nav>
      )}
    </>
  );
}

function CartaoVendedor({ vendedor }) {
  const temNota = vendedor.qtdAvaliacoes > 0;
  const destino = `/vendedores/${encodeURIComponent(vendedor.id)}`;

  return (
    <article className="vendedores__cartao">
      {vendedor.fotoUrl ? (
        <img
          className="vendedores__foto"
          src={vendedor.fotoUrl}
          alt=""
          referrerPolicy="no-referrer"
        />
      ) : (
        <span className="vendedores__foto vendedores__foto--inicial" aria-hidden="true">
          {vendedor.nome.charAt(0).toUpperCase()}
        </span>
      )}

      <h2 className="vendedores__nome">
        <Link to={destino}>{vendedor.nome}</Link>
      </h2>

      <p className="vendedores__nota">
        {temNota
          ? `★ ${formatadorNota.format(vendedor.notaMedia)} · ${vendedor.qtdAvaliacoes} avaliação(ões)`
          : 'Ainda sem avaliações'}
      </p>

      {vendedor.descricaoNegocio && (
        <p className="vendedores__descricao">{vendedor.descricaoNegocio}</p>
      )}

      <Link className="vendedores__ver" to={destino} aria-label={`Ver perfil de ${vendedor.nome}`}>
        Ver perfil →
      </Link>
    </article>
  );
}
