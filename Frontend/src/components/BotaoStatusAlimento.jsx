import { useRef, useState } from 'react';
import { FiEyeOff, FiRotateCcw } from 'react-icons/fi';

import '../pages/FormularioBazar.css';
import './BotaoStatusAlimento.css';
import { ApiError } from '../services/api.js';
import { inativarAlimento, reativarAlimento } from '../services/alimentos.js';

// ProdutoStatus do backend: 0 = Disponível, 1 = Esgotado, 2 = Inativo.
const STATUS_INATIVO = 2;

// Botão de gestão de um alimento do vendedor: alterna entre "Inativar anúncio"
// (some para os compradores, mas é reversível) e "Reativar anúncio" (volta a
// ficar disponível, desde que haja estoque), conforme o status atual.
//
// Não é o mesmo que excluir: o alimento NUNCA é apagado, só inativado. Por isso
// existe separado do BotaoExcluirItemBazar, que exclui de verdade no Bazar.
//
// Após o sucesso chama onAlterado(id), para a tela recarregar a lista e refletir
// o novo status vindo do servidor.
export default function BotaoStatusAlimento({ id, nome, status, onAlterado, disabled = false }) {
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState('');
  const emCurso = useRef(false);

  const inativo = status === STATUS_INATIVO || status === 'Inativo';

  async function alterar() {
    if (emCurso.current || disabled) return;

    // Inativar pede confirmação (o anúncio some para os compradores).
    // Reativar é seguro e não pede.
    if (
      !inativo &&
      !window.confirm(
        `Inativar “${nome}”? Ele deixa de aparecer para os compradores, mas você pode reativá-lo depois.`,
      )
    ) {
      return;
    }

    emCurso.current = true;
    setEnviando(true);
    setErro('');

    try {
      await (inativo ? reativarAlimento(id) : inativarAlimento(id));
    } catch (e) {
      setErro(mensagemErro(e, inativo));
      emCurso.current = false;
      setEnviando(false);
      return;
    }

    emCurso.current = false;
    setEnviando(false);
    onAlterado?.(id);
  }

  const rotulo = inativo
    ? enviando ? 'Reativando…' : 'Reativar anúncio'
    : enviando ? 'Inativando…' : 'Inativar anúncio';

  return (
    <div className="formulario-bazar-exclusao botao-status-alimento">
      <button
        type="button"
        className={
          inativo
            ? 'botao-status-alimento__reativar'
            : 'formulario-bazar-botao-excluir'
        }
        onClick={alterar}
        disabled={disabled || enviando}
      >
        {inativo ? <FiRotateCcw aria-hidden="true" /> : <FiEyeOff aria-hidden="true" />}
        {rotulo}
      </button>
      {erro && <p role="alert">{erro}</p>}
    </div>
  );
}

function mensagemErro(erro, reativando) {
  if (erro instanceof ApiError) {
    if (erro.status === 401) return 'Sua sessão expirou. Entre novamente para continuar.';
    if (erro.status === 403) return 'Você não tem autorização para alterar esse alimento.';
    if (erro.status === 404) return 'Este alimento não foi encontrado.';
    if (erro.status >= 500) return 'O servidor não conseguiu concluir a operação. Tente novamente mais tarde.';
    // 400 ao reativar: o backend recusa quando o estoque é zero.
    if (reativando && erro.status === 400) {
      return `${erro.message} Reponha o estoque pela edição do anúncio e tente de novo.`;
    }
    return erro.message;
  }
  return 'Não foi possível concluir a operação. Tente novamente.';
}