// Detalhe de um anúncio de Alimentação.
//
// Exibe as informações completas do alimento e permite a compra imediata.
// Alimentação usa somente TipoPedido.Compra; Reserva não faz parte deste módulo.

import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';

import { obterAlimento } from '../services/alimentos.js';
import { criarPedido, TIPO_PEDIDO } from '../services/pedidos.js';
import { ApiError } from '../services/api.js';
import { rotuloStatus } from '../services/rotulosProduto.js';
import BlocoVendedor from '../components/BlocoVendedor.jsx';

import './DetalheAlimentacao.css';

const moeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

function alimentoDisponivel(alimento) {
  const statusDisponivel =
    alimento.status === 0 || alimento.status === 'Disponivel';

  return (
    statusDisponivel &&
    Number(alimento.quantidadeDisponivel) > 0
  );
}

function ConsultaDetalhe({ id }) {
  const [alimento, setAlimento] = useState(null);
  const [erro, setErro] = useState('');
  const [carregando, setCarregando] = useState(true);
  const [tentativa, setTentativa] = useState(0);

  const [quantidade, setQuantidade] = useState(1);
  const [comprando, setComprando] = useState(false);
  const [erroCompra, setErroCompra] = useState('');
  const [compraRealizada, setCompraRealizada] = useState(false);

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      setCarregando(true);
      setErro('');

      try {
        const dados = await obterAlimento(id);

        if (ativo) {
          setAlimento(dados);
          setQuantidade(1);
          setCompraRealizada(false);
          setErroCompra('');
        }
      } catch (error) {
        if (!ativo) return;

        let mensagem = 'Não foi possível carregar o alimento.';

        if (error instanceof ApiError) {
          if (error.status === 404) {
            mensagem = 'Este alimento não foi encontrado.';
          } else if (error.status === 401) {
            mensagem = 'Sessão inválida ou expirada. Saia e entre novamente.';
          } else if (error.status === 403) {
            mensagem = 'Você não tem permissão para consultar este alimento.';
          }
        }

        setErro(mensagem);
      } finally {
        if (ativo) {
          setCarregando(false);
        }
      }
    }

    carregar();

    return () => {
      ativo = false;
    };
  }, [id, tentativa]);

  function tentarNovamente() {
    setErro('');
    setCarregando(true);
    setTentativa((valor) => valor + 1);
  }

  function alterarQuantidade(evento) {
    const valor = Number(evento.target.value);
    const estoque = Number(alimento?.quantidadeDisponivel) || 1;

    if (Number.isNaN(valor)) {
      setQuantidade(1);
      return;
    }

    setQuantidade(Math.min(Math.max(1, valor), estoque));
  }

  async function comprar() {
    if (!alimentoDisponivel(alimento)) {
      setErroCompra('Este alimento não está disponível para compra.');
      return;
    }

    const estoque = Number(alimento.quantidadeDisponivel);
    const quantidadeFinal = Math.min(
      Math.max(1, Number(quantidade) || 1),
      estoque,
    );

    setComprando(true);
    setErroCompra('');
    setCompraRealizada(false);

    try {
      await criarPedido({
        produtoId: alimento.id,
        quantidade: quantidadeFinal,
        tipo: TIPO_PEDIDO.COMPRA,
      });

      const estoqueAtualizado = estoque - quantidadeFinal;

      setAlimento((atual) => ({
        ...atual,
        quantidadeDisponivel: estoqueAtualizado,
        status: estoqueAtualizado > 0 ? atual.status : 1,
      }));

      setCompraRealizada(true);
    } catch (error) {
      setErroCompra(
        error instanceof ApiError
          ? error.message
          : 'Não foi possível concluir a compra. Tente novamente.',
      );
    } finally {
      setComprando(false);
    }
  }

  return (
    <section
      className="detalhe-alimentacao"
      aria-labelledby="titulo-detalhe-alimentacao"
    >
      <Link to="/alimentacao">← Voltar à Alimentação</Link>

      <h1 id="titulo-detalhe-alimentacao">
        Detalhes do alimento
      </h1>

      {carregando ? (
        <p role="status">Carregando alimento…</p>
      ) : erro ? (
        <div className="detalhe-alimentacao__mensagem">
          <p role="alert">{erro}</p>

          <button type="button" onClick={tentarNovamente}>
            Tentar novamente
          </button>
        </div>
      ) : alimento ? (
        <article className="detalhe-alimentacao__cartao">
          {alimento.imagemUrl && (
            <img
              className="detalhe-alimentacao__imagem"
              src={alimento.imagemUrl}
              alt={alimento.nome}
            />
          )}
          <h2>{alimento.nome}</h2>

          <p className="detalhe-alimentacao__descricao">
            {alimento.descricao || 'Descrição não informada.'}
          </p>

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
            <dd>
              {alimento.restricoes &&
              alimento.restricoes !== 'null'
                ? alimento.restricoes
                : 'Nenhuma informada'}
            </dd>

            <dt>Status</dt>
            <dd>{rotuloStatus(alimento.status) || 'Não informado'}</dd>

            <dt>Quantidade disponível</dt>
            <dd>
              {alimento.quantidadeDisponivel ?? 'Não informada'}
            </dd>
          </dl>

          <BlocoVendedor vendedorId={alimento.vendedorId} />

          {alimentoDisponivel(alimento) ? (
            compraRealizada ? (
              <div
                className="detalhe-alimentacao__sucesso"
                role="status"
              >
                <p>Alimento comprado com sucesso!</p>

                <Link
                  to="/pedidos"
                  className="detalhe-alimentacao__link-pedidos"
                >
                  Ver em “Meus pedidos”
                </Link>
              </div>
            ) : (
              <div className="detalhe-alimentacao__acoes">
                <label className="detalhe-alimentacao__quantidade">
                  <span>Quantidade</span>

                  <input
                    type="number"
                    min="1"
                    max={alimento.quantidadeDisponivel}
                    value={quantidade}
                    disabled={comprando}
                    onChange={alterarQuantidade}
                  />

                  <small>
                    {alimento.quantidadeDisponivel} disponível(is)
                  </small>
                </label>

                <button
                  type="button"
                  className="detalhe-alimentacao__comprar"
                  disabled={comprando}
                  onClick={comprar}
                >
                  {comprando ? 'Comprando…' : 'Comprar'}
                </button>

                {erroCompra && (
                  <p
                    className="detalhe-alimentacao__erro-compra"
                    role="alert"
                  >
                    {erroCompra}
                  </p>
                )}
              </div>
            )
          ) : (
            <p
              className="detalhe-alimentacao__indisponivel"
              role="status"
            >
              Este alimento está esgotado ou indisponível no momento.
            </p>
          )}
        </article>
      ) : null}
    </section>
  );
}

export default function DetalheAlimentacao() {
  const { id } = useParams();

  return <ConsultaDetalhe key={id} id={id} />;
}
