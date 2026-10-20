// Tela inicial de boas-vindas do UniHub (BRV-116).
//
// Fluxo de login:
//  1. O usuário clica no botão do Google (<GoogleLogin>).
//  2. O Google devolve um "credential" = ID token (JWT do Google).
//  3. Enviamos esse idToken para POST /api/auth/google-login.
//  4. A API valida que o e-mail é @unifesp.br, cria/retoma o usuário e devolve
//     o JWT do UniHub. Guardamos esse JWT (via AuthContext/authStorage) e
//     redirecionamos para uma rota protegida.
//
// Observações:
//  - A API espera o corpo { idToken: "..." }.
//  - O campo do token na resposta pode vir como "token" (camelCase) ou "Token";
//    tratamos ambos para não quebrar caso a serialização do backend mude.

import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { GoogleLogin } from '@react-oauth/google';

import { api, ApiError } from '../services/api.js';
import { useAuth } from '../auth/AuthContext.jsx';
import logoUniHub from '../assets/logo-unihub.png';
import './BoasVindas.css';

export default function BoasVindas() {
  const navigate = useNavigate();
  const { login } = useAuth();

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
      <section className="boasvindas__cartao">
        <img
          className="boasvindas__logo"
          src={logoUniHub}
          alt="UniHub"
          width={140}
          height={140}
        />

        <h1 className="boasvindas__titulo">UniHub</h1>
        <p className="boasvindas__subtitulo">Troca e conexão para estudantes.</p>

        <p className="boasvindas__descricao">
          Entre com seu e-mail institucional <strong>@unifesp.br</strong> para
          acessar os módulos de Alimentação, Bazar Acadêmico e Ação Solidária.
        </p>

        <div className="boasvindas__login">
          <GoogleLogin
            onSuccess={aoLogarComGoogle}
            onError={aoFalharLoginGoogle}
            text="signin_with"
            shape="pill"
            locale="pt-BR"
          />
        </div>

        {carregando && <p className="boasvindas__status">Entrando…</p>}

        {erro && (
          <p className="boasvindas__erro" role="alert">
            {erro}
          </p>
        )}

        <p className="boasvindas__rodape">
          Acesso restrito à comunidade acadêmica da UNIFESP.
        </p>
      </section>
    </main>
  );
}