// Minhas doações (Ação Solidária — Task 5).
//
// Duas funções:
//  - Listar as doações do voluntário logado (GET /api/doacoes/minhas) e
//    gerenciá-las (confirmar entrega / cancelar).
//  - Registrar uma nova doação a partir de uma correspondência aceita. O link
//    da tela "Minhas correspondências" traz ?solicitacao=<correspondenciaId>;
//    enviamos esse id como `correspondenciaId` e o backend resolve a solicitação
//    (o front nunca manuseia o id da solicitação — LGPD).
//
// ⚠️ LGPD: a resposta só traz o CodigoPublicoSolicitacao — exibir apenas isso.

import { useEffect, useState } from 'react';
import { Link, useSearchParams } from 'react-router-dom';

import { ApiError } from '../../../services/api.js';
import {
  listarMinhasDoacoes,
  criarDoacao,
  confirmarEntregaDoacao,
  cancelarDoacao,
} from '../../../services/acaoSolidaria.js';
import { rotuloStatusDoacao } from '../../../services/rotulosSolidariedade.js';
import './Doacoes.css';

// StatusDoacao: 0 = Pendente (quando confirmar entrega / cancelar fazem sentido).
const STATUS_PENDENTE = 0;

function formatarData(iso) {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString('pt-BR', { dateStyle: 'short' });
}

export default function Doacoes() {
  const [searchParams] = useSearchParams();
  // id da correspondência vindo do link "Registrar doação" (tela de correspondências).
  const correspondenciaId = searchParams.get('solicitacao') || '';

  const [doacoes, setDoacoes] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');
  const [mensagem, setMensagem] = useState('');
  const [acaoId, setAcaoId] = useState(null);
  const [gatilho, setGatilho] = useState(0);

  // Form de registro (aparece quando há uma correspondência no link).
  const [descricao, setDescricao] = useState('');
  const [registrando, setRegistrando] = useState(false);

  useEffect(() => {
    let cancelado = false;

    async function carregar() {
      setCarregando(true);
      setErro('');
      try {
        const lista = await listarMinhasDoacoes();
        if (cancelado) return;
        setDoacoes(Array.isArray(lista) ? lista : []);
      } catch (e) {
        if (cancelado) return;
        if (e instanceof ApiError && e.status === 400) {
          setErro('Você precisa ser voluntário para ver suas doações.');
        } else if (e instanceof ApiError && e.status === 401) {
          setErro('Sessão expirada. Entre novamente.');
        } else {
          setErro('Não foi possível carregar suas doações.');
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

  async function aoRegistrar(evento) {
    evento.preventDefault();
    setErro('');
    setMensagem('');
    setRegistrando(true);
    try {
      await criarDoacao({ correspondenciaId, descricao: descricao.trim() });
      setMensagem('Doação registrada com sucesso.');
      setDescricao('');
      setGatilho((n) => n + 1);
    } catch (e) {
      // 409 = correspondência não aceita / não é do voluntário.
      setErro(e instanceof ApiError ? e.message : 'Não foi possível registrar a doação.');
    } finally {
      setRegistrando(false);
    }
  }

  async function executarAcao(id, acao, textoSucesso) {
    setErro('');
    setMensagem('');
    setAcaoId(id);
    try {
      await acao(id);
      setMensagem(textoSucesso);
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível concluir a ação.');
    } finally {
      setAcaoId(null);
      setGatilho((n) => n + 1);
    }
  }

  return (
    <main className="doacoes">
      <header className="doacoes__topo">
        <h1>Minhas doações</h1>
        <Link to="/solidariedade/voluntario" className="doacoes__voltar">
          ← Área do voluntário
        </Link>
      </header>

      {/* Form de registro: só quando veio de uma correspondência (link da Task 4). */}
      {correspondenciaId && (
        <form className="doacoes__form" onSubmit={aoRegistrar}>
          <h2>Registrar doação</h2>
          <p className="doacoes__form-texto">
            Descreva o que você vai doar para a correspondência que aceitou.
          </p>
          <label className="doacoes__campo">
            <span>Descrição</span>
            <textarea
              value={descricao}
              onChange={(e) => setDescricao(e.target.value)}
              minLength={3}
              maxLength={500}
              required
              rows={3}
              placeholder="Ex.: 2 kg de arroz e 1 pacote de feijão."
              disabled={registrando}
            />
          </label>
          <button type="submit" className="doacoes__botao" disabled={registrando}>
            {registrando ? 'Registrando…' : 'Registrar doação'}
          </button>
        </form>
      )}

      {carregando && <p className="doacoes__aviso">Carregando…</p>}

      {!carregando && erro && (
        <p className="doacoes__aviso doacoes__aviso--erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && mensagem && (
        <p className="doacoes__aviso doacoes__aviso--ok" role="status">
          {mensagem}
        </p>
      )}

      {!carregando && !erro && doacoes.length === 0 && (
        <p className="doacoes__aviso">
          Você ainda não registrou doações. Elas partem de uma correspondência aceita em{' '}
          <Link to="/solidariedade/correspondencias">Minhas correspondências</Link>.
        </p>
      )}

      {!carregando && !erro && doacoes.length > 0 && (
        <ul className="doacoes__lista">
          {doacoes.map((d) => {
            const pendente = d.status === STATUS_PENDENTE;
            return (
              <li key={d.id} className="doacao">
                <div className="doacao__cabecalho">
                  <span className="doacao__codigo" title="Identificação anônima">
                    Solicitação {d.codigoPublicoSolicitacao}
                  </span>
                  <span className={`doacao__status doacao__status--${d.status}`}>
                    {rotuloStatusDoacao(d.status)}
                  </span>
                </div>

                <p className="doacao__descricao">{d.descricao}</p>

                <div className="doacao__base">
                  <span className="doacao__data">
                    Registrada em {formatarData(d.createdAt)}
                    {d.entregueEm && ` · Entregue em ${formatarData(d.entregueEm)}`}
                  </span>
                  {pendente && (
                    <div className="doacao__acoes">
                      <button
                        type="button"
                        className="doacao__botao"
                        disabled={acaoId === d.id}
                        onClick={() =>
                          executarAcao(d.id, confirmarEntregaDoacao, 'Entrega confirmada.')
                        }
                      >
                        {acaoId === d.id ? 'Processando…' : 'Confirmar entrega'}
                      </button>
                      <button
                        type="button"
                        className="doacao__botao doacao__botao--perigo"
                        disabled={acaoId === d.id}
                        onClick={() => executarAcao(d.id, cancelarDoacao, 'Doação cancelada.')}
                      >
                        Cancelar
                      </button>
                    </div>
                  )}
                </div>
              </li>
            );
          })}
        </ul>
      )}
    </main>
  );
}
