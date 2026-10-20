// Envolve rotas que exigem usuário autenticado. Se não houver JWT, redireciona
// para a tela inicial de boas-vindas (/). Prepara o terreno para as telas de
// Perfil e Alimentação que os colegas de squad vão desenvolver.

import { Navigate } from 'react-router-dom';
import { useAuth } from './AuthContext.jsx';

export default function RotaProtegida({ children }) {
  const { estaAutenticado } = useAuth();

  if (!estaAutenticado) {
    return <Navigate to="/" replace />;
  }

  return children;
}