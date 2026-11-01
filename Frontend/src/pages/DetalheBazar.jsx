import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import { obterItemBazar } from '../services/anuncios.js';
import { ApiError } from '../services/api.js';
import { criarPedido, TIPO_PEDIDO } from '../services/pedidos.js';
import { rotuloStatus, rotuloCondicao } from '../services/rotulosProduto.js';
import './DetalheBazar.css';

const moeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

// Status 0 = Disponivel (ProdutoStatus). Só vendemos/reservamos se disponível.
const STATUS_DISPONIVEL = 0;

// Área de ação (Comprar / Reservar) de um item do Bazar. Isolada para manter o
// componente de consulta enxuto. Recebe o item já carregado.
function AcoesPedido({ item }) {
  const estoque = Number(item.quantidadeDisponivel) || 0;
  const disponivel =
    (item.status === STATUS_DISPONIVEL || item.status === 'Disponivel') && estoque > 0;

  const [quantidade, setQuantidade] = useState(1);
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState('');
  // sucesso guarda o tipo enviado (0/1) para diferenciar a mensagem; null = sem ação.
  const [sucessoTipo, setSucessoTipo] = useState(null);

  async function enviar(tipo) {
    // Trava defensiva: nunca enviar fora de 1..estoque.
    const qtd = Math.min(Math.max(1, Number(quantidade) || 1), estoque);

    setErro('');
    setSucessoTipo(null);
    setEnviando(true);
    try {
      await criarPedido({ produtoId: item.id, quantidade: qtd, tipo });
      setSucessoTipo(tipo);
    } catch (e) {
      // 409 traz o motivo de negócio em e.message (ex.: estoque insuficiente).
      setErro(
        e instanceof ApiError
          ? e.message
          : 'Não foi possível concluir. Tente novamente.',
      );
    } finally {
      setEnviando(false);
    }
  }

  if (!disponivel) {
    return (
      <p className="detalhe-bazar__indisponivel" role="status">
        Este item não está disponível no momento.
      </p>
    );
  }

  // Após sucesso, troca a área de ação pela confirmação + link para o histórico.
  if (sucessoTipo !== null) {
    const acao = sucessoTipo === TIPO_PEDIDO.RESERVA ? 'reservado' : 'comprado';
    return (
      <div className="detalhe-bazar__sucesso" role="status">
        <p>Item {acao} com sucesso! 🎉</p>
        <Link to="/pedidos" className="detalhe-bazar__link-pedidos">
          Ver em “Meus pedidos”
        </Link>
      </div>
    );
  }

  return (
    <div className="detalhe-bazar__acoes">
      <label className="detalhe-bazar__qtd">
        <span>Quantidade</span>
        <input
          type="number"
          min={1}
          max={estoque}
          value={quantidade}
          disabled={enviando}
          onChange={(e) => {
            const valor = Number(e.target.value);
            if (Number.isNaN(valor)) return setQuantidade(1);
            // Mantém dentro de 1..estoque.
            setQuantidade(Math.min(Math.max(1, valor), estoque));
          }}
        />
        <small>{estoque} disponível(is)</small>
      </label>

      <div className="detalhe-bazar__botoes">
        <button
          type="button"
          className="detalhe-bazar__comprar"
          disabled={enviando}
          onClick={() => enviar(TIPO_PEDIDO.COMPRA)}
        >
          {enviando ? 'Enviando…' : 'Comprar'}
        </button>
        <button
          type="button"
          className="detalhe-bazar__reservar"
          disabled={enviando}
          onClick={() => enviar(TIPO_PEDIDO.RESERVA)}
        >
          Reservar
        </button>
      </div>

      <p className="detalhe-bazar__dica">
        A reserva fica guardada por tempo limitado; a compra é definitiva.
      </p>

      {erro && (
        <p className="detalhe-bazar__erro-acao" role="alert">
          {erro}
        </p>
      )}
    </div>
  );
}

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

          <AcoesPedido item={item} />
        </article>
      ) : null}
    </section>
  );
}

export default function DetalheBazar() {
  const { id } = useParams();
  return <ConsultaDetalhe key={id} id={id} />;
}