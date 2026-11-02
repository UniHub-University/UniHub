import { Routes, Route } from 'react-router-dom';

import PainelVendedor from './pages/PainelVendedor.jsx';
import { SomenteVendedor } from './auth/PerfilVendedor.jsx';
import FormularioBazar from './pages/FormularioBazar.jsx';
import BoasVindas from './pages/BoasVindas.jsx';
import DetalheBazar from './pages/DetalheBazar.jsx';
import Home from './pages/Home.jsx';
import Bazar from './pages/Bazar.jsx';
import EmBreve from './pages/EmBreve.jsx';
import Perfil from './pages/Perfil.jsx';
import BuscaProdutos from './pages/BuscaProdutos.jsx';
import HistoricoPedidos from './pages/HistoricoPedidos.jsx';
import LayoutApp from './components/LayoutApp.jsx';
import RotaProtegida from './auth/RotaProtegida.jsx';
import Alimentacao from './pages/Alimentacao.jsx';
import DetalheAlimentacao from './pages/DetalheAlimentacao.jsx';

// Mapa de rotas da aplicação.
// "/"      -> tela inicial de boas-vindas + login (pública) — BRV-116
// Área autenticada: todas as rotas protegidas compartilham o LayoutApp
// (header + navegação + rodapé) — BRV-147/BRV-148.
// "/home"     -> vitrine dos anúncios mais recentes
// "/bazar"    -> feed completo com busca (+ /bazar/:id detalhe)
// "/produtos" -> busca e filtro de produtos — BRV-6
// "/pedidos"  -> histórico de pedidos — BRV-89
// "/perfil"   -> perfil do usuário — BRV-115
// "/vendedor" -> painel do vendedor
// "/vendedor/produtos/novo" -> formulário de criação de produto
// "/vendedor/produtos/:id/editar" -> formulário de edição de produto
// "/alimentacao" -> anúncios de alimentos
// "/solidariedade" -> placeholder "em breve"

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
        <Route path="/vendedor" element={<SomenteVendedor><PainelVendedor /></SomenteVendedor>} />
        <Route path="/home" element={<Home />} />
        <Route path="/vendedor/produtos/novo" element={<SomenteVendedor><FormularioBazar /></SomenteVendedor>} />
        <Route path="/vendedor/produtos/:id/editar" element={<SomenteVendedor><FormularioBazar /></SomenteVendedor>} />
        <Route path="/bazar" element={<Bazar />} />
        <Route path="/bazar/:id" element={<DetalheBazar />} />
        <Route path="/produtos" element={<BuscaProdutos />} />
        <Route path="/pedidos" element={<HistoricoPedidos />} />
        <Route path="/perfil" element={<Perfil />} />
        <Route path="/alimentacao" element={<Alimentacao />} />
        <Route path="/alimentacao/:id" element={<DetalheAlimentacao  />} />
        <Route path="/solidariedade" element={<EmBreve titulo="Ação Solidária" />} />
      </Route>
    </Routes>
  );
}

export default App;