// Tela "Minhas solicitações" do lado SOLICITANTE da Ação Solidária.
//
// Lista as solicitações do próprio usuário (GET /api/SolicitacaoApoio/minhas),
// em qualquer status, para ele acompanhar o andamento, e permite cancelar as
// que ainda estão abertas ou em andamento.
//
// LGPD: a lista mostra o voluntário por código público (pseudônimo). O nome/
// contato do voluntário só aparece depois que o solicitante revela a própria
// identidade — ação que parte sempre dele, nunca automática.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import {
  listarMinhasSolicitacoes,
  cancelarSolicitacao,
  listarMinhasCorrespondencias,
  revelarIdentidade,
} from '../../../services/acaoSolidaria.js';
import { rotuloCategoria, rotuloStatusSolicitacao } from '../../../services/rotulosSolidariedade.js';
import { ApiError } from '../../../services/api.js';
import './MinhasSolicitacoes.css';

// Status de correspondência: Aceita pelo voluntário (1) é quando faz sentido
// o solicitante revelar a própria identidade.
const CORRESPONDENCIA_ACEITA = 1;

function formatarData(iso) {
  return new Date(iso).toLocaleDateString('pt-BR', { dateStyle: 'short' });
}

// Mapeia o status (inteiro do enum) para uma classe de cor do selo.
function classeStatus(status) {
  return `minhas-solicitacoes__status minhas-solicitacoes__status--${status}`;
}

// Status que ainda permitem cancelamento: Aberta (0) e Em andamento (1).
// Concluída (2) e Cancelada (3) não podem ser canceladas.
function podeCancelar(status) {
  return status === 0 || status === 1;
}

export default function MinhasSolicitacoes() {
  const [solicitacoes, setSolicitacoes] = useState([]);
  // Correspondências indexadas por solicitacaoId (pega a aceita, se houver).
  const [correspondenciaPorSolicitacao, setCorrespondenciaPorSolicitacao] = useState({});
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [cancelandoId, setCancelandoId] = useState(null);
  const [revelandoId, setRevelandoId] = useState(null);

  useEffect(() => {
    let cancelado = false;

    // Busca solicitações e correspondências em paralelo. A falha nas
    // correspondências não derruba a tela — só desativa a revelação.
    Promise.all([
      listarMinhasSolicitacoes(),
      listarMinhasCorrespondencias().catch(() => []),
    ])
      .then(([listaSolicitacoes, listaCorrespondencias]) => {
        if (cancelado) return;
        setSolicitacoes(Array.isArray(listaSolicitacoes) ? listaSolicitacoes : []);

        const indice = {};
        (Array.isArray(listaCorrespondencias) ? listaCorrespondencias : []).forEach((c) => {
          // Prioriza uma correspondência aceita/revelada sobre uma cancelada.
          const atual = indice[c.solicitacaoId];
          if (!atual || c.status === CORRESPONDENCIA_ACEITA || c.identidadeRevelada) {
            indice[c.solicitacaoId] = c;
          }
        });
        setCorrespondenciaPorSolicitacao(indice);
        setErro('');
      })
      .catch((e) => {
        if (cancelado) return;
        setSolicitacoes([]);
        setErro(e instanceof ApiError ? e.message : 'Não foi possível carregar suas solicitações.');
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, []);

  async function aoCancelar(id) {
    setErro('');
    setCancelandoId(id);
    try {
      await cancelarSolicitacao(id);
      // Reflete o novo status na lista sem refazer a busca inteira.
      setSolicitacoes((atual) =>
        atual.map((s) => (s.id === id ? { ...s, status: 3 } : s)),
      );
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível cancelar a solicitação.');
    } finally {
      setCancelandoId(null);
    }
  }

  async function aoRevelarIdentidade(correspondencia) {
    setErro('');
    setRevelandoId(correspondencia.id);
    try {
      await revelarIdentidade(correspondencia.id);
      // Marca como revelada localmente. O nome/contato do voluntário só vêm
      // numa nova busca; aqui sinalizamos que a identidade foi autorizada.
      setCorrespondenciaPorSolicitacao((atual) => ({
        ...atual,
        [correspondencia.solicitacaoId]: {
          ...atual[correspondencia.solicitacaoId],
          identidadeRevelada: true,
        },
      }));
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível revelar sua identidade.');
    } finally {
      setRevelandoId(null);
    }
  }

  return (
    <main className="minhas-solicitacoes">
      <header className="minhas-solicitacoes__cabecalho">
        <div>
          <h1 className="minhas-solicitacoes__titulo">Minhas solicitações</h1>
          <p className="minhas-solicitacoes__subtitulo">
            Acompanhe o andamento dos apoios que você pediu.
          </p>
        </div>
        <Link className="minhas-solicitacoes__nova" to="/solidariedade/solicitar">
          Pedir apoio
        </Link>
      </header>

      {carregando && (
        <p className="minhas-solicitacoes__status-msg" role="status">Carregando…</p>
      )}

      {erro && !carregando && (
        <p className="minhas-solicitacoes__erro" role="alert">{erro}</p>
      )}

      {!carregando && !erro && solicitacoes.length === 0 && (
        <p className="minhas-solicitacoes__vazio">
          Você ainda não tem solicitações. Que tal <Link to="/solidariedade/solicitar">pedir apoio</Link>?
        </p>
      )}

      {!carregando && !erro && solicitacoes.length > 0 && (
        <ul className="minhas-solicitacoes__lista">
          {solicitacoes.map((s) => {
            const correspondencia = correspondenciaPorSolicitacao[s.id];
            const temVoluntarioAceito =
              correspondencia && correspondencia.status === CORRESPONDENCIA_ACEITA;

            return (
              <li key={s.id} className="minhas-solicitacoes__item">
                <div className="minhas-solicitacoes__item-topo">
                  <span className="minhas-solicitacoes__categoria">{rotuloCategoria(s.categoria)}</span>
                  <span className={classeStatus(s.status)}>{rotuloStatusSolicitacao(s.status)}</span>
                </div>
                <p className="minhas-solicitacoes__descricao">{s.descricao}</p>
                <div className="minhas-solicitacoes__item-base">
                  <span className="minhas-solicitacoes__codigo">{s.codigoPublico}</span>
                  <span className="minhas-solicitacoes__data">{formatarData(s.createdAt)}</span>
                </div>

                {/* Correspondência: um voluntário aceitou ajudar nesta solicitação. */}
                {temVoluntarioAceito && (
                  <div className="minhas-solicitacoes__match">
                    {correspondencia.identidadeRevelada ? (
                      <p className="minhas-solicitacoes__match-info">
                        Identidade revelada ao voluntário{' '}
                        <strong>{correspondencia.nomeVoluntario || correspondencia.codigoPublicoVoluntario}</strong>
                        {correspondencia.contatoVoluntario ? ` · ${correspondencia.contatoVoluntario}` : ''}.
                      </p>
                    ) : (
                      <>
                        <p className="minhas-solicitacoes__match-info">
                          O voluntário <strong>{correspondencia.codigoPublicoVoluntario}</strong> aceitou
                          ajudar. Para combinar os detalhes, você pode revelar sua identidade a ele.
                        </p>
                        <button
                          type="button"
                          className="minhas-solicitacoes__revelar"
                          onClick={() => aoRevelarIdentidade(correspondencia)}
                          disabled={revelandoId === correspondencia.id}
                        >
                          {revelandoId === correspondencia.id ? 'Revelando…' : 'Revelar minha identidade'}
                        </button>
                      </>
                    )}
                  </div>
                )}

                {podeCancelar(s.status) && (
                  <div className="minhas-solicitacoes__item-acoes">
                    <button
                      type="button"
                      className="minhas-solicitacoes__cancelar"
                      onClick={() => aoCancelar(s.id)}
                      disabled={cancelandoId === s.id}
                    >
                      {cancelandoId === s.id ? 'Cancelando…' : 'Cancelar solicitação'}
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
