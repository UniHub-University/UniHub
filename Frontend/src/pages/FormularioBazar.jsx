import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';
import FormularioItemBazar from '../components/FormularioItemBazar.jsx';
import BotaoExcluirItemBazar from '../components/BotaoExcluirItemBazar.jsx';
import { obterItemBazarParaEdicao, criarItemBazar, atualizarItemBazar, mensagemErroItem } from '../services/itensBazar.js';
import './FormularioBazar.css';

function Editor({ id }) {
  const [semAutorizacao, setSemAutorizacao] = useState(false);
  const [item, setItem] = useState(null);
  const [carregando, setCarregando] = useState(Boolean(id));
  const [erro, setErro] = useState('');
  const [tentativa, setTentativa] = useState(0);
  const [salvando, setSalvando] = useState(false);
  const [excluindo, setExcluindo] = useState(false);
  const [resultado, setResultado] = useState(null);

  useEffect(() => {
    if (!id) return;
    let ativo = true;
    obterItemBazarParaEdicao(id)
      .then((dados) => { if (ativo) setItem(dados); })
      .catch((e) => { if (ativo) {
        if (e.status === 403) setSemAutorizacao(true);
        else setErro(mensagemErroItem(e));
      } })
      .finally(() => { if (ativo) setCarregando(false); });
    return () => { ativo = false; };
  }, [id, tentativa]);

  async function salvar(dados) {
    if (id) {
      await atualizarItemBazar(id, dados);
      setResultado({ mensagem: 'Alterações salvas com sucesso.', id });
    } else {
      const criado = await criarItemBazar(dados);
      setResultado({ mensagem: 'Item cadastrado com sucesso.', id: criado?.id });
    }
  }

  function tentarNovamente() {
    setErro(''); setCarregando(true); setTentativa((anterior) => anterior + 1);
  }

  return (
    <section className="formulario-bazar" aria-labelledby="formulario-bazar-titulo">
      <Link to="/vendedor">← Voltar ao Painel do Vendedor</Link>
      <h1 id="formulario-bazar-titulo">{id ? 'Editar item do Bazar' : 'Cadastrar item no Bazar'}</h1>
      {semAutorizacao ? <div className="formulario-bazar-acesso-negado" role="alert">
        <h2>Acesso restrito</h2>
        <p>Você não tem autorização para editar/excluir esse produto.</p>
        <p>Somente o vendedor responsável pode gerenciar este anúncio.</p>
        <Link to="/bazar">Voltar aos anúncios</Link>
      </div> : resultado ? <div className="formulario-bazar-resultado">
        <p role="status">{resultado.mensagem}</p>
        {resultado.id && <Link to={`/bazar/${resultado.id}`}>Ver anúncio</Link>}
        <p><Link to="/vendedor">Voltar ao Painel do Vendedor</Link></p>
      </div> : carregando ? <p role="status">Carregando item…</p> : erro ? <div>
        <p role="alert">{erro}</p><button type="button" onClick={tentarNovamente}>Tentar novamente</button>
      </div> : <>
        <FormularioItemBazar item={item} onSalvar={salvar} onOcupado={setSalvando} onSemAutorizacao={() => setSemAutorizacao(true)} disabled={excluindo} />
        {id && item && <div className="formulario-bazar-area-exclusao">
          <h2>Excluir anúncio</h2>
          <p>A exclusão é definitiva. Se o item estiver vinculado a pedidos, o servidor pode impedir a operação.</p>
          <BotaoExcluirItemBazar id={id} nome={item.nome} disabled={salvando} onOcupado={setExcluindo}
            onSemAutorizacao={() => setSemAutorizacao(true)} onExcluido={() => setResultado({ mensagem: 'Item excluído com sucesso.' })} />
        </div>}
      </>}
    </section>
  );
}

export default function FormularioBazar() {
  const { id } = useParams();
  return <Editor key={id ?? 'novo'} id={id} />;
}