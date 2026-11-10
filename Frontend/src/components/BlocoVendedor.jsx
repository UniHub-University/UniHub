import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { obterVendedorPublico } from '../services/vendedoresPublico.js';
import './BlocoVendedor.css';

const formatadorNota = new Intl.NumberFormat('pt-BR', {
  minimumFractionDigits: 1,
  maximumFractionDigits: 1,
});

// Bloco "Vendedor" dos detalhes de anuncio (Bazar e Alimentacao): nome como link
// para o perfil publico + nota. Foto e PIX ficam fora daqui de proposito.
// Informacao complementar: se falhar ou nao houver vendedorId, nao mostra nada
// (nao deve atrapalhar a compra).
export default function BlocoVendedor({ vendedorId }) {
  if (!vendedorId) return null;
  return <ConsultaVendedor key={vendedorId} vendedorId={vendedorId} />;
}

function ConsultaVendedor({ vendedorId }) {
  const [vendedor, setVendedor] = useState(null);

  useEffect(() => {
    let cancelado = false;

    obterVendedorPublico(vendedorId)
      .then((dados) => {
        if (!cancelado) setVendedor(dados);
      })
      .catch(() => {
        // Silencioso de proposito: o bloco e opcional.
      });

    return () => {
      cancelado = true;
    };
  }, [vendedorId]);

  if (!vendedor) return null;

  const temNota = vendedor.qtdAvaliacoes > 0;

  return (
    <div className="bloco-vendedor">
      <span className="bloco-vendedor__rotulo">Vendedor</span>
      <Link className="bloco-vendedor__nome" to={`/vendedores/${encodeURIComponent(vendedor.id)}`}>
        {vendedor.nome}
      </Link>
      <span className="bloco-vendedor__nota">
        {temNota
          ? `★ ${formatadorNota.format(vendedor.notaMedia)} (${vendedor.qtdAvaliacoes})`
          : 'Sem avaliações'}
      </span>
    </div>
  );
}
