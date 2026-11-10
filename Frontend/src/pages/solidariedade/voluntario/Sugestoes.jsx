// Sugestões para o voluntário (Ação Solidária — Task 3).
//
// Lista as solicitações compatíveis com as áreas de atuação do voluntário
// (GET /api/correspondencias/sugestoes) e permite aceitar uma (POST
// /api/correspondencias/aceitar). Ao aceitar, cria-se uma correspondência.
//
// ⚠️ LGPD: a lista é ANÔNIMA por construção — só código público, categoria e
// descrição. A identidade do solicitante nunca aparece aqui.
//
// Concorrência: se outro voluntário aceitar a mesma solicitação primeiro, o
// backend devolve 409 — mostramos a mensagem e recarregamos a lista.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError } from '../../../services/api.js';
import { listarSugestoes, aceitarCorrespondencia } from '../../../services/acaoSolidaria.js';
import { rotuloCategoria } from '../../../services/rotulosSolidariedade.js';
import './Sugestoes.css';

function formatarData(iso) {
  return new Date(iso).toLocaleDateString('pt-BR', { dateStyle: 'short' });
}

export default function Sugestoes() {
  const [sugestoes, setSugestoes] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [mensagem, setMensagem] = useState('');
  // id da solicitação em processo de aceite (desabilita o botão do card).
  const [aceitandoId, setAceitandoId] = useState(null);
  // incrementado para forçar o recarregamento da lista.
  const [gatilho, setGatilho] = useState(0);

  useEffect(() => {
    let cancelado = false;

    // Toda a carga (inclusive o reset de "carregando"/erro) vive dentro de uma
    // função async: assim nenhum setState é chamado de forma síncrona no corpo
    // do effect, resolvendo a regra react-hooks/set-state-in-effect sem disable.
    async function carregar() {
      setCarregando(true);
      setErro('');
      try {
        const lista = await listarSugestoes();
        if (cancelado) return;
        setSugestoes(Array.isArray(lista) ? lista : []);
      } catch (e) {
        if (cancelado) return;
        if (e instanceof ApiError && e.status === 400) {
          // Não é voluntário: orienta a ir para a área do voluntário.
          setErro('Você precisa ser voluntário para ver sugestões.');
        } else if (e instanceof ApiError && e.status === 401) {
          setErro('Sessão expirada. Entre novamente.');
        } else {
          setErro('Não foi possível carregar as sugestões.');
        }
      } finally {
        if (!cancelado) setCarregando(false);
      }
    }

    carregar();

    return () => {
      cancelado = true;
    };
  }, [gatilho]);

  async function aoAceitar(solicitacaoId) {
    setErro('');
    setMensagem('');
    setAceitandoId(solicitacaoId);
    try {
      await aceitarCorrespondencia(solicitacaoId);
      setMensagem('Correspondência aceita! Acompanhe em "Minhas correspondências".');
    } catch (e) {
      // 409 = outro voluntário aceitou primeiro (ou já não está aberta).
      setMensagem('');
      setErro(
        e instanceof ApiError
          ? e.message
          : 'Não foi possível aceitar. Tente novamente.',
      );
    } finally {
      setAceitandoId(null);
      // Recarrega para refletir o estado real (aceita sai; disputada some).
      setGatilho((n) => n + 1);
    }
  }

  return (
    <main className="sugestoes">
      <header className="sugestoes__topo">
        <h1>Sugestões para você</h1>
        <Link to="/solidariedade/voluntario" className="sugestoes__voltar">
          ← Área do voluntário
        </Link>
      </header>

      {carregando && <p className="sugestoes__aviso">Carregando sugestões…</p>}

      {!carregando && erro && (
        <p className="sugestoes__aviso sugestoes__aviso--erro" role="alert">
          {erro}{' '}
          <Link to="/solidariedade/voluntario">Ir para a área do voluntário</Link>
        </p>
      )}

      {!carregando && mensagem && (
        <p className="sugestoes__aviso sugestoes__aviso--ok" role="status">
          {mensagem}
        </p>
      )}

      {!carregando && !erro && sugestoes.length === 0 && (
        <p className="sugestoes__aviso">
          Nenhuma sugestão no momento. Verifique se você definiu áreas de atuação e se
          sua atuação está ativa na{' '}
          <Link to="/solidariedade/voluntario">Área do voluntário</Link>.
        </p>
      )}

      {!carregando && !erro && sugestoes.length > 0 && (
        <ul className="sugestoes__lista">
          {sugestoes.map((s) => (
            <li key={s.id} className="sugestao">
              <div className="sugestao__cabecalho">
                <span className="sugestao__categoria">{rotuloCategoria(s.categoria)}</span>
                <span className="sugestao__codigo" title="Identificação anônima">
                  {s.codigoPublico}
                </span>
              </div>

              <p className="sugestao__descricao">{s.descricao}</p>

              <div className="sugestao__base">
                <span className="sugestao__data">Aberta em {formatarData(s.createdAt)}</span>
                <button
                  type="button"
                  className="sugestao__aceitar"
                  disabled={aceitandoId === s.id}
                  onClick={() => aoAceitar(s.id)}
                >
                  {aceitandoId === s.id ? 'Aceitando…' : 'Aceitar'}
                </button>
              </div>
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
