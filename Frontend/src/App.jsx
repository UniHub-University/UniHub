import { Routes, Route } from 'react-router-dom';

import BoasVindas from './pages/BoasVindas.jsx';
import DetalheBazar from './pages/DetalheBazar.jsx';
import Home from './pages/Home.jsx';
import EmBreve from './pages/EmBreve.jsx';
import LayoutApp from './components/LayoutApp.jsx';
import RotaProtegida from './auth/RotaProtegida.jsx';

// Mapa de rotas da aplicação.
// "/"      -> tela inicial de boas-vindas + login (pública) — BRV-116 (sua entrega)
//Área autenticada (BRV-147): todas as rotas protegidas compartilham o
// LayoutApp (header + navegação). A Home exibe o feed de anúncios do Bazar;
// as demais áreas ainda não têm tela/endpoint e usam o placeholder "em breve"
// até que o squad as desenvolva.

function App() {
  return (
    <Routes>
      <Route path="/" element={<BoasVindas />} />

      <Route
        element={
          <RotaProtegida>
            <LayoutApp />
          </RotaProtegida>
        }
      >
        <Route path="/home" element={<Home />} />
        <Route path="/alimentacao" element={<EmBreve titulo="Alimentação" />} />
        <Route path="/bazar/:id" element={<DetalheBazar />} />
        <Route
          path="/solidariedade"
          element={<EmBreve titulo="Ação Solidária" />}
        />
        <Route path="/perfil" element={<EmBreve titulo="Perfil" />} />
      </Route>
    </Routes>
  );
}

export default App;