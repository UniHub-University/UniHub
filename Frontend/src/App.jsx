import { Routes, Route } from 'react-router-dom';

import BoasVindas from './pages/BoasVindas.jsx';
import HistoricoPedidos from './pages/HistoricoPedidos.jsx';
import RotaProtegida from './auth/RotaProtegida.jsx';

// Mapa de rotas da aplicação.
// "/"      -> tela inicial de boas-vindas + login (pública) — BRV-116 (sua entrega)
// "/pedidos" -> histórico de pedidos do usuário (protegida) — BRV-89
// "/home"  -> tela principal da plataforma (protegida). Placeholder por enquanto;
//             a tela real (anúncios, módulos) será criada por outra pessoa do squad.
function App() {
  return (
    <Routes>
      <Route path="/" element={<BoasVindas />} />

      <Route
        path="/pedidos"
        element={
          <RotaProtegida>
            <HistoricoPedidos />
          </RotaProtegida>
        }
      />

      <Route
        path="/home"
        element={
          <RotaProtegida>
            <PlaceholderHome />
          </RotaProtegida>
        }
      />
    </Routes>
  );
}

// Placeholder temporário da tela principal. Serve apenas para o fluxo de login
// ter um destino válido enquanto a tela real (home/feed) não é desenvolvida.
// Quem assumir a home substitui este componente pela página de verdade.
function PlaceholderHome() {
  return (
    <main style={{ padding: '48px', fontFamily: 'var(--fonte-corpo)' }}>
      <h1>Bem-vindo ao UniHub 🎉</h1>
      <p style={{ color: 'var(--cor-tan)' }}>
        Login realizado com sucesso. Esta é a tela principal (placeholder) — em
        breve ela exibirá os anúncios e os módulos da plataforma.
      </p>
    </main>
  );
}

export default App;