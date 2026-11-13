// Minhas correspondências (Ação Solidária — Task 4).
//
// Lista as correspondências que o voluntário logado aceitou
// (GET /api/correspondencias/minhas) e permite concluir ou cancelar cada uma.
//
// ⚠️ LGPD: a resposta é anônima — só código público e categoria da solicitação.
// A identidade do solicitante nunca aparece aqui (quem revela é o solicitante).

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError } from '../../../services/api.js';
import {
  listarMinhasCorrespondencias,
  concluirCorrespondencia,
  cancelarCorrespondencia,
} from '../../../services/acaoSolidaria.js';
import {
  rotuloCategoria,
  rotuloStatusCorrespondencia,
} from '../../../services/rotulosSolidariedade.js';
import './MinhasCorrespondencias.css';

// StatusCorrespondencia: 1 = AceitaPeloVoluntario (quando concluir/cancelar fazem sentido).
const STATUS_ACEITA = 1;

function formatarData(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString('pt-BR', { dateStyle: 'short' });
}

export default function MinhasCorrespondencias() {
  const [correspondencias, setCorrespondencias] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [mensagem, setMensagem] = useState('');
  // id da correspondência em ação (desabilita os botões daquele card).
  const [acaoId, setAcaoId] = useState(null);
  const [gatilho, setGatilho] = useState(0);

  useEffect(() => {
    let cancelado = false;

    async function carregar() {
      setCarregando(true);
      setErro('');
      try {
        const lista = await listarMinhasCorrespondencias();
        if (cancelado) return;
        setCorrespondencias(Array.isArray(lista) ? lista : []);
      } catch (e) {
        if (cancelado) return;
        if (e instanceof ApiError && e.status === 400) {
          setErro('Você precisa ser voluntário para ver suas correspondências.');
        } else if (e instanceof ApiError && e.status === 401) {
          setErro('Sessão expirada. Entre novamente.');
        } else {
          setErro('Não foi possível carregar suas correspondências.');
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

  async function executarAcao(id, acao, textoSucesso) {
    setErro('');
    setMensagem('');
    setAcaoId(id);
    try {
      await acao(id);
      setMensagem(textoSucesso);
    } catch (e) {
      setErro(
        e instanceof ApiError
          ? e.message
          : 'Não foi possível concluir a ação. Tente novamente.',
      );
    } finally {
      setAcaoId(null);
      setGatilho((n) => n + 1); // recarrega para refletir o novo estado
    }
  }

  return (
    <main className="minhas-correspondencias">
      <header className="minhas-correspondencias__topo">
        <h1>Minhas correspondências</h1>
        <Link to="/solidariedade/voluntario" className="minhas-correspondencias__voltar">
          ← Área do voluntário
        </Link>
      </header>

      {carregando && <p className="minhas-correspondencias__aviso">Carregando…</p>}

      {!carregando && erro && (
        <p className="minhas-correspondencias__aviso minhas-correspondencias__aviso--erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && mensagem && (
        <p className="minhas-correspondencias__aviso minhas-correspondencias__aviso--ok" role="status">
          {mensagem}
        </p>
      )}

      {!carregando && !erro && correspondencias.length === 0 && (
        <p className="minhas-correspondencias__aviso">
          Você ainda não aceitou nenhuma correspondência. Veja as{' '}
          <Link to="/solidariedade/sugestoes">sugestões para você</Link>.
        </p>
      )}

      {!carregando && !erro && correspondencias.length > 0 && (
        <ul className="minhas-correspondencias__lista">
          {correspondencias.map((c) => {
            const aceita = c.status === STATUS_ACEITA;
            return (
              <li key={c.id} className="correspondencia">
                <div className="correspondencia__cabecalho">
                  <span className="correspondencia__categoria">
                    {rotuloCategoria(c.categoriaSolicitacao)}
                  </span>
                  <span className={`correspondencia__status correspondencia__status--${c.status}`}>
                    {rotuloStatusCorrespondencia(c.status)}
                  </span>
                </div>

                <p className="correspondencia__codigo" title="Identificação anônima">
                  Solicitação {c.codigoPublicoSolicitacao}
                </p>

                <dl className="correspondencia__datas">
                  <div>
                    <dt>Aceita em</dt>
                    <dd>{formatarData(c.aceitaEm)}</dd>
                  </div>
                  <div>
                    <dt>Identidade revelada</dt>
                    <dd>{c.identidadeRevelada ? 'Sim' : 'Não'}</dd>
                  </div>
                </dl>

                {aceita && (
                  <div className="correspondencia__acoes">
                    <Link
                      to={`/solidariedade/doacoes?solicitacao=${encodeURIComponent(
                        c.id,
                      )}`}
                      className="correspondencia__botao correspondencia__botao--secundario"
                    >
                      Registrar doação
                    </Link>
                    <button
                      type="button"
                      className="correspondencia__botao"
                      disabled={acaoId === c.id}
                      onClick={() =>
                        executarAcao(c.id, concluirCorrespondencia, 'Correspondência concluída.')
                      }
                    >
                      {acaoId === c.id ? 'Processando…' : 'Concluir'}
                    </button>
                    <button
                      type="button"
                      className="correspondencia__botao correspondencia__botao--perigo"
                      disabled={acaoId === c.id}
                      onClick={() =>
                        executarAcao(c.id, cancelarCorrespondencia, 'Correspondência cancelada.')
                      }
                    >
                      Cancelar
                    </button>
                  </div>
                )}
              </li>
            );
          })}
        </ul>
      )}
    </main>
  );
}
