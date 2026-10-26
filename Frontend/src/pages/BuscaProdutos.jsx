import { useEffect, useState } from 'react';
import { FaMagnifyingGlass } from 'react-icons/fa6';

import ProdutoCard from '../components/ProdutoCard.jsx';
import { buscarProdutos } from '../services/produtosService.js';
import './BuscaProdutos.css';

const FILTROS_INICIAIS = {
  termo: '',
  tipo: '',
  precoMin: '',
  precoMax: '',
  campus: '',
  diaSemana: '',
  hora: '',
};

const TAMANHO_PAGINA = 12;
const DIAS = ['Domingo', 'Segunda', 'Terça', 'Quarta', 'Quinta', 'Sexta', 'Sábado'];

export default function BuscaProdutos() {
  // Dois estados: o que está no formulário (form) e o que foi de fato aplicado
  // (filtrosAplicados). A busca só dispara ao submeter, não a cada tecla.
  const [form, setForm] = useState(FILTROS_INICIAIS);
  const [filtrosAplicados, setFiltrosAplicados] = useState(FILTROS_INICIAIS);
  const [pagina, setPagina] = useState(1);

  const [resultado, setResultado] = useState(null);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  // A busca dispara quando os filtros aplicados ou a página mudam. O estado de
  // "carregando" é ligado por quem aciona a mudança (submeter/limpar/paginar) e
  // pelo valor inicial do useState — não dentro do effect, para não gerar
  // renders em cascata (regra react-hooks/set-state-in-effect).
  useEffect(() => {
    let cancelado = false;

    buscarProdutos({ ...filtrosAplicados, page: pagina, pageSize: TAMANHO_PAGINA })
      .then((dados) => {
        if (cancelado) return;
        setResultado(dados);
        setErro('');
      })
      .catch((e) => {
        if (cancelado) return;
        setResultado(null);
        setErro(e.message || 'Erro ao buscar produtos.');
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, [filtrosAplicados, pagina]);

  function atualizarCampo(evento) {
    const { name, value } = evento.target;
    setForm((atual) => ({ ...atual, [name]: value }));
  }

  function aplicarFiltros(evento) {
    evento.preventDefault();
    setCarregando(true);
    setPagina(1);
    setFiltrosAplicados({ ...form });
  }

  function limparFiltros() {
    setCarregando(true);
    setForm(FILTROS_INICIAIS);
    setPagina(1);
    setFiltrosAplicados({ ...FILTROS_INICIAIS });
  }

  function irParaPagina(nova) {
    setCarregando(true);
    setPagina(nova);
  }

  const itens = resultado?.items ?? [];
  const totalItens = resultado?.totalItems ?? 0;
  const totalPaginas = resultado?.totalPages ?? 0;

  return (
    <main className="busca">
      <header className="busca__cabecalho">
        <h1 className="busca__titulo">Buscar produtos</h1>
        <p className="busca__subtitulo">
          Encontre lanches e itens do bazar por palavra-chave, categoria, preço,
          campus e horário.
        </p>
      </header>

      <form className="busca__filtros" onSubmit={aplicarFiltros} aria-label="Filtros de busca">
        <div className="busca__campo busca__campo--termo">
          <label htmlFor="termo">Palavra-chave</label>
          <input
            id="termo"
            name="termo"
            type="search"
            value={form.termo}
            onChange={atualizarCampo}
            placeholder="Ex.: livro de cálculo, brownie"
          />
        </div>

        <div className="busca__campo">
          <label htmlFor="tipo">Categoria</label>
          <select id="tipo" name="tipo" value={form.tipo} onChange={atualizarCampo}>
            <option value="">Todas</option>
            <option value="bazar">Bazar</option>
            <option value="lanche">Lanches</option>
          </select>
        </div>

        <div className="busca__campo">
          <label htmlFor="precoMin">Preço mín. (R$)</label>
          <input
            id="precoMin"
            name="precoMin"
            type="number"
            min="0"
            step="0.01"
            value={form.precoMin}
            onChange={atualizarCampo}
          />
        </div>

        <div className="busca__campo">
          <label htmlFor="precoMax">Preço máx. (R$)</label>
          <input
            id="precoMax"
            name="precoMax"
            type="number"
            min="0"
            step="0.01"
            value={form.precoMax}
            onChange={atualizarCampo}
          />
        </div>

        <div className="busca__campo">
          <label htmlFor="campus">Campus</label>
          <input
            id="campus"
            name="campus"
            type="text"
            value={form.campus}
            onChange={atualizarCampo}
            placeholder="Ex.: São José dos Campos"
          />
        </div>

        <div className="busca__campo">
          <label htmlFor="diaSemana">Dia</label>
          <select id="diaSemana" name="diaSemana" value={form.diaSemana} onChange={atualizarCampo}>
            <option value="">Qualquer dia</option>
            {DIAS.map((dia, indice) => (
              <option key={dia} value={indice}>
                {dia}
              </option>
            ))}
          </select>
        </div>

        <div className="busca__campo">
          <label htmlFor="hora">Horário</label>
          <input id="hora" name="hora" type="time" value={form.hora} onChange={atualizarCampo} />
        </div>

        <div className="busca__acoes">
          <button type="submit" className="busca__botao">
            <FaMagnifyingGlass aria-hidden="true" /> Buscar
          </button>
          <button
            type="button"
            className="busca__botao busca__botao--secundario"
            onClick={limparFiltros}
          >
            Limpar
          </button>
        </div>
      </form>

      {erro && (
        <p className="busca__erro" role="alert">
          {erro}
        </p>
      )}

      <p className="busca__status" role="status" aria-live="polite">
        {carregando
          ? 'Carregando...'
          : !erro && `${totalItens} produto(s) encontrado(s)`}
      </p>

      {!carregando && !erro && itens.length === 0 && (
        <p className="busca__vazio">Nenhum produto encontrado com esses filtros.</p>
      )}

      <section className="busca__grade" aria-label="Resultados">
        {itens.map((produto) => (
          <ProdutoCard key={produto.id} produto={produto} />
        ))}
      </section>

      {totalPaginas > 1 && (
        <nav className="busca__paginacao" aria-label="Paginação dos resultados">
          <button
            type="button"
            className="busca__botao busca__botao--secundario"
            onClick={() => irParaPagina(pagina - 1)}
            disabled={pagina <= 1 || carregando}
          >
            Anterior
          </button>
          <span className="busca__pagina-info">
            Página {pagina} de {totalPaginas}
          </span>
          <button
            type="button"
            className="busca__botao busca__botao--secundario"
            onClick={() => irParaPagina(pagina + 1)}
            disabled={pagina >= totalPaginas || carregando}
          >
            Próxima
          </button>
        </nav>
      )}
    </main>
  );
}
