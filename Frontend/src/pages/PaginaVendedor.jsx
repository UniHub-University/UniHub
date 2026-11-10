import { useEffect, useState } from 'react';
import { Link, useParams, useSearchParams } from 'react-router-dom';

import ProdutoCard from '../components/ProdutoCard.jsx';
import { ApiError } from '../services/api.js';
import { listarProdutosDoVendedor, obterVendedorPublico } from '../services/vendedoresPublico.js';
import './PaginaVendedor.css';

const TAMANHO_PAGINA = 12;
// DiaSemana chega como inteiro (0 = Domingo ... 6 = Sabado): a API nao serializa enums como texto.
const DIAS = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'];

const formatadorNota = new Intl.NumberFormat('pt-BR', {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
});

// So aceita http(s) em links vindos do vendedor (evita "javascript:" etc.).
function linkSeguro(url) {
  try {
    const { protocol } = new URL(url);
    return protocol === 'https:' || protocol === 'http:' ? url : null;
  } catch {
    return null;
  }
}

// Perfil publico de um vendedor + os anuncios dele. O pai e remontado a cada
// troca de vendedor (key) e o filho de anuncios a cada troca de pagina (key):
// assim o estado "carregando" nasce ligado, sem set-state dentro do effect.
export default function PaginaVendedor() {
  const { id } = useParams();
  return <PerfilDoVendedor key={id} id={id} />;
}

function PerfilDoVendedor({ id }) {
  const [parametros, setParametros] = useSearchParams();
  const numero = Number(parametros.get('page') ?? 1);
  const pagina = Number.isSafeInteger(numero) && numero > 0 ? numero : 1;

  const [vendedor, setVendedor] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    let cancelado = false;

    obterVendedorPublico(id)
      .then((dados) => {
        if (!cancelado) setVendedor(dados);
      })
      .catch((e) => {
        if (cancelado) return;
        setErro(
          e instanceof ApiError && e.status === 404
            ? 'Vendedor não encontrado.'
            : e.message || 'Erro ao carregar o vendedor.',
        );
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, [id]);

  function irParaPagina(nova) {
    setParametros(nova > 1 ? { page: String(nova) } : {});
  }

  if (carregando) {
    return (
      <main className="vendedor-pagina">
        <p className="vendedor-pagina__status" role="status">Carregando...</p>
      </main>
    );
  }

  if (erro) {
    return (
      <main className="vendedor-pagina">
        <p className="vendedor-pagina__erro" role="alert">{erro}</p>
        <Link to="/vendedores" className="vendedor-pagina__voltar">← Ver vendedores</Link>
      </main>
    );
  }

  const cardapio = vendedor.cardapioUrl ? linkSeguro(vendedor.cardapioUrl) : null;
  const temNota = vendedor.qtdAvaliacoes > 0;

  return (
    <main className="vendedor-pagina">
      <header className="vendedor-pagina__cabecalho">
        {vendedor.fotoUrl ? (
          <img
            className="vendedor-pagina__foto"
            src={vendedor.fotoUrl}
            alt={`Foto de ${vendedor.nome}`}
            referrerPolicy="no-referrer"
          />
        ) : (
          <span className="vendedor-pagina__foto vendedor-pagina__foto--inicial" aria-hidden="true">
            {vendedor.nome.charAt(0).toUpperCase()}
          </span>
        )}

        <div className="vendedor-pagina__identidade">
          <h1 className="vendedor-pagina__nome">{vendedor.nome}</h1>
          <p className="vendedor-pagina__nota">
            {temNota
              ? `★ ${formatadorNota.format(vendedor.notaMedia)} · ${vendedor.qtdAvaliacoes} avaliação(ões)`
              : 'Ainda sem avaliações'}
          </p>
          {vendedor.descricaoNegocio && (
            <p className="vendedor-pagina__descricao">{vendedor.descricaoNegocio}</p>
          )}
          {cardapio && (
            <a className="vendedor-pagina__cardapio" href={cardapio} target="_blank" rel="noopener noreferrer">
              Ver cardápio ↗
            </a>
          )}
        </div>
      </header>

      {(vendedor.locais.length > 0 || vendedor.horarios.length > 0) && (
        <section className="vendedor-pagina__info" aria-label="Onde e quando">
          {vendedor.locais.length > 0 && (
            <div>
              <h2 className="vendedor-pagina__subtitulo">Onde encontrar</h2>
              <ul className="vendedor-pagina__lista">
                {vendedor.locais.map((local) => (
                  <li key={`${local.campus}|${local.pontoDeEncontro}`}>
                    {local.campus} · {local.pontoDeEncontro}
                  </li>
                ))}
              </ul>
            </div>
          )}
          {vendedor.horarios.length > 0 && (
            <div>
              <h2 className="vendedor-pagina__subtitulo">Horários</h2>
              <ul className="vendedor-pagina__lista">
                {vendedor.horarios.map((h) => (
                  <li key={`${h.diaSemana}|${h.horaInicio}|${h.horaFim}`}>
                    {DIAS[h.diaSemana] ?? 'Dia'} · {h.horaInicio}–{h.horaFim}
                  </li>
                ))}
              </ul>
            </div>
          )}
        </section>
      )}

      <h2 className="vendedor-pagina__subtitulo">Anúncios</h2>
      <AnunciosDoVendedor key={pagina} id={id} pagina={pagina} irParaPagina={irParaPagina} />
    </main>
  );
}

function AnunciosDoVendedor({ id, pagina, irParaPagina }) {
  const [resultado, setResultado] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    let cancelado = false;

    listarProdutosDoVendedor(id, { page: pagina, pageSize: TAMANHO_PAGINA })
      .then((dados) => {
        if (!cancelado) setResultado(dados);
      })
      .catch((e) => {
        if (!cancelado) setErro(e.message || 'Erro ao carregar os anúncios.');
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, [id, pagina]);

  if (carregando) {
    return <p className="vendedor-pagina__status" role="status">Carregando anúncios...</p>;
  }

  if (erro) {
    return <p className="vendedor-pagina__erro" role="alert">{erro}</p>;
  }

  const itens = resultado?.items ?? [];
  const totalPaginas = resultado?.totalPages ?? 0;

  if (itens.length === 0) {
    return <p className="vendedor-pagina__vazio">Este vendedor não tem anúncios disponíveis no momento.</p>;
  }

  return (
    <>
      <section className="vendedor-pagina__grade" aria-label="Anúncios do vendedor">
        {itens.map((produto) => (
          <ProdutoCard key={produto.id} produto={produto} />
        ))}
      </section>

      {totalPaginas > 1 && (
        <nav className="vendedor-pagina__paginacao" aria-label="Paginação dos anúncios">
          <button
            type="button"
            className="vendedor-pagina__botao"
            onClick={() => irParaPagina(pagina - 1)}
            disabled={pagina <= 1}
          >
            Anterior
          </button>
          <span className="vendedor-pagina__pagina-info">
            Página {pagina} de {totalPaginas}
          </span>
          <button
            type="button"
            className="vendedor-pagina__botao"
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
