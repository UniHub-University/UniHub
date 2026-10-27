// Tela de histórico de pedidos do usuário autenticado (BRV-89).
//
// Fluxo:
//  1. Ao abrir (ou ao trocar de página), chama GET /api/pedidos?page=N&pageSize=10.
//     O api.js já envia o JWT; a API descobre o usuário pelo token (claim "sub").
//  2. A resposta vem no formato paginado combinado com o squad:
//     { items, page, pageSize, totalItems, totalPages }.
//  3. Mostramos os pedidos (mais recentes primeiro) e os botões de página.
//
// Rota protegida: o acesso sem login é barrado pela <RotaProtegida> no App.jsx.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { api, ApiError } from '../services/api.js';
import { useAuth } from '../auth/AuthContext.jsx';
import './HistoricoPedidos.css';

const TAMANHO_PAGINA = 10;

// A API devolve o status como número (enum PedidoStatus do backend).
const STATUS = {
  0: 'Reservado',
  1: 'Confirmado',
  2: 'Concluído',
  3: 'Cancelado',
};

function formatarMoeda(valor) {
  return Number(valor).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function formatarData(iso) {
  return new Date(iso).toLocaleString('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
}

export default function HistoricoPedidos() {
  const { logout } = useAuth();
  const [pagina, setPagina] = useState(1);
  const [dados, setDados] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    // "cancelado" evita atualizar o estado se o usuário trocar de página (ou sair
    // da tela) antes da resposta chegar.
    let cancelado = false;

    api
      .get(`/api/pedidos?page=${pagina}&pageSize=${TAMANHO_PAGINA}`)
      .then((resposta) => {
        if (cancelado) return;
        setDados(resposta);
        setErro('');
      })
      .catch((e) => {
        if (cancelado) return;
        // Token inválido ou expirado: sai da sessão (a RotaProtegida leva ao login).
        if (e instanceof ApiError && e.status === 401) {
          logout();
          return;
        }
        setErro(e instanceof ApiError ? e.message : 'Erro ao carregar o histórico.');
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, [pagina, logout]);

  function irParaPagina(numero) {
    setCarregando(true);
    setPagina(numero);
  }

  const pedidos = dados?.items ?? [];
  const totalPaginas = dados?.totalPages ?? 0;

  return (
    <main className="historico">
      <header className="historico__topo">
        <h1>Meus pedidos</h1>
        <Link to="/home" className="historico__voltar">
          Voltar ao início
        </Link>
      </header>

      {carregando && <p className="historico__aviso">Carregando seus pedidos...</p>}

      {!carregando && erro && (
        <p className="historico__aviso historico__aviso--erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && !erro && pedidos.length === 0 && (
        <p className="historico__aviso">Você ainda não fez nenhum pedido.</p>
      )}

      {!carregando && !erro && pedidos.length > 0 && (
        <>
          <ul className="historico__lista">
            {pedidos.map((pedido) => (
              <li key={pedido.pedidoId} className="pedido">
                <div className="pedido__cabecalho">
                  <div>
                    <strong>Pedido #{pedido.pedidoId.slice(0, 8).toUpperCase()}</strong>
                    <span className="pedido__data">{formatarData(pedido.createdAt)}</span>
                  </div>
                  <span className={`pedido__status pedido__status--${pedido.status}`}>
                    {STATUS[pedido.status] ?? 'Desconhecido'}
                  </span>
                </div>

                <ul className="pedido__itens">
                  {pedido.itens.map((item) => (
                    <li key={item.produtoId}>
                      <span>
                        {item.quantidade}x {item.produtoNome}
                      </span>
                      <span>{formatarMoeda(item.precoUnitarioNaCompra)} cada</span>
                    </li>
                  ))}
                </ul>

                <div className="pedido__total">
                  Total <strong>{formatarMoeda(pedido.total)}</strong>
                </div>
              </li>
            ))}
          </ul>

          <nav className="historico__paginacao" aria-label="Paginação do histórico">
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