// Componente utilitário que apenas define o título da aba e renderiza a página.
//
// Uso no App.jsx, envolvendo cada rota:
//   <Route path="/bazar" element={<Titulo texto="Bazar"><Bazar /></Titulo>} />
//
// Centraliza os títulos no mapa de rotas, sem precisar editar cada página.
// Passar `texto` vazio/ausente usa o título de marca (ver useTituloPagina).

import { useTituloPagina } from '../hooks/useTituloPagina.js';

export default function Titulo({ texto, children }) {
  useTituloPagina(texto);
  return children;
}