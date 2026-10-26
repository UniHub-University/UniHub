import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { obterItemBazar } from '../services/anuncios.js';
import { ApiError } from '../services/api.js';
import { rotuloStatus, rotuloCondicao } from '../services/rotulosProduto.js';
import './DetalheBazar.css';

const moeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

function ConsultaDetalhe({ id }) {
  const [item, setItem] = useState(null);
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);
  const [tentativa, setTentativa] = useState(0);

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      try {
        const dados = await obterItemBazar(id);
        if (ativo) setItem(dados);
      } catch (e) {
        if (!ativo) return;

        let mensagem = 'Não foi possível carregar o anúncio.';
        if (e instanceof ApiError) {
          if (e.status === 404) {
            mensagem = 'Este anúncio não foi encontrado.';
          } else if (e.status === 401) {
            mensagem = 'Sessão inválida ou expirada. Saia e entre novamente.';
          } else if (e.status === 403) {
            mensagem = 'Você não tem permissão para consultar este anúncio.';
          }
        }
        setErro(mensagem);
      } finally {
        if (ativo) setCarregando(false);
      }
    }

    carregar();
    return () => { ativo = false; };
  }, [id, tentativa]);

  function tentarNovamente() {
    setErro('');
    setCarregando(true);
    setTentativa((valor) => valor + 1);
  }

  return (
    <section className="detalhe-bazar" aria-labelledby="titulo-detalhe">
      <Link to="/bazar">← Voltar ao Bazar</Link>
      <h1 id="titulo-detalhe">Detalhes do anúncio</h1>

      {carregando ? (
        <p role="status">Carregando anúncio…</p>
      ) : erro ? (
        <div>
          <p role="alert">{erro}</p>
          <button type="button" onClick={tentarNovamente}>
            Tentar novamente
          </button>
        </div>
      ) : item ? (
        <article>
          <h2>{item.nome}</h2>
          <p>{item.descricao || 'Descrição não informada.'}</p>
          <dl>
            <dt>Preço</dt>
            <dd>
              {Number.isFinite(item.preco)
                ? moeda.format(item.preco)
                : 'Não informado'}
            </dd>
            <dt>Categoria</dt>
            <dd>{item.categoria || 'Não informada'}</dd>
            <dt>Condição</dt>
            <dd>{rotuloCondicao(item.condicao) ?? 'Não informada'}</dd>
            <dt>Status</dt>
            <dd>{rotuloStatus(item.status) || 'Não informado'}</dd>
            <dt>Quantidade disponível</dt>
            <dd>{item.quantidadeDisponivel ?? 'Não informada'}</dd>
          </dl>
        </article>
      ) : null}
    </section>
  );
}

export default function DetalheBazar() {
  const { id } = useParams();
  return <ConsultaDetalhe key={id} id={id} />;
}