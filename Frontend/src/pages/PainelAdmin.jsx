// Painel Admin de Moderação (BRV-232, issue #115, frente B3).
//
// Tela restrita a administradores (guard <SomenteAdmin> na rota + backend com
// [Authorize(Roles="Admin")]). Lista denúncias com nomes já resolvidos e oferece,
// por item: mudar o status da denúncia, suspender/reativar o usuário denunciado e
// inativar o produto denunciado (quando houver produtoId).
//
// Consome só o service de moderação (services/moderacao.js). As denúncias vêm de
// um MOCK por enquanto (backend da B2 ainda não existe); as ações de
// usuário/produto chamam endpoints reais. A página NÃO muda quando a B2 entrar.

import { useEffect, useRef, useState } from 'react';
import { ApiError } from '../services/api.js';
import {
  listarDenuncias,
  alterarStatusDenuncia,
  inativarProduto,
  suspenderUsuario,
  reativarUsuario,
} from '../services/moderacao.js';
import {
  rotuloStatusDenuncia,
  rotuloMotivoDenuncia,
  OPCOES_STATUS_DENUNCIA,
} from '../services/rotulosDenuncia.js';
import './PainelAdmin.css';

const formatadorData = new Intl.DateTimeFormat('pt-BR', {
  day: '2-digit',
  month: '2-digit',
  year: 'numeric',
  hour: '2-digit',
  minute: '2-digit',
});

function formatarData(iso) {
  const data = new Date(iso);
  return Number.isNaN(data.getTime()) ? '—' : formatadorData.format(data);
}

// Mensagem de erro ramificada por ApiError.status (ênfase da issue #115).
function mensagemErro(e) {
  if (e instanceof ApiError) {
    if (e.status === 403)
      return 'Sem permissão. Seu acesso de administrador pode ter expirado — reentre na conta e tente de novo.';
    if (e.status === 409)
      return e.message || 'Essa transição de status não é permitida.';
    if (e.status === 404) return 'Recurso não encontrado. A lista pode estar desatualizada.';
    return e.message || 'Não foi possível concluir a ação. Tente novamente.';
  }
  return 'Não foi possível concluir a ação. Tente novamente.';
}

// Card de uma denúncia, com as ações e o feedback isolados por item.
function CartaoDenuncia({ denuncia, aoAtualizar }) {
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState('');
  const [statusSelecionado, setStatusSelecionado] = useState(Number(denuncia.status));
  const emCurso = useRef(false);

  // Executa uma ação com confirmação + anti-duplo-clique, no molde do
  // BotaoExcluirItemBazar: window.confirm antes, useRef para travar reenvio,
  // desabilita enquanto envia e trata o erro por ação.
  async function executar(confirmacao, acao, mensagemSucesso) {
    if (emCurso.current || enviando) return;
    if (!window.confirm(confirmacao)) return;
    emCurso.current = true;
    setEnviando(true);
    setErro('');
    setSucesso('');
    try {
      await acao();
    } catch (e) {
      setErro(mensagemErro(e));
      emCurso.current = false;
      setEnviando(false);
      return;
    }
    setSucesso(mensagemSucesso);
    emCurso.current = false;
    setEnviando(false);
    aoAtualizar?.();
  }

  const statusMudou = statusSelecionado !== Number(denuncia.status);

  return (
    <li className="painel-admin__item">
      <div className="painel-admin__info">
        <div className="painel-admin__topo">
          <span className={`painel-admin__status painel-admin__status--${denuncia.status}`}>
            {rotuloStatusDenuncia(denuncia.status)}
          </span>
          <span className="painel-admin__motivo">{rotuloMotivoDenuncia(denuncia.motivo)}</span>
          <span className="painel-admin__data">{formatarData(denuncia.criadaEm)}</span>
        </div>

        <dl className="painel-admin__pessoas">
          <div>
            <dt>Denunciante</dt>
            <dd>{denuncia.denuncianteNome}</dd>
          </div>
          <div>
            <dt>Denunciado</dt>
            <dd>{denuncia.denunciadoNome}</dd>
          </div>
          {denuncia.produtoId && (
            <div>
              <dt>Produto</dt>
              <dd>{denuncia.produtoNome ?? '—'}</dd>
            </div>
          )}
        </dl>

        <p className="painel-admin__descricao">{denuncia.descricao}</p>
      </div>

      <div className="painel-admin__acoes">
        <div className="painel-admin__acao painel-admin__acao--status">
          <label htmlFor={`status-${denuncia.id}`}>Status da denúncia</label>
          <div className="painel-admin__status-controle">
            <select
              id={`status-${denuncia.id}`}
              value={statusSelecionado}
              disabled={enviando}
              onChange={(e) => setStatusSelecionado(Number(e.target.value))}
            >
              {OPCOES_STATUS_DENUNCIA.map((o) => (
                <option key={o.valor} value={o.valor}>
                  {o.rotulo}
                </option>
              ))}
            </select>
            <button
              type="button"
              disabled={enviando || !statusMudou}
              onClick={() =>
                executar(
                  `Alterar o status desta denúncia para “${rotuloStatusDenuncia(statusSelecionado)}”?`,
                  () => alterarStatusDenuncia(denuncia.id, statusSelecionado),
                  'Status atualizado.',
                )
              }
            >
              Salvar status
            </button>
          </div>
        </div>

        <div className="painel-admin__acao painel-admin__acao--usuario">
          <span className="painel-admin__acao-titulo">Usuário denunciado</span>
          <div className="painel-admin__botoes">
            <button
              type="button"
              disabled={enviando}
              onClick={() =>
                executar(
                  `Suspender a conta de “${denuncia.denunciadoNome}”?`,
                  () => suspenderUsuario(denuncia.denunciadoId),
                  'Usuário suspenso.',
                )
              }
            >
              Suspender
            </button>
            <button
              type="button"
              disabled={enviando}
              onClick={() =>
                executar(
                  `Reativar a conta de “${denuncia.denunciadoNome}”?`,
                  () => reativarUsuario(denuncia.denunciadoId),
                  'Usuário reativado.',
                )
              }
            >
              Reativar
            </button>
          </div>
        </div>

        {denuncia.produtoId && (
          <div className="painel-admin__acao painel-admin__acao--produto">
            <span className="painel-admin__acao-titulo">Produto denunciado</span>
            <button
              type="button"
              disabled={enviando}
              onClick={() =>
                executar(
                  `Inativar o produto “${denuncia.produtoNome ?? ''}”? Ele deixará de aparecer nas buscas.`,
                  () => inativarProduto(denuncia.produtoId),
                  'Produto inativado.',
                )
              }
            >
              Inativar produto
            </button>
          </div>
        )}

        {erro && (
          <p className="painel-admin__feedback" role="alert">
            {erro}
          </p>
        )}
        {sucesso && (
          <p className="painel-admin__feedback painel-admin__feedback--ok" role="status">
            {sucesso}
          </p>
        )}
      </div>
    </li>
  );
}

export default function PainelAdmin() {
  const [denuncias, setDenuncias] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [recarregar, setRecarregar] = useState(0);

  useEffect(() => {
    let ativo = true;

    async function carregar() {
      if (recarregar === 0) setCarregando(true);
      setErro('');
      try {
        const dados = await listarDenuncias();
        if (ativo) setDenuncias(dados);
      } catch (e) {
        if (!ativo) return;
        setErro(
          e instanceof ApiError
            ? e.message
            : 'Não foi possível carregar as denúncias. Tente novamente.',
        );
      } finally {
        if (ativo) setCarregando(false);
      }
    }

    carregar();
    return () => {
      ativo = false;
    };
  }, [recarregar]);

  // Após uma ação bem-sucedida, recarrega a lista para refletir sem F5.
  function aoAtualizar() {
    setRecarregar((n) => n + 1);
  }

  return (
    <section className="painel-admin" aria-labelledby="painel-admin-titulo">
      <header className="painel-admin__cabecalho">
        <h1 id="painel-admin-titulo">Moderação</h1>
        <p>Analise as denúncias e tome as ações necessárias sobre usuários e produtos.</p>
      </header>

      {carregando && (
        <p className="painel-admin__estado" role="status">
          Carregando denúncias…
        </p>
      )}

      {!carregando && erro && (
        <p className="painel-admin__estado painel-admin__estado--erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && !erro && denuncias.length === 0 && (
        <p className="painel-admin__estado">Nenhuma denúncia para moderar no momento.</p>
      )}

      {!carregando && !erro && denuncias.length > 0 && (
        <ul className="painel-admin__lista">
          {denuncias.map((denuncia) => (
            <CartaoDenuncia key={denuncia.id} denuncia={denuncia} aoAtualizar={aoAtualizar} />
          ))}
        </ul>
      )}
    </section>
  );
}
