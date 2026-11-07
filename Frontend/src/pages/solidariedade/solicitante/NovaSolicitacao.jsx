// Tela "Nova solicitação" do lado SOLICITANTE da Ação Solidária.
//
// Formulário simples: o solicitante escolhe uma categoria de apoio e descreve
// o que precisa. Envia via criarSolicitacao() (acaoSolidaria.js). Ao concluir,
// redireciona para "Minhas solicitações" para acompanhar o status.
//
// LGPD: nada aqui expõe identidade — a solicitação circula só pelo CodigoPublico.

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { criarSolicitacao } from '../../../services/acaoSolidaria.js';
import { OPCOES_CATEGORIA } from '../../../services/rotulosSolidariedade.js';
import { ApiError } from '../../../services/api.js';
import './NovaSolicitacao.css';

const DESCRICAO_MIN = 10;
const DESCRICAO_MAX = 500;

export default function NovaSolicitacao() {
  const navigate = useNavigate();

  const [categoria, setCategoria] = useState('');
  const [descricao, setDescricao] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState('');

  function validar() {
    if (categoria === '') {
      return 'Selecione uma categoria de apoio.';
    }
    const texto = descricao.trim();
    if (texto.length < DESCRICAO_MIN) {
      return `A descrição deve ter ao menos ${DESCRICAO_MIN} caracteres.`;
    }
    if (texto.length > DESCRICAO_MAX) {
      return `A descrição deve ter no máximo ${DESCRICAO_MAX} caracteres.`;
    }
    return '';
  }

  async function enviar(evento) {
    evento.preventDefault();

    const mensagemErro = validar();
    if (mensagemErro) {
      setErro(mensagemErro);
      return;
    }

    setErro('');
    setEnviando(true);

    try {
      await criarSolicitacao({
        categoria: Number(categoria),
        descricao: descricao.trim(),
      });
      // Sucesso: vai para a lista acompanhar o status.
      navigate('/solidariedade/minhas', { replace: true });
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível criar a solicitação. Tente novamente.');
      setEnviando(false);
    }
  }

  const restante = DESCRICAO_MAX - descricao.trim().length;

  return (
    <main className="nova-solicitacao">
      <header className="nova-solicitacao__cabecalho">
        <h1 className="nova-solicitacao__titulo">Pedir apoio</h1>
        <p className="nova-solicitacao__subtitulo">
          Descreva o que você precisa. Sua solicitação é anônima — sua identidade só
          é revelada se você autorizar.
        </p>
      </header>

      <form className="nova-solicitacao__form" onSubmit={enviar} aria-label="Nova solicitação de apoio">
        <div className="nova-solicitacao__campo">
          <label htmlFor="categoria">Categoria</label>
          <select
            id="categoria"
            name="categoria"
            value={categoria}
            onChange={(e) => { setCategoria(e.target.value); setErro(''); }}
            disabled={enviando}
            required
          >
            <option value="" disabled>Selecione…</option>
            {OPCOES_CATEGORIA.map((opcao) => (
              <option key={opcao.valor} value={opcao.valor}>{opcao.rotulo}</option>
            ))}
          </select>
        </div>

        <div className="nova-solicitacao__campo">
          <label htmlFor="descricao">Descrição</label>
          <textarea
            id="descricao"
            name="descricao"
            rows={5}
            value={descricao}
            onChange={(e) => { setDescricao(e.target.value); setErro(''); }}
            maxLength={DESCRICAO_MAX}
            placeholder="Explique com suas palavras o apoio de que precisa."
            disabled={enviando}
            required
          />
          <small className="nova-solicitacao__contador">{restante} caracteres restantes</small>
        </div>

        {erro && (
          <p className="nova-solicitacao__erro" role="alert">{erro}</p>
        )}

        <div className="nova-solicitacao__acoes">
          <button type="submit" className="nova-solicitacao__botao" disabled={enviando}>
            {enviando ? 'Enviando…' : 'Enviar solicitação'}
          </button>
          <button
            type="button"
            className="nova-solicitacao__botao nova-solicitacao__botao--secundario"
            onClick={() => navigate('/solidariedade')}
            disabled={enviando}
          >
            Cancelar
          </button>
        </div>
      </form>
    </main>
  );
}
