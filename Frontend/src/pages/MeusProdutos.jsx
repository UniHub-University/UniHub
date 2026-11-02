// Meus Produtos — listagem e gerenciamento dos produtos do vendedor (BRV-164).
//
// Consome GET /api/perfil/vendedor/produtos (vendedor derivado do token) e lista
// os produtos do vendedor em qualquer status, inclusive esgotados/inativos (que a
// busca pública esconde). Cada item permite editar (reusa o formulário existente)
// e excluir (reusa o BotaoExcluirItemBazar). Atalho para adicionar um novo produto.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { FiPlusCircle, FiEdit2 } from 'react-icons/fi';

import { listarMeusProdutos } from '../services/vendedor.js';
import { ApiError } from '../services/api.js';
import { rotuloStatus } from '../services/rotulosProduto.js';
import BotaoExcluirItemBazar from '../components/BotaoExcluirItemBazar.jsx';
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

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      setCarregando(true);
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
  }, []);

  // Remove da lista o item excluído, sem precisar recarregar a página.
  function aoExcluir(id) {
    setProdutos((atuais) => atuais.filter((p) => p.id !== id));
  }

  return (
    <section className="meus-produtos" aria-labelledby="meus-produtos-titulo">
      <header className="meus-produtos__cabecalho">
        <div>
          <h1 id="meus-produtos-titulo">Meus produtos</h1>
          <p>Visualize e gerencie os anúncios que você publicou.</p>
        </div>
        <Link className="meus-produtos__novo" to="/vendedor/produtos/novo">
          <FiPlusCircle aria-hidden="true" />
          Adicionar produto
        </Link>
      </header>

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
                  to={`/vendedor/produtos/${produto.id}/editar`}
                >
                  <FiEdit2 aria-hidden="true" />
                  Editar
                </Link>
                <BotaoExcluirItemBazar
                  id={produto.id}
                  nome={produto.nome}
                  onExcluido={aoExcluir}
                />
              </div>
            </li>
          ))}
        </ul>
      )}
    </section>
  );
}
