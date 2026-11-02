// Pedidos recebidos pelo vendedor (P9 — Frente B).
//
// Lista os pedidos que contêm produtos do vendedor autenticado (GET
// /api/pedidos/vendedor, P6) e permite dar baixa manual num pedido reservado
// (POST /api/pedidos/{id}/concluir, P7).
//
// LGPD: o backend devolve APENAS o nome do comprador — a tela nunca exibe mais
// que isso. Itens/Total refletem só os produtos deste vendedor.
//
// Rota protegida por <SomenteVendedor> no App.jsx.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError } from '../services/api.js';
import { listarPedidosDoVendedor, concluirPedido } from '../services/pedidos.js';
import { useAuth } from '../auth/AuthContext.jsx';
import './PedidosVendedor.css';

const TAMANHO_PAGINA = 10;

// Mesmo mapa de status usado no histórico do comprador, para consistência visual.
const STATUS = {
  0: 'Reservado',
  1: 'Confirmado',
  2: 'Concluído',
  3: 'Cancelado',
  4: 'Expirado',
};

const STATUS_RESERVADO = 0;

// TipoPedido: 0 = Compra, 1 = Reserva.
const TIPO = { 0: 'Compra', 1: 'Reserva' };

function formatarMoeda(valor) {
  return Number(valor).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function formatarData(iso) {
  return new Date(iso).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
}

function formatarDataCurta(iso) {
  return new Date(iso).toLocaleDateString('pt-BR', { dateStyle: 'short' });
}

export default function PedidosVendedor() {
  const { logout } = useAuth();
  const [pagina, setPagina] = useState(1);
  const [dados, setDados] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  // id do pedido em processo de conclusão (desabilita o botão daquele card).
  const [concluindoId, setConcluindoId] = useState(null);
  // mensagem de erro específica de uma ação de concluir.
  const [erroAcao, setErroAcao] = useState('');
  // Incrementado para forçar o recarregamento da lista (ex.: após concluir).
  const [gatilho, setGatilho] = useState(0);

  // Toda a carga vive dentro do effect (padrão recomendado): assim não chamamos
  // setState de forma síncrona a partir de uma função externa referenciada no
  // effect. Recarrega quando muda a página ou quando o gatilho é incrementado.
  useEffect(() => {
    let cancelado = false;
    // Reset síncrono ao (re)carregar: volta a UI para "carregando" antes de
    // refazer a busca (sincronização com a API). Mesmo padrão adotado em
    // PerfilVendedor.jsx para a regra react-hooks/set-state-in-effect.
    // eslint-disable-next-line react-hooks/set-state-in-effect
    setCarregando(true);

    listarPedidosDoVendedor({ page: pagina, pageSize: TAMANHO_PAGINA })
      .then((resposta) => {
        if (cancelado) return;
        setDados(resposta);
        setErro('');
      })
      .catch((e) => {
        if (cancelado) return;
        if (e instanceof ApiError && e.status === 401) {
          logout();
          return;
        }
        setErro(e instanceof ApiError ? e.message : 'Erro ao carregar os pedidos.');
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, [pagina, gatilho, logout]);

  async function aoConcluir(pedidoId) {
    setErroAcao('');
    setConcluindoId(pedidoId);
    try {
      await concluirPedido(pedidoId);
    } catch (e) {
      // 409 normalmente = o pedido mudou de estado (já concluído/expirado).
      setErroAcao(
        e instanceof ApiError
          ? e.message
          : 'Não foi possível concluir o pedido. Tente novamente.',
      );
    } finally {
      setConcluindoId(null);
      // Recarrega a lista para refletir o estado real (sucesso ou conflito).
      setGatilho((n) => n + 1);
    }
  }

  function irParaPagina(numero) {
    setPagina(numero);
  }

  const pedidos = dados?.items ?? [];
  const totalPaginas = dados?.totalPages ?? 0;

  return (
    <main className="pedidos-vendedor">
      <header className="pedidos-vendedor__topo">
        <h1>Pedidos recebidos</h1>
        <Link to="/vendedor" className="pedidos-vendedor__voltar">
          Voltar ao painel
        </Link>
      </header>

      {carregando && <p className="pedidos-vendedor__aviso">Carregando os pedidos...</p>}

      {!carregando && erro && (
        <p className="pedidos-vendedor__aviso pedidos-vendedor__aviso--erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && !erro && pedidos.length === 0 && (
        <p className="pedidos-vendedor__aviso">
          Você ainda não recebeu pedidos nos seus produtos.
        </p>
      )}

      {erroAcao && (
        <p className="pedidos-vendedor__aviso pedidos-vendedor__aviso--erro" role="alert">
          {erroAcao}
        </p>
      )}

      {!carregando && !erro && pedidos.length > 0 && (
        <>
          <ul className="pedidos-vendedor__lista">
            {pedidos.map((pedido) => {
              const reservado = pedido.status === STATUS_RESERVADO;
              const ehReserva = pedido.tipo === 1;
              return (
                <li key={pedido.pedidoId} className="pv-pedido">
                  <div className="pv-pedido__cabecalho">
                    <div>
                      <strong>Pedido #{pedido.pedidoId.slice(0, 8).toUpperCase()}</strong>
                      <span className="pv-pedido__data">{formatarData(pedido.createdAt)}</span>
                    </div>
                    <span className={`pv-pedido__status pv-pedido__status--${pedido.status}`}>
                      {STATUS[pedido.status] ?? 'Desconhecido'}
                    </span>
                  </div>

                  <div className="pv-pedido__meta">
                    <span className="pv-pedido__tag">{TIPO[pedido.tipo] ?? 'Pedido'}</span>
                    <span className="pv-pedido__comprador">
                      Comprador: <strong>{pedido.compradorNome}</strong>
                    </span>
                    {ehReserva && pedido.expiraEm && (
                      <span className="pv-pedido__expira">
                        Reserva até {formatarDataCurta(pedido.expiraEm)}
                      </span>
                    )}
                  </div>

                  <ul className="pv-pedido__itens">
                    {pedido.itens.map((item) => (
                      <li key={item.produtoId}>
                        <span>
                          {item.quantidade}x {item.produtoNome}
                        </span>
                        <span>{formatarMoeda(item.precoUnitarioNaCompra)} cada</span>
                      </li>
                    ))}
                  </ul>

                  <div className="pv-pedido__base">
                    <span className="pv-pedido__total">
                      Total <strong>{formatarMoeda(pedido.total)}</strong>
                    </span>
                    {reservado && (
                      <button
                        type="button"
                        className="pv-pedido__concluir"
                        disabled={concluindoId === pedido.pedidoId}
                        onClick={() => aoConcluir(pedido.pedidoId)}
                      >
                        {concluindoId === pedido.pedidoId ? 'Concluindo…' : 'Concluir'}
                      </button>
                    )}
                  </div>
                </li>
              );
            })}
          </ul>

          <nav className="pedidos-vendedor__paginacao" aria-label="Paginação dos pedidos">
            <button
              type="button"
              onClick={() => irParaPagina(pagina - 1)}
              disabled={pagina <= 1}
            >
              Anterior
            </button>
            <span>
              Página {dados.page} de {totalPaginas}
            </span>
            <button
              type="button"
              onClick={() => irParaPagina(pagina + 1)}
              disabled={pagina >= totalPaginas}
            >
              Próxima
            </button>
          </nav>
        </>
      )}
    </main>
  );
}