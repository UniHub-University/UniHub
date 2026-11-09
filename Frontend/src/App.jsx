import { Routes, Route } from 'react-router-dom';

import PainelVendedor from './pages/PainelVendedor.jsx';
import MeusProdutos from './pages/MeusProdutos.jsx';
import PedidosVendedor from './pages/PedidosVendedor.jsx';
import { SomenteVendedor } from './auth/PerfilVendedor.jsx';
import FormularioBazar from './pages/FormularioBazar.jsx';
import FormularioAlimentacao from './pages/FormularioAlimentacao.jsx';
import BoasVindas from './pages/BoasVindas.jsx';
import DetalheBazar from './pages/DetalheBazar.jsx';
import Home from './pages/Home.jsx';
import Bazar from './pages/Bazar.jsx';
import Perfil from './pages/Perfil.jsx';
import BuscaProdutos from './pages/BuscaProdutos.jsx';
import PaginaVendedor from './pages/PaginaVendedor.jsx';
import HistoricoPedidos from './pages/HistoricoPedidos.jsx';
import LayoutApp from './components/LayoutApp.jsx';
import RotaProtegida from './auth/RotaProtegida.jsx';
import Titulo from './components/Titulo.jsx';
import Alimentacao from './pages/Alimentacao.jsx';
import DetalheAlimentacao from './pages/DetalheAlimentacao.jsx';
import SolidariedadeHub from './pages/solidariedade/SolidariedadeHub.jsx';
import EmConstrucao from './pages/solidariedade/EmConstrucao.jsx';
import AreaVoluntario from './pages/solidariedade/voluntario/AreaVoluntario.jsx';

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
        <Route
          path="/vendedor/alimentos/novo"
          element={
            <SomenteVendedor>
              <Titulo texto="Novo alimento">
                <FormularioAlimentacao />
              </Titulo>
            </SomenteVendedor>
          }
        />
        <Route
          path="/vendedor/alimentos/:id/editar"
          element={
            <SomenteVendedor>
              <Titulo texto="Editar alimento">
                <FormularioAlimentacao />
              </Titulo>
            </SomenteVendedor>
          }
        />
        <Route
          path="/vendedor/pedidos"
          element={
            <SomenteVendedor>
              <Titulo texto="Pedidos recebidos">
                <PedidosVendedor />
              </Titulo>
            </SomenteVendedor>
          }
        />
        <Route path="/home" element={<Titulo texto="Início"><Home /></Titulo>} />
        <Route path="/bazar" element={<Titulo texto="Bazar"><Bazar /></Titulo>} />
        <Route path="/bazar/:id" element={<Titulo texto="Detalhe do item"><DetalheBazar /></Titulo>} />
        <Route path="/produtos" element={<Titulo texto="Buscar produtos"><BuscaProdutos /></Titulo>} />
        <Route path="/vendedores/:id" element={<Titulo texto="Vendedor"><PaginaVendedor /></Titulo>} />
        <Route path="/pedidos" element={<Titulo texto="Meus pedidos"><HistoricoPedidos /></Titulo>} />
        <Route path="/perfil" element={<Titulo texto="Meu perfil"><Perfil /></Titulo>} />
        <Route path="/alimentacao" element={ <Titulo texto="Alimentação"><Alimentacao /></Titulo>}/>
        <Route path="/alimentacao/:id" element={ <Titulo texto="Detalhe do alimento"><DetalheAlimentacao /></Titulo>}/>
        {/* ---------- Módulo de Ação Solidária (Frente 1, Sprint 5) ---------- */}
        {/* Hub compartilhado + sub-rotas. As telas marcadas com <EmConstrucao>
            serão substituídas pelos componentes reais:
            - Lado SOLICITANTE (Lucas): /solidariedade/solicitar, /solidariedade/minhas
            - Lado VOLUNTÁRIO (Gabriel): /solidariedade/voluntario, /sugestoes, /doacoes */}
        <Route
          path="/solidariedade"
          element={<Titulo texto="Ação Solidária"><SolidariedadeHub /></Titulo>}
        />
        <Route
          path="/solidariedade/solicitar"
          element={<Titulo texto="Pedir apoio"><EmConstrucao titulo="Pedir apoio" /></Titulo>}
        />
        <Route
          path="/solidariedade/minhas"
          element={<Titulo texto="Minhas solicitações"><EmConstrucao titulo="Minhas solicitações" /></Titulo>}
        />
        <Route
          path="/solidariedade/voluntario"
          element={<Titulo texto="Área do voluntário"><AreaVoluntario /></Titulo>}
        />
        <Route
          path="/solidariedade/sugestoes"
          element={<Titulo texto="Sugestões"><EmConstrucao titulo="Sugestões para você" /></Titulo>}
        />
        <Route
          path="/solidariedade/doacoes"
          element={<Titulo texto="Minhas doações"><EmConstrucao titulo="Minhas doações" /></Titulo>}
        />
      </Route>
    </Routes>
  );
}

export default App;
