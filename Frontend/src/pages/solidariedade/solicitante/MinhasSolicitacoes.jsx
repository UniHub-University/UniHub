// Tela "Minhas solicitações" do lado SOLICITANTE da Ação Solidária.
//
// Lista as solicitações do próprio usuário (GET /api/SolicitacaoApoio/minhas),
// em qualquer status, para ele acompanhar o andamento. Esta entrega cobre a
// listagem e a exibição de status; as ações (cancelar, revelar identidade)
// entram em commits seguintes.
//
// LGPD: a lista mostra apenas dados da própria solicitação (código público,
// categoria, descrição, status). Nenhuma identidade de terceiros é exposta.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { listarMinhasSolicitacoes } from '../../../services/acaoSolidaria.js';
import { rotuloCategoria, rotuloStatusSolicitacao } from '../../../services/rotulosSolidariedade.js';
import { ApiError } from '../../../services/api.js';
import './MinhasSolicitacoes.css';

function formatarData(iso) {
  return new Date(iso).toLocaleDateString('pt-BR', { dateStyle: 'short' });
}

// Mapeia o status (inteiro do enum) para uma classe de cor do selo.
function classeStatus(status) {
  return `minhas-solicitacoes__status minhas-solicitacoes__status--${status}`;
}

export default function MinhasSolicitacoes() {
  const [solicitacoes, setSolicitacoes] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    let cancelado = false;

    listarMinhasSolicitacoes()
      .then((dados) => {
        if (cancelado) return;
        setSolicitacoes(Array.isArray(dados) ? dados : []);
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
          {solicitacoes.map((s) => (
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
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
