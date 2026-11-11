import { useState } from 'react';
import { useNavigate } from 'react-router-dom';

import './BuscaAnuncios.css';

// Textos por tipo de anúncio. O tipo é o mesmo que a tela /produtos entende
// ("bazar" ou "lanche"), então o encaminhamento já chega com a categoria marcada.
const TEXTOS = {
  bazar: {
    formulario: 'Pesquisar no Bazar',
    campo: 'Buscar anúncios',
    placeholder: 'Buscar anúncios…',
  },
  lanche: {
    formulario: 'Pesquisar em Alimentação',
    campo: 'Buscar alimentos',
    placeholder: 'Buscar alimentos…',
  },
};

// Caixa de pesquisa das abas Bazar e Alimentação: fica igual nas duas e leva o
// termo para a tela unificada /produtos (preço, campus, dia e horário).
export default function BuscaAnuncios({ tipo }) {
  const navigate = useNavigate();
  const [busca, setBusca] = useState('');
  const textos = TEXTOS[tipo] ?? TEXTOS.bazar;

  function pesquisar(evento) {
    evento.preventDefault();
    const parametros = new URLSearchParams({ tipo });
    if (busca.trim()) parametros.set('termo', busca.trim());
    navigate(`/produtos?${parametros.toString()}`);
  }

  return (
    <form className="busca-anuncios" onSubmit={pesquisar} role="search" aria-label={textos.formulario}>
      <input
        type="search"
        className="busca-anuncios__campo"
        placeholder={textos.placeholder}
        value={busca}
        onChange={(evento) => setBusca(evento.target.value)}
        aria-label={textos.campo}
      />
      <button className="busca-anuncios__botao" type="submit">Pesquisar</button>
    </form>
  );
}
