// Área do voluntário (Ação Solidária — Task 1).
//
// Três estados:
//  - carregando
//  - NÃO é voluntário  -> CTA "Tornar-se voluntário"
//  - É voluntário      -> editor de áreas de atuação + pausar/retomar
//
// Consome o backend (pronto) via services/acaoSolidaria.js. Reusa as opções de
// categoria e os rótulos de rotulosSolidariedade.js. Sem dado sensível (é o
// próprio perfil do voluntário).

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { ApiError } from '../../../services/api.js';
import {
  obterMeuVoluntario,
  tornarSeVoluntario,
  definirAreas,
  pausarAtuacao,
  retomarAtuacao,
} from '../../../services/acaoSolidaria.js';
import { OPCOES_CATEGORIA } from '../../../services/rotulosSolidariedade.js';
import './AreaVoluntario.css';

export default function AreaVoluntario() {
  const [carregando, setCarregando] = useState(true);
  const [ehVoluntario, setEhVoluntario] = useState(false);
  const [disponivel, setDisponivel] = useState(true);
  const [areasSelecionadas, setAreasSelecionadas] = useState([]);

  const [salvando, setSalvando] = useState(false);
  const [acaoEmCurso, setAcaoEmCurso] = useState(false);
  const [erro, setErro] = useState('');
  const [mensagem, setMensagem] = useState('');
  // força o recarregamento do perfil após uma ação (tornar-se, pausar, etc.).
  const [gatilho, setGatilho] = useState(0);

  // Carrega o perfil de voluntário. O GET devolve 400 quando o usuário ainda
  // NÃO é voluntário — aqui isso é um estado normal (mostra o CTA), não um erro.
  useEffect(() => {
    let cancelado = false;

    // Toda a carga (inclusive o reset de "carregando"/erro) vive dentro de uma
    // função async: assim nenhum setState é chamado de forma síncrona no corpo
    // do effect, resolvendo a regra react-hooks/set-state-in-effect sem disable.
    async function carregar() {
      setCarregando(true);
      setErro('');
      try {
        const dados = await obterMeuVoluntario();
        if (cancelado) return;
        setEhVoluntario(true);
        setDisponivel(dados?.disponivel ?? true);
        setAreasSelecionadas(dados?.areasDeAtuacao ?? []);
      } catch (e) {
        if (cancelado) return;
        if (e instanceof ApiError && e.status === 400) {
          // Ainda não é voluntário: estado esperado.
          setEhVoluntario(false);
        } else if (e instanceof ApiError && e.status === 401) {
          setErro('Sessão expirada. Entre novamente.');
        } else {
          setErro('Não foi possível carregar seu perfil de voluntário.');
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

  async function aoTornarSeVoluntario() {
    setErro('');
    setMensagem('');
    setAcaoEmCurso(true);
    try {
      await tornarSeVoluntario();
      setMensagem('Agora você é voluntário! Defina suas áreas de atuação abaixo.');
      setGatilho((n) => n + 1);
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível concluir. Tente novamente.');
    } finally {
      setAcaoEmCurso(false);
    }
  }

  function alternarArea(valor) {
    setAreasSelecionadas((atual) =>
      atual.includes(valor) ? atual.filter((v) => v !== valor) : [...atual, valor],
    );
  }

  async function aoSalvarAreas() {
    setErro('');
    setMensagem('');
    setSalvando(true);
    try {
      await definirAreas(areasSelecionadas);
      setMensagem('Áreas de atuação atualizadas.');
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível salvar as áreas.');
    } finally {
      setSalvando(false);
    }
  }

  async function aoAlternarDisponibilidade() {
    setErro('');
    setMensagem('');
    setAcaoEmCurso(true);
    try {
      if (disponivel) {
        await pausarAtuacao();
        setDisponivel(false);
        setMensagem('Atuação pausada. Você não receberá novas sugestões.');
      } else {
        await retomarAtuacao();
        setDisponivel(true);
        setMensagem('Atuação retomada. Você voltará a receber sugestões.');
      }
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível alterar sua disponibilidade.');
    } finally {
      setAcaoEmCurso(false);
    }
  }

  return (
    <main className="area-voluntario">
      <header className="area-voluntario__topo">
        <h1>Área do voluntário</h1>
        <Link to="/solidariedade" className="area-voluntario__voltar">
          ← Ação Solidária
        </Link>
      </header>

      {carregando && <p className="area-voluntario__aviso">Carregando…</p>}

      {!carregando && erro && (
        <p className="area-voluntario__aviso area-voluntario__aviso--erro" role="alert">
          {erro}
        </p>
      )}

      {!carregando && mensagem && (
        <p className="area-voluntario__aviso area-voluntario__aviso--ok" role="status">
          {mensagem}
        </p>
      )}

      {/* ---------- Estado: ainda NÃO é voluntário ---------- */}
      {!carregando && !ehVoluntario && (
        <section className="area-voluntario__cartao">
          <p className="area-voluntario__texto">
            Como voluntário, você pode atender solicitações de apoio da comunidade —
            aulas de reforço, doações de roupas e alimentos, material acadêmico e mais.
            Você escolhe suas áreas e só recebe sugestões compatíveis.
          </p>
          <button
            type="button"
            className="area-voluntario__botao"
            disabled={acaoEmCurso}
            onClick={aoTornarSeVoluntario}
          >
            {acaoEmCurso ? 'Processando…' : 'Tornar-se voluntário'}
          </button>
        </section>
      )}

      {/* ---------- Estado: É voluntário ---------- */}
      {!carregando && ehVoluntario && (
        <>
          <section className="area-voluntario__cartao">
            <div className="area-voluntario__cabecalho-cartao">
              <h2>Suas áreas de atuação</h2>
              <span
                className={
                  disponivel
                    ? 'area-voluntario__selo area-voluntario__selo--ativo'
                    : 'area-voluntario__selo area-voluntario__selo--pausado'
                }
              >
                {disponivel ? 'Atuação ativa' : 'Atuação pausada'}
              </span>
            </div>

            <p className="area-voluntario__texto">
              Selecione as categorias em que você pode ajudar. Você recebe sugestões
              apenas das áreas marcadas.
            </p>

            <fieldset className="area-voluntario__areas">
              <legend className="area-voluntario__legenda">Categorias</legend>
              {OPCOES_CATEGORIA.map((opcao) => (
                <label key={opcao.valor} className="area-voluntario__opcao">
                  <input
                    type="checkbox"
                    checked={areasSelecionadas.includes(opcao.valor)}
                    onChange={() => alternarArea(opcao.valor)}
                    disabled={salvando}
                  />
                  <span>{opcao.rotulo}</span>
                </label>
              ))}
            </fieldset>

            {areasSelecionadas.length === 0 && (
              <p className="area-voluntario__dica">
                Sem áreas selecionadas, você não receberá sugestões.
              </p>
            )}

            <button
              type="button"
              className="area-voluntario__botao"
              disabled={salvando}
              onClick={aoSalvarAreas}
            >
              {salvando ? 'Salvando…' : 'Salvar áreas'}
            </button>
          </section>

          <section className="area-voluntario__cartao">
            <h2>Disponibilidade</h2>
            <p className="area-voluntario__texto">
              {disponivel
                ? 'Você está recebendo sugestões. Pause se precisar de uma pausa.'
                : 'Sua atuação está pausada. Retome para voltar a receber sugestões.'}
            </p>
            <button
              type="button"
              className="area-voluntario__botao area-voluntario__botao--secundario"
              disabled={acaoEmCurso}
              onClick={aoAlternarDisponibilidade}
            >
              {acaoEmCurso ? 'Processando…' : disponivel ? 'Pausar atuação' : 'Retomar atuação'}
            </button>
          </section>

          <nav className="area-voluntario__acessos" aria-label="Fluxo do voluntário">
            <Link to="/solidariedade/sugestoes" className="area-voluntario__link">
              Ver sugestões para você →
            </Link>
            <Link to="/solidariedade/correspondencias" className="area-voluntario__link">
              Minhas correspondências →
            </Link>
            <Link to="/solidariedade/doacoes" className="area-voluntario__link">
              Minhas doações →
            </Link>
          </nav>
        </>
      )}
    </main>
  );
}
