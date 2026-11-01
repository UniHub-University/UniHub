// Tela inicial de boas-vindas do UniHub (BRV-116).
//
// Layout em duas colunas:
//  - Esquerda (hero): marca, tagline, proposta de valor e os 3 módulos.
//  - Direita (ação): cartão de login com o botão do Google.
// No mobile, as colunas se empilham (hero em cima, login embaixo).
//
// IMPORTANTE: o redesign é puramente visual. A LÓGICA de login é idêntica à
// versão anterior — mesmo fluxo, mesmo contrato com a API:
//  1. O usuário clica no botão do Google (<GoogleLogin>).
//  2. O Google devolve um "credential" = ID token (JWT do Google).
//  3. Enviamos esse idToken para POST /api/auth/google-login.
//  4. A API valida o e-mail @unifesp.br e devolve o JWT do UniHub, que guardamos
//     (via AuthContext/authStorage) antes de redirecionar.

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { GoogleLogin } from '@react-oauth/google';
import {
  FaBowlFood,
  FaBookOpen,
  FaHandHoldingHeart,
  FaGraduationCap,
} from 'react-icons/fa6';

import { api, ApiError } from '../services/api.js';
import { useAuth } from '../auth/AuthContext.jsx';
import logoUniHub from '../assets/logo-unihub.png';
import './BoasVindas.css';

import { useTituloPagina } from '../hooks/useTituloPagina.js';

// Módulos exibidos no hero. Centralizados numa constante para facilitar manutenção.
const MODULOS = [
  {
    icone: FaBowlFood,
    titulo: 'Alimentação',
    descricao: 'Reserve lanches e evite filas no intervalo.',
  },
  {
    icone: FaBookOpen,
    titulo: 'Bazar Acadêmico',
    descricao: 'Compre e venda materiais entre estudantes.',
  },
  {
    icone: FaHandHoldingHeart,
    titulo: 'Ação Solidária',
    descricao: 'Conecte-se a voluntários com privacidade.',
  },
];

export default function BoasVindas() {
  const navigate = useNavigate();
  const { login } = useAuth();

  // Título de marca na aba (texto vazio => usa o título completo do UniHub).
  useTituloPagina('');

  const [carregando, setCarregando] = useState(false);
  const [erro, setErro] = useState('');

  async function aoLogarComGoogle(credentialResponse) {
    const idToken = credentialResponse?.credential;
    if (!idToken) {
      setErro('Não foi possível obter as credenciais do Google. Tente novamente.');
      return;
    }

    setErro('');
    setCarregando(true);

    try {
      // A rota de login é pública — não envia Authorization.
      const dados = await api.post(
        '/api/auth/google-login',
        { idToken },
        { autenticado: false },
      );

      const token = dados?.token ?? dados?.Token;
      const nome = dados?.usuario ?? dados?.Usuario ?? '';

      if (!token) {
        setErro('A resposta do servidor não trouxe um token válido.');
        return;
      }

      login({ token, usuario: { nome } });
      navigate('/home', { replace: true });
    } catch (e) {
      if (e instanceof ApiError) {
        // 401: e-mail não @unifesp.br ou conta suspensa (mensagem vem da API).
        setErro(e.message);
      } else {
        setErro('Erro inesperado ao entrar. Tente novamente.');
      }
    } finally {
      setCarregando(false);
    }
  }

  function aoFalharLoginGoogle() {
    setErro('Falha ao autenticar com o Google. Tente novamente.');
  }

  return (
    <main className="boasvindas">
      {/* -------- Coluna esquerda: hero / marca -------- */}
      <section className="boasvindas__hero">
        <div className="boasvindas__hero-conteudo">
          <div className="boasvindas__marca">
            <img
              className="boasvindas__logo"
              src={logoUniHub}
              alt="UniHub"
              width={96}
              height={96}
            />
            <div>
              <h1 className="boasvindas__titulo">UniHub</h1>
              <p className="boasvindas__subtitulo">Troca e conexão para estudantes.</p>
            </div>
          </div>

          <p className="boasvindas__pitch">
            O ecossistema digital da UNIFESP que reúne, num só lugar, o que antes
            ficava espalhado em grupos de WhatsApp.
          </p>

          <ul className="boasvindas__modulos">
            {MODULOS.map(({ icone: Icone, titulo, descricao }) => (
              <li key={titulo} className="boasvindas__modulo">
                <span className="boasvindas__modulo-icone" aria-hidden="true">
                  <Icone />
                </span>
                <div>
                  <p className="boasvindas__modulo-titulo">{titulo}</p>
                  <p className="boasvindas__modulo-descricao">{descricao}</p>
                </div>
              </li>
            ))}
          </ul>
        </div>
      </section>

      {/* -------- Coluna direita: ação / login -------- */}
      <section className="boasvindas__painel">
        <div className="boasvindas__cartao">
          <span className="boasvindas__selo">
            <FaGraduationCap aria-hidden="true" /> Acesso institucional
          </span>

          <h2 className="boasvindas__cartao-titulo">Entre na plataforma</h2>
          <p className="boasvindas__cartao-texto">
            Use seu e-mail <strong>@unifesp.br</strong> para acessar todos os módulos.
          </p>

          <div className="boasvindas__login">
            <GoogleLogin
              onSuccess={aoLogarComGoogle}
              onError={aoFalharLoginGoogle}
              text="signin_with"
              shape="pill"
              locale="pt-BR"
              width="280"
            />
          </div>

          {carregando && (
            <p className="boasvindas__status" role="status">
              <span className="boasvindas__spinner" aria-hidden="true" />
              Entrando…
            </p>
          )}

          {erro && (
            <p className="boasvindas__erro" role="alert">
              {erro}
            </p>
          )}

          <p className="boasvindas__rodape">
            Acesso restrito à comunidade acadêmica da UNIFESP.
          </p>
        </div>
      </section>
    </main>
  );
}