// Página de perfil do usuário autenticado (BRV-115).
//
// Responsabilidades:
//  - Buscar os dados do usuário por GET /api/perfil.
//  - Exibir nome, e-mail, telefone, foto, papel e indicadores de perfil.
//  - Permitir a edição do nome e telefone.
//  - Validar os campos antes do envio.
//  - Enviar as alterações por PUT /api/perfil.
//  - Exibir estados de carregamento, erro, salvamento e sucesso.
//  - Ações (coluna direita): tornar-se voluntário (com pausar/retomar atuação)
//    e resumo das últimas compras (BRV-89).
//
// Layout: blocos independentes (cartão de dados à esquerda; cartões de ações à
// direita), sem um cartão único envolvendo tudo.
//
// O usuário é identificado pelo JWT enviado automaticamente pelo wrapper api.
// O e-mail institucional é somente para leitura.

import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api, ApiError } from '../services/api.js';
import './Perfil.css';

const formularioInicial = {
  nomeCompleto: '',
  telefone: '',
};
const TAMANHO_MAXIMO_FOTO = 3 * 1024 * 1024;

const TIPOS_DE_IMAGEM_PERMITIDOS = [
  'image/jpeg',
  'image/png',
  'image/webp',
];

// Resumo de pedidos no Perfil: mesmas convenções da tela /pedidos (BRV-89).
const QTD_RESUMO_PEDIDOS = 3;

const STATUS_PEDIDO = {
  0: 'Reservado',
  1: 'Confirmado',
  2: 'Concluído',
  3: 'Cancelado',
  4: 'Expirado',
};

const DESCRICAO_VOLUNTARIO =
  'Como voluntário, você pode se oferecer para atender solicitações de apoio da ' +
  'comunidade — aulas de reforço, doações de roupas e alimentos, material acadêmico ' +
  'e mais. Você escolhe suas áreas de atuação e só recebe sugestões compatíveis.';

function obterSomenteDigitos(valor) {
  return valor.replace(/\D/g, '').slice(0, 11);
}

function formatarTelefone(valor) {
  const digitos = obterSomenteDigitos(valor);

  if (!digitos) {
    return '';
  }

  if (digitos.length <= 2) {
    return `(${digitos}`;
  }

  const ddd = digitos.slice(0, 2);
  const numero = digitos.slice(2);

  if (numero.length <= 4) {
    return `(${ddd}) ${numero}`;
  }

  if (digitos.length <= 10) {
    return `(${ddd}) ${numero.slice(0, 4)}-${numero.slice(4)}`;
  }

  return `(${ddd}) ${numero.slice(0, 5)}-${numero.slice(5)}`;
}

function formatarMoeda(valor) {
  return Number(valor).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' });
}

function formatarDataCurta(iso) {
  return new Date(iso).toLocaleDateString('pt-BR', { dateStyle: 'short' });
}

// Resumo compacto dos itens de um pedido: "2x Brownie" + "+N item(ns)" se houver mais.
function resumirItens(itens) {
  if (!itens || itens.length === 0) return 'Pedido sem itens';

  const primeiro = itens[0];
  const base = `${primeiro.quantidade}x ${primeiro.produtoNome}`;
  const extras = itens.length - 1;

  if (extras <= 0) return base;
  return `${base} +${extras} ${extras === 1 ? 'item' : 'itens'}`;
}

function validarFormulario(formulario) {
  const nome = formulario.nomeCompleto.trim();

  if (nome.length < 2 || nome.length > 150) {
    return 'O nome deve ter entre 2 e 150 caracteres.';
  }

  return '';
}

export default function Perfil() {
  const [perfil, setPerfil] = useState(null);
  const [formulario, setFormulario] = useState(formularioInicial);
  const [carregando, setCarregando] = useState(true);
  const [salvando, setSalvando] = useState(false);
  const [erro, setErro] = useState('');
  const [mensagemSucesso, setMensagemSucesso] = useState('');
  const [fotoSelecionada, setFotoSelecionada] = useState(null);
  const [previewFoto, setPreviewFoto] = useState('');
  const [tornandoVoluntario, setTornandoVoluntario] = useState(false);
  const [descricaoAberta, setDescricaoAberta] = useState(false);
  const [voluntarioDisponivel, setVoluntarioDisponivel] = useState(null);
  const [alterandoDisponibilidade, setAlterandoDisponibilidade] = useState(false);
  const [resumoPedidos, setResumoPedidos] = useState([]);
  const [carregandoPedidos, setCarregandoPedidos] = useState(true);

  useEffect(() => {
    async function carregarPerfil() {
      setErro('');

      try {
        const dados = await api.get('/api/perfil');

        setPerfil(dados);
        setFormulario({
          nomeCompleto: dados.nomeCompleto ?? '',
          telefone: dados.telefone ?? '',
        });
      } catch (e) {
        if (e instanceof ApiError) {
          setErro(e.message);
        } else {
          setErro('Não foi possível carregar o perfil.');
        }
      } finally {
        setCarregando(false);
      }
    }

    carregarPerfil();
  }, []);

  // Resumo das últimas compras.
  useEffect(() => {
    let cancelado = false;

    api
      .get(`/api/pedidos?page=1&pageSize=${QTD_RESUMO_PEDIDOS}`)
      .then((resposta) => {
        if (!cancelado) setResumoPedidos(resposta?.items ?? []);
      })
      .catch(() => {
        // Falha no resumo não deve travar o perfil inteiro — só mostra vazio.
        if (!cancelado) setResumoPedidos([]);
      })
      .finally(() => {
        if (!cancelado) setCarregandoPedidos(false);
      });

    return () => {
      cancelado = true;
    };
  }, []);

  // Status de disponibilidade do voluntário (ativo/pausado). O GET /api/perfil
  // não traz isso; buscamos no endpoint dedicado só quando o usuário é voluntário.
  useEffect(() => {
    if (!perfil?.ehVoluntario) {
      setVoluntarioDisponivel(null);
      return;
    }

    let cancelado = false;
    api
      .get('/api/perfil/voluntario')
      .then((dados) => {
        if (!cancelado) setVoluntarioDisponivel(dados?.disponivel ?? true);
      })
      .catch(() => {
        if (!cancelado) setVoluntarioDisponivel(true); // assume ativo se não conseguir ler
      });

    return () => {
      cancelado = true;
    };
  }, [perfil?.ehVoluntario]);

  function selecionarFoto(evento) {
    const arquivo = evento.target.files?.[0];

    if (!arquivo) {
      return;
    }

    if (!TIPOS_DE_IMAGEM_PERMITIDOS.includes(arquivo.type)) {
      setErro('Formato não permitido. Use JPG, JPEG, PNG ou WEBP.');
      setFotoSelecionada(null);
      setPreviewFoto('');
      evento.target.value = '';
      return;
    }

    if (arquivo.size > TAMANHO_MAXIMO_FOTO) {
      setErro('A imagem deve ter no máximo 3 MB.');
      setFotoSelecionada(null);
      setPreviewFoto('');
      evento.target.value = '';
      return;
    }

    setErro('');
    setMensagemSucesso('');
    setFotoSelecionada(arquivo);
    setPreviewFoto(URL.createObjectURL(arquivo));
  }

  function alterarCampo(evento) {
    const { name, value } = evento.target;

    setFormulario((estadoAtual) => ({
      ...estadoAtual,
      [name]:
        name === 'telefone' ? obterSomenteDigitos(value) : value,
    }));

    setMensagemSucesso('');
    setErro('');
  }

  async function salvarPerfil(evento) {
    evento.preventDefault();

    const mensagemErro = validarFormulario(formulario);

    if (mensagemErro) {
      setErro(mensagemErro);
      setMensagemSucesso('');
      return;
    }

    setErro('');
    setMensagemSucesso('');
    setSalvando(true);

    try {
      let fotoUrl = perfil?.fotoPerfilUrl ?? null;

      if (fotoSelecionada) {
        const dadosImagem = new FormData();
        dadosImagem.append('arquivo', fotoSelecionada);

        const respostaUpload = await api.post(
          '/api/upload/imagem?pasta=perfis',
          dadosImagem,
        );

        fotoUrl = respostaUpload?.url ?? respostaUpload?.Url;

        if (!fotoUrl) {
          throw new Error('A API não retornou a URL da imagem.');
        }
      }

      const dadosAtualizados = {
        nomeCompleto: formulario.nomeCompleto.trim(),
        telefone: obterSomenteDigitos(formulario.telefone),
        fotoUrl,
      };

      await api.put('/api/perfil', dadosAtualizados);

      const perfilAtualizado = {
        ...perfil,
        nomeCompleto: dadosAtualizados.nomeCompleto,
        telefone: dadosAtualizados.telefone,
        fotoPerfilUrl: dadosAtualizados.fotoUrl,
      };

      setPerfil(perfilAtualizado);
      setFormulario({
        nomeCompleto: perfilAtualizado.nomeCompleto,
        telefone: perfilAtualizado.telefone,
      });
      setMensagemSucesso('Perfil atualizado com sucesso.');
      setFotoSelecionada(null);
      setPreviewFoto('');
    } catch (e) {
      if (e instanceof ApiError) {
        setErro(e.message);
      } else if (e instanceof Error) {
        setErro(e.message);
      } else {
        setErro('Não foi possível atualizar o perfil.');
      }
    } finally {
      setSalvando(false);
    }
  }

  async function tornarSeVoluntario() {
    setErro('');
    setMensagemSucesso('');
    setTornandoVoluntario(true);

    try {
      await api.post('/api/perfil/voluntario');
      setPerfil((atual) => ({ ...atual, ehVoluntario: true }));
      setVoluntarioDisponivel(true);
      setMensagemSucesso('Agora você é um voluntário! 🤝');
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível concluir. Tente novamente.');
    } finally {
      setTornandoVoluntario(false);
    }
  }

  async function alternarDisponibilidade() {
    const vaiPausar = voluntarioDisponivel; // se está disponível, a ação é pausar
    setErro('');
    setMensagemSucesso('');
    setAlterandoDisponibilidade(true);

    try {
      const rota = vaiPausar
        ? '/api/perfil/voluntario/pausar'
        : '/api/perfil/voluntario/retomar';

      await api.patch(rota);
      setVoluntarioDisponivel(!vaiPausar);
      setMensagemSucesso(vaiPausar ? 'Atuação pausada.' : 'Atuação retomada.');
    } catch (e) {
      setErro(e instanceof ApiError ? e.message : 'Não foi possível alterar. Tente novamente.');
    } finally {
      setAlterandoDisponibilidade(false);
    }
  }

  if (carregando) {
    return (
      <main className="perfil">
        <p className="perfil__status">Carregando perfil...</p>
      </main>
    );
  }

  if (erro && !perfil) {
    return (
      <main className="perfil">
        <p className="perfil__mensagem perfil__mensagem--erro" role="alert">
          {erro}
        </p>
      </main>
    );
  }

  return (
    <main className="perfil">
      <div className="perfil__grade">
        {/* ===== BLOCO ESQUERDA — cartão de dados do perfil ===== */}
        <section className="perfil__cartao perfil__cartao--dados" aria-labelledby="perfil-titulo">
          <header className="perfil__cabecalho">
            <div className="perfil__foto-container">
              {previewFoto || perfil?.fotoPerfilUrl ? (
                <img
                  className="perfil__foto"
                  src={previewFoto || perfil.fotoPerfilUrl}
                  alt={`Foto de perfil de ${perfil.nomeCompleto}`}
                />
              ) : (
                <div className="perfil__foto perfil__foto--vazia" aria-hidden="true">
                  {perfil?.nomeCompleto?.charAt(0).toUpperCase() || '?'}
                </div>
              )}
            </div>

            <div>
              <h1 id="perfil-titulo" className="perfil__titulo">
                Meu perfil
              </h1>
              <p className="perfil__subtitulo">
                Consulte e atualize seus dados pessoais.
              </p>
            </div>
          </header>

          {erro && (
            <p className="perfil__mensagem perfil__mensagem--erro" role="alert">
              {erro}
            </p>
          )}

          {mensagemSucesso && (
            <p className="perfil__mensagem perfil__mensagem--sucesso" role="status">
              {mensagemSucesso}
            </p>
          )}

          <form className="perfil__formulario" onSubmit={salvarPerfil}>
            <div className="perfil__campo">
              <label htmlFor="fotoPerfil">Foto de perfil</label>
              <input
                id="fotoPerfil"
                name="fotoPerfil"
                type="file"
                accept=".jpg,.jpeg,.png,.webp"
                onChange={selecionarFoto}
                disabled={salvando}
              />
              <small>Escolha uma imagem de até 3 MB.</small>
            </div>

            <div className="perfil__campo">
              <label htmlFor="nomeCompleto">Nome completo</label>
              <input
                id="nomeCompleto"
                name="nomeCompleto"
                type="text"
                value={formulario.nomeCompleto}
                onChange={alterarCampo}
                minLength={2}
                maxLength={150}
                required
                disabled={salvando}
              />
            </div>

            <div className="perfil__campo">
              <label htmlFor="emailInstitucional">E-mail institucional</label>
              <input
                id="emailInstitucional"
                type="email"
                value={perfil?.emailInstitucional ?? ''}
                readOnly
                disabled
              />
            </div>

            <div className="perfil__campo">
              <label htmlFor="telefone">Telefone</label>
              <input
                id="telefone"
                name="telefone"
                type="tel"
                value={formatarTelefone(formulario.telefone)}
                onChange={alterarCampo}
                onInvalid={(evento) => {
                  evento.currentTarget.setCustomValidity(
                    'Deixe em branco OU informe um telefone com DDD e 10/11 dígitos.',
                  );
                }}
                onInput={(evento) => {
                  evento.currentTarget.setCustomValidity('');
                }}
                placeholder="(11) 99999-9999"
                inputMode="numeric"
                maxLength={15}
                pattern="^$|^\(\d{2}\) \d{4,5}-\d{4}$"
                disabled={salvando}
              />

              <small>Opcional. Informe DDD e número.</small>
            </div>

            <div className="perfil__informacoes">
              <p>
                <strong>Papel:</strong> {perfil?.role ?? 'Não informado'}
              </p>

              <p>
                <strong>Vendedor:</strong>{' '}
                {perfil?.ehVendedor ? 'Sim' : 'Não'}
              </p>
            </div>

            <button
              className="perfil__botao"
              type="submit"
              disabled={salvando}
            >
              {salvando ? 'Salvando...' : 'Salvar alterações'}
            </button>
          </form>
        </section>

        {/* ===== BLOCO DIREITA — cartões de ações ===== */}
        <aside className="perfil__coluna perfil__coluna--acoes">
                  {/* Card: voluntário */}
          <section className="perfil__card" aria-label="Voluntariado">
            {perfil?.ehVoluntario ? (
              <>
                <div className="perfil__card-linha">
                  <p className="perfil__card-titulo">
                    Você é voluntário{' '}
                    {voluntarioDisponivel === null
                      ? ''
                      : voluntarioDisponivel
                      ? '· Ativo'
                      : '· Pausado'}
                  </p>
                </div>

                {voluntarioDisponivel !== null && (
                  <button
                    type="button"
                    className="perfil__botao perfil__botao--compacto"
                    onClick={alternarDisponibilidade}
                    disabled={alterandoDisponibilidade}
                  >
                    {alterandoDisponibilidade
                      ? 'Processando...'
                      : voluntarioDisponivel
                      ? 'Deixar de atuar'
                      : 'Voltar a atuar'}
                  </button>
                )}

                {voluntarioDisponivel === false && (
                  <p className="perfil__card-aviso">
                    Enquanto estiver fora de atuação, você não recebe novas sugestões
                    de apoio. As solicitações que você já aceitou continuam acessíveis.
                  </p>
                )}
              </>
            ) : (
              <div className="perfil__card-linha">
                <p className="perfil__card-titulo">Quer ser voluntário?</p>
                <button
                  type="button"
                  className="perfil__botao perfil__botao--compacto"
                  onClick={tornarSeVoluntario}
                  disabled={tornandoVoluntario}
                >
                  {tornandoVoluntario ? 'Processando...' : 'Ser voluntário'}
                </button>
              </div>
            )}

            {/* "Saiba mais" aparece nos dois casos (voluntário ou não) */}
            <button
              type="button"
              className="perfil__expandir"
              onClick={() => setDescricaoAberta((v) => !v)}
              aria-expanded={descricaoAberta}
            >
              {descricaoAberta ? 'Ocultar' : 'Saiba mais'} {descricaoAberta ? '▴' : '▾'}
            </button>

            {descricaoAberta && (
              <p className="perfil__card-descricao">{DESCRICAO_VOLUNTARIO}</p>
            )}
          </section>


          {/* Card: resumo de pedidos */}
          <section className="perfil__card" aria-label="Resumo de pedidos">
            <div className="perfil__card-linha">
              <p className="perfil__card-titulo">Últimas compras</p>
              <Link className="perfil__botao perfil__botao--compacto" to="/pedidos">
                Histórico
              </Link>
            </div>

            {carregandoPedidos && (
              <p className="perfil__card-vazio">Carregando…</p>
            )}

            {!carregandoPedidos && resumoPedidos.length === 0 && (
              <p className="perfil__card-vazio">Você ainda não fez pedidos.</p>
            )}

            {!carregandoPedidos && resumoPedidos.length > 0 && (
              <ul className="perfil__pedidos-lista">
                {resumoPedidos.map((pedido) => (
                  <li key={pedido.pedidoId} className="perfil__pedido-item">
                    <div className="perfil__pedido-topo">
                      <span className="perfil__pedido-produto" title={resumirItens(pedido.itens)}>
                        {resumirItens(pedido.itens)}
                      </span>
                      <span className={`perfil__pedido-status perfil__pedido-status--${pedido.status}`}>
                        {STATUS_PEDIDO[pedido.status] ?? 'Desconhecido'}
                      </span>
                    </div>
                    <div className="perfil__pedido-base">
                      <span className="perfil__pedido-data">
                        {formatarDataCurta(pedido.createdAt)}
                      </span>
                      <span className="perfil__pedido-total">
                        {formatarMoeda(pedido.total)}
                      </span>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </aside>
      </div>
    </main>
  );
}
