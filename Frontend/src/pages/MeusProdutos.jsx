// Meus Produtos — listagem e gerenciamento dos produtos do vendedor (BRV-164).
//
// Consome GET /api/perfil/vendedor/produtos (vendedor derivado do token) e lista
// os produtos do vendedor em qualquer status, inclusive esgotados/inativos (que a
// busca pública esconde). Cada item permite editar (reusa o formulário existente)
// e remover: item do Bazar é excluído (BotaoExcluirItemBazar); alimento é
// inativado/reativado (BotaoStatusAlimento, BRV-192), pois é reversível.
// Atalho para adicionar um novo produto.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiPlusCircle, FiEdit2 } from 'react-icons/fi';

import { listarMeusProdutos } from '../services/vendedor.js';
import { excluirItemBazar } from '../services/itensBazar.js';
import { ApiError } from '../services/api.js';
import { rotuloStatus } from '../services/rotulosProduto.js';
import { conectar, desconectar } from '../services/painelRealtime.js';
import BotaoExcluirItemBazar from '../components/BotaoExcluirItemBazar.jsx';
import BotaoStatusAlimento from '../components/BotaoStatusAlimento.jsx';
import './MeusProdutos.css';

const formatadorMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

function formatarPreco(valor) {
  if (typeof valor !== 'number' || Number.isNaN(valor)) return '—';
  return formatadorMoeda.format(valor);
}

const TIPO_ROTULO = { bazar: 'Bazar', lanche: 'Alimentação' };

export default function MeusProdutos() {
  const [produtos, setProdutos] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  // Ao mudar, refaz a busca (usado depois de inativar/reativar um alimento).
  const [recarregar, setRecarregar] = useState(0);
  // Tempo real (Frente 2 / SignalR): estado da conexão e aviso de novo pedido.
  const [conexaoRealtime, setConexaoRealtime] = useState('desconectado');
  const [avisoNovoPedido, setAvisoNovoPedido] = useState('');

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      // "Carregando…" só na primeira carga; nas recargas a lista continua visível.
      if (recarregar === 0) setCarregando(true);
      setErro('');
      try {
        const dados = await listarMeusProdutos();
        if (ativo) setProdutos(dados);
      } catch (e) {
        if (!ativo) return;
        setErro(
          e instanceof ApiError
            ? e.message
            : 'Não foi possível carregar seus produtos. Tente novamente.',
        );
      } finally {
        if (ativo) setCarregando(false);
      }
    }

    carregar();
    return () => {
      ativo = false;
    };
  }, [recarregar]);

  // Tempo real (B3): conecta ao PainelHub e atualiza os cards sem refresh.
  // Roda uma vez no ciclo de vida da tela; o estado inicial (REST) acima segue
  // funcionando mesmo que o hub caia (degradação graciosa — corte seguro).
  useEffect(() => {
    conectar({
      // Estoque mudou (alguém comprou/reservou): atualiza quantidade e status
      // do card correspondente em memória. produtoId casa com produto.id.
      onEstoqueAtualizado: ({ produtoId, quantidadeDisponivel, status }) => {
        setProdutos((atuais) =>
          atuais.map((p) =>
            p.id === produtoId
              ? { ...p, quantidadeDisponivel, status }
              : p,
          ),
        );
      },
      // Novo pedido afetou um produto do vendedor: aviso discreto e temporário.
      onNovoPedido: ({ nomeProduto, quantidade }) => {
        setAvisoNovoPedido(
          `Novo pedido: ${quantidade}× ${nomeProduto}.`,
        );
      },
      onMudancaConexao: (estado) => setConexaoRealtime(estado),
    }).catch(() => {
      // Falha ao conectar não quebra o painel: segue com o estado inicial REST.
      setConexaoRealtime('desconectado');
    });

    return () => {
      desconectar();
    };
  }, []);

  // Remove da lista o item do Bazar excluído, sem precisar recarregar a página.
  function aoExcluir(id) {
    setProdutos((atuais) => atuais.filter((p) => p.id !== id));
  }

  // Alimento inativado/reativado: busca de novo para refletir o status do servidor.
  function aoAlterarStatus() {
    setRecarregar((n) => n + 1);
  }

  return (
    <section className="meus-produtos" aria-labelledby="meus-produtos-titulo">
      <header className="meus-produtos__cabecalho">
        <div>
          <h1 id="meus-produtos-titulo">Meus produtos</h1>
          <p>Visualize e gerencie os anúncios que você publicou.</p>
        </div>
        <div className="meus-produtos__cabecalho-acoes">
          <span
            className={`meus-produtos__realtime meus-produtos__realtime--${conexaoRealtime}`}
            role="status"
          >
            {conexaoRealtime === 'conectado'
              ? 'Ao vivo'
              : conexaoRealtime === 'reconectando'
              ? 'Reconectando…'
              : 'Offline'}
          </span>
          <Link className="meus-produtos__novo" to="/vendedor/produtos/novo">
            <FiPlusCircle aria-hidden="true" />
            Adicionar produto
          </Link>
        </div>
      </header>

      {avisoNovoPedido && (
        <p className="meus-produtos__novo-pedido" role="status">
          {avisoNovoPedido}
        </p>
      )}

      {carregando && <p className="meus-produtos__status">Carregando seus produtos…</p>}

      {!carregando && erro && (
        <p className="meus-produtos__erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && !erro && produtos.length === 0 && (
        <p className="meus-produtos__status">
          Você ainda não publicou nenhum produto. Que tal{' '}
          <Link to="/vendedor/produtos/novo">adicionar o primeiro</Link>?
        </p>
      )}

      {!carregando && !erro && produtos.length > 0 && (
        <ul className="meus-produtos__lista">
          {produtos.map((produto) => (
            <li key={produto.id} className="meus-produtos__item">
              <div className="meus-produtos__info">
                <div className="meus-produtos__topo">
                  <h2 className="meus-produtos__nome">{produto.nome}</h2>
                  <span className="meus-produtos__tag">
                    {TIPO_ROTULO[produto.tipo] ?? produto.tipo}
                  </span>
                  <span
                    className={`meus-produtos__status meus-produtos__status--${produto.status}`}
                  >
                    {rotuloStatus(produto.status)}
                  </span>
                </div>
                <div className="meus-produtos__meta">
                  <span className="meus-produtos__preco">
                    {formatarPreco(produto.preco)}
                  </span>
                  <span className="meus-produtos__qtd">
                    {produto.quantidadeDisponivel} em estoque
                  </span>
                </div>
              </div>

              <div className="meus-produtos__acoes">
                <Link
                  className="meus-produtos__editar"
                  to={
                    produto.tipo === 'lanche'
                      ? `/vendedor/alimentos/${produto.id}/editar`
                      : `/vendedor/produtos/${produto.id}/editar`
                    }
                >
                  <FiEdit2 aria-hidden="true" />
                  Editar
                </Link>
                {produto.tipo === 'lanche' ? (
                  <BotaoStatusAlimento
                    id={produto.id}
                    nome={produto.nome}
                    status={produto.status}
                    onAlterado={aoAlterarStatus}
                  />
                ) : (
                  <BotaoExcluirItemBazar
                    id={produto.id}
                    nome={produto.nome}
                    onExcluido={aoExcluir}
                    aoExcluir={excluirItemBazar}
                  />
                )}
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
