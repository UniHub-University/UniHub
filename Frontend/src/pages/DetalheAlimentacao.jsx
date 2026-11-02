import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { buscarAlimento } from '../services/alimentos.js';
import { ApiError } from '../services/api.js';
import { rotuloStatus } from '../services/rotulosProduto.js';
import './DetalheBazar.css';

const moeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

function ConsultaDetalhe({ id }) {
  const [alimento, setAlimento] = useState(null);
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);
  const [tentativa, setTentativa] = useState(0);

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      try {
        const dados = await buscarAlimento(id);
        if (ativo) setAlimento(dados);
      } catch (e) {
        if (!ativo) return;

        let mensagem = 'Não foi possível carregar o alimento.';
        if (e instanceof ApiError) {
          if (e.status === 404) {
            mensagem = 'Este alimento não foi encontrado.';
          } else if (e.status === 401) {
            mensagem = 'Sessão inválida ou expirada. Saia e entre novamente.';
          } else if (e.status === 403) {
            mensagem = 'Você não tem permissão para consultar este alimento.';
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
      <Link to="/alimentacao">← Voltar à Alimentação</Link>
      <h1 id="titulo-detalhe">Detalhes do alimento</h1>

      {carregando ? (
        <p role="status">Carregando alimento…</p>
      ) : erro ? (
        <div>
          <p role="alert">{erro}</p>
          <button type="button" onClick={tentarNovamente}>
            Tentar novamente
          </button>
        </div>
      ) : alimento ? (
        <article>
          <h2>{alimento.nome}</h2>
          <p>{alimento.descricao || 'Descrição não informada.'}</p>
          <dl>
            <dt>Preço</dt>
            <dd>
              {Number.isFinite(Number(alimento.preco))
                ? moeda.format(Number(alimento.preco))
                : 'Não informado'}
            </dd>
            <dt>Categoria</dt>
            <dd>{alimento.categoria || 'Não informada'}</dd>
            <dt>Restrições</dt>
            <dd>{alimento.restricoes && alimento.restricoes !== 'null'
                    ? alimento.restricoes
                    : 'Nenhuma informada'}</dd>
            <dt>Status</dt>
            <dd>{rotuloStatus(alimento.status) || 'Não informado'}</dd>
            <dt>Quantidade disponível</dt>
            <dd>{alimento.quantidadeDisponivel ?? 'Não informada'}</dd>
          </dl>
        </article>
      ) : null}
    </section>
  );
}

export default function DetalheAlimentacao() {
  const { id } = useParams();
  return <ConsultaDetalhe key={id} id={id} />;
}