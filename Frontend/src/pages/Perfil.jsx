import { useEffect, useState } from 'react';
import { api, ApiError } from '../services/api.js';
import './Perfil.css';

const formularioInicial = {
  nomeCompleto: '',
  telefone: '',
};

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
      const dadosAtualizados = {
        nomeCompleto: formulario.nomeCompleto.trim(),
        telefone: obterSomenteDigitos(formulario.telefone),
        fotoUrl: perfil?.fotoPerfilUrl ?? null,
      };

      await api.put('/api/perfil', dadosAtualizados);

      const perfilAtualizado = {
        ...perfil,
        nomeCompleto: dadosAtualizados.nomeCompleto,
        telefone: dadosAtualizados.telefone,
      };

      setPerfil(perfilAtualizado);
      setFormulario({
        nomeCompleto: perfilAtualizado.nomeCompleto,
        telefone: perfilAtualizado.telefone,
      });
      setMensagemSucesso('Perfil atualizado com sucesso.');
    } catch (e) {
      if (e instanceof ApiError) {
        setErro(e.message);
      } else {
        setErro('Não foi possível atualizar o perfil.');
      }
    } finally {
      setSalvando(false);
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
      <section className="perfil__cartao" aria-labelledby="perfil-titulo">
        <header className="perfil__cabecalho">
          <div className="perfil__foto-container">
            {perfil?.fotoPerfilUrl ? (
              <img
                className="perfil__foto"
                src={perfil.fotoPerfilUrl}
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

            <p>
              <strong>Voluntário:</strong>{' '}
              {perfil?.ehVoluntario ? 'Sim' : 'Não'}
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
    </main>
  );
}
