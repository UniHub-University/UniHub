import { Routes, Route } from 'react-router-dom';

import PainelVendedor from './pages/PainelVendedor.jsx';
import MeusProdutos from './pages/MeusProdutos.jsx';
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
import Titulo from './components/Titulo.jsx';
import Alimentacao from './pages/Alimentacao.jsx';
import DetalheAlimentacao from './pages/DetalheAlimentacao.jsx';

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
        <Route
          path="/vendedor"
          element={
            <SomenteVendedor>
              <Titulo texto="Painel do vendedor">
                <PainelVendedor />
              </Titulo>
            </SomenteVendedor>
          }
        />
        <Route
          path="/vendedor/produtos"
          element={
            <SomenteVendedor>
              <Titulo texto="Meus produtos">
                <MeusProdutos />
              </Titulo>
            </SomenteVendedor>
          }
        />
        <Route
          path="/vendedor/produtos/novo"
          element={
            <SomenteVendedor>
              <Titulo texto="Novo produto">
                <FormularioBazar />
              </Titulo>
            </SomenteVendedor>
          }
        />
        <Route
          path="/vendedor/produtos/:id/editar"
          element={
            <SomenteVendedor>
              <Titulo texto="Editar produto">
                <FormularioBazar />
              </Titulo>
            </SomenteVendedor>
          }
        />
        <Route path="/home" element={<Titulo texto="Início"><Home /></Titulo>} />
        <Route path="/bazar" element={<Titulo texto="Bazar"><Bazar /></Titulo>} />
        <Route path="/bazar/:id" element={<Titulo texto="Detalhe do item"><DetalheBazar /></Titulo>} />
        <Route path="/produtos" element={<Titulo texto="Buscar produtos"><BuscaProdutos /></Titulo>} />
        <Route path="/pedidos" element={<Titulo texto="Meus pedidos"><HistoricoPedidos /></Titulo>} />
        <Route path="/perfil" element={<Titulo texto="Meu perfil"><Perfil /></Titulo>} />
        <Route path="/alimentacao" element={ <Titulo texto="Alimentação"><Alimentacao /></Titulo>}/>
        <Route path="/alimentacao/:id" element={ <Titulo texto="Detalhe do alimento"><DetalheAlimentacao /></Titulo>}/>
        <Route
          path="/solidariedade"
          element={<Titulo texto="Ação Solidária"><EmBreve titulo="Ação Solidária" /></Titulo>}
        />
      </Route>
    </Routes>
  );
}

export default App;
