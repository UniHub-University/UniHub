import { useEffect, useState } from 'react';
import { Link, useParams } from 'react-router-dom';

import FormularioAlimento from '../components/FormularioAlimento.jsx';
import { obterAlimento, criarAlimento, atualizarAlimento } from '../services/alimentos.js';
import { ApiError } from '../services/api.js';
import './FormularioAlimentacao.css';

function mensagemErro(erro) {
  if (erro instanceof ApiError) {
    if (erro.status === 404) return 'Este alimento não foi encontrado. Ele pode ter sido excluído.';
    if (erro.status >= 500) return 'O servidor não conseguiu concluir a operação. Tente novamente mais tarde.';
    return erro.message;
  }
  return 'Não foi possível carregar o alimento. Tente novamente.';
}

function Editor({ id }) {
  const [semAutorizacao, setSemAutorizacao] = useState(false);
  const [alimento, setAlimento] = useState(null);
  const [carregando, setCarregando] = useState(Boolean(id));
  const [erro, setErro] = useState('');
  const [tentativa, setTentativa] = useState(0);
  const [, setSalvando] = useState(false);
  const [resultado, setResultado] = useState(null);

  useEffect(() => {
    if (!id) return;
    let ativo = true;
    obterAlimento(id)
      .then((dados) => { if (ativo) setAlimento(dados); })
      .catch((e) => {
        if (!ativo) return;
        if (e.status === 403) setSemAutorizacao(true);
        else setErro(mensagemErro(e));
      })
      .finally(() => { if (ativo) setCarregando(false); });
    return () => { ativo = false; };
  }, [id, tentativa]);

  async function salvar(dados) {
    if (id) {
      await atualizarAlimento(id, dados);
      setResultado({ mensagem: 'Alterações salvas com sucesso.', id });
    } else {
      const criado = await criarAlimento(dados);
      setResultado({ mensagem: 'Alimento cadastrado com sucesso.', id: criado?.id });
    }
  }

  function tentarNovamente() {
    setErro('');
    setCarregando(true);
    setTentativa((anterior) => anterior + 1);
  }

  return (
    <section className="formulario-bazar" aria-labelledby="formulario-alimento-titulo">
      <Link to="/vendedor">← Voltar ao Painel do Vendedor</Link>
      <h1 id="formulario-alimento-titulo">{id ? 'Editar alimento' : 'Cadastrar alimento'}</h1>

      {semAutorizacao ? (
        <div className="formulario-bazar-acesso-negado" role="alert">
          <h2>Acesso restrito</h2>
          <p>Você não tem autorização para editar esse alimento.</p>
          <p>Somente o vendedor responsável pode gerenciar este anúncio.</p>
          <Link to="/alimentacao">Voltar aos anúncios</Link>
        </div>
      ) : resultado ? (
        <div className="formulario-bazar-resultado">
          <p role="status">{resultado.mensagem}</p>
          {resultado.id && <Link to={`/alimentacao/${resultado.id}`}>Ver anúncio</Link>}
          <p><Link to="/vendedor">Voltar ao Painel do Vendedor</Link></p>
        </div>
      ) : carregando ? (
        <p role="status">Carregando alimento…</p>
      ) : erro ? (
        <div>
          <p role="alert">{erro}</p>
          <button type="button" onClick={tentarNovamente}>Tentar novamente</button>
        </div>
      ) : (
        <FormularioAlimento
          alimento={alimento}
          onSalvar={salvar}
          onOcupado={setSalvando}
          onSemAutorizacao={() => setSemAutorizacao(true)}
        />
      )}
    </section>
  );
}

export default function FormularioAlimentacao() {
  const { id } = useParams();
  return <Editor key={id ?? 'novo'} id={id} />;
}
