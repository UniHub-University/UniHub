import { useRef, useState } from 'react';
import { FiTrash2 } from 'react-icons/fi';
import '../pages/FormularioBazar.css';
import { excluirItemBazar, mensagemErroItem } from '../services/itensBazar.js';

// A listagem pode reutilizar este botão e recarregar a listagem no onExcluido.
export default function BotaoExcluirItemBazar({ id, nome, onExcluido, onOcupado, onSemAutorizacao, disabled = false }) {
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState('');
  const [excluido, setExcluido] = useState(false);
  const emCurso = useRef(false);

  async function excluir() {
    if (emCurso.current || disabled || excluido) return;
    if (!window.confirm(`Excluir “${nome}”? Esta ação não pode ser desfeita.`)) return;
    emCurso.current = true;
    setEnviando(true);
    onOcupado?.(true);
    setErro('');
    try {
      await excluirItemBazar(id);
    } catch (e) {
      if (e.status === 403 && onSemAutorizacao) onSemAutorizacao();
      else setErro(mensagemErroItem(e));
      emCurso.current = false;
      setEnviando(false);
      onOcupado?.(false);
      return;
    }
    setExcluido(true);
    setEnviando(false);
      onOcupado?.(false);
    onExcluido?.(id);
  }

  return (
    <div className="formulario-bazar-exclusao">
      <button className="formulario-bazar-botao-excluir" type="button" onClick={excluir} disabled={disabled || enviando || excluido}>
        <FiTrash2 aria-hidden="true" />
        {enviando ? 'Excluindo…' : excluido ? 'Item excluído' : 'Excluir anúncio'}
      </button>
      {erro && <p role="alert">{erro}</p>}
      {excluido && <p role="status">Item excluído com sucesso.</p>}
    </div>
  );
}
