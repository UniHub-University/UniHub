import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { BrowserRouter } from 'react-router-dom';
import { GoogleOAuthProvider } from '@react-oauth/google';

import App from './App.jsx';
import { AuthProvider } from './auth/AuthContext.jsx';
import { GOOGLE_CLIENT_ID } from './config/env.js';
import './index.css';

// Hierarquia de providers:
//  GoogleOAuthProvider -> habilita o componente de login do Google em toda a app
//  BrowserRouter       -> roteamento SPA
//  AuthProvider        -> estado global de autenticação (JWT do UniHub)
createRoot(document.getElementById('root')).render(
  <StrictMode>
    <GoogleOAuthProvider clientId={GOOGLE_CLIENT_ID}>
      <BrowserRouter>
        <AuthProvider>
          <App />
        </AuthProvider>
      </BrowserRouter>
    </GoogleOAuthProvider>
  </StrictMode>,
);