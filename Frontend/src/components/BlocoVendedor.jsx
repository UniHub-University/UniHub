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
export default function BlocoVendedor({ vendedorId, chavePix }) {
  if (!vendedorId && !chavePix) return null;

  return (
    <ConsultaVendedor
      key={vendedorId}
      vendedorId={vendedorId}
      chavePix={chavePix}
    />
  );
}

function ConsultaVendedor({ vendedorId, chavePix }) {
  const [vendedor, setVendedor] = useState(null);
  const [pixCopiado, setPixCopiado] = useState(false);

  useEffect(() => {
    if (!vendedorId) return undefined;

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

  if (!vendedor && !chavePix) return null;

  const temNota = Boolean(vendedor) && vendedor.qtdAvaliacoes > 0;
  const inicial = vendedor?.nome?.charAt(0)?.toUpperCase() || '?';

  return (
    <div className="bloco-vendedor">
      {vendedor && (
        <div className="bloco-vendedor__identidade">
          {vendedor.fotoUrl ? (
            <img
              className="bloco-vendedor__foto"
              src={vendedor.fotoUrl}
              alt={`Foto de ${vendedor.nome}`}
            />
          ) : (
            <span className="bloco-vendedor__avatar" aria-hidden="true">
              {inicial}
            </span>
          )}

          <div className="bloco-vendedor__dados">
            <span className="bloco-vendedor__rotulo">Vendedor</span>
            <Link
              className="bloco-vendedor__nome"
              to={`/vendedores/${encodeURIComponent(vendedor.id)}`}
            >
              {vendedor.nome}
            </Link>
            <span className="bloco-vendedor__nota">
              {temNota
                ? `★ ${formatadorNota.format(vendedor.notaMedia)} (${vendedor.qtdAvaliacoes})`
                : 'Sem avaliações'}
            </span>
          </div>
        </div>
      )}

      {chavePix && (
        <section className="bloco-vendedor__pix" aria-label="Pagamento via PIX">
          <div className="bloco-vendedor__pix-cabecalho">
            <span className="bloco-vendedor__pix-icone" aria-hidden="true">PIX</span>
            <div>
              <h3>Pagamento via PIX</h3>
              <p>Copie a chave e pague pelo app do seu banco.</p>
            </div>
          </div>
          <div className="bloco-vendedor__pix-chave">
            <code>{chavePix}</code>
            <button
              type="button"
              className="bloco-vendedor__pix-copiar"
              onClick={async () => {
                try {
                  await navigator.clipboard.writeText(chavePix);
                  setPixCopiado(true);
                  window.setTimeout(() => setPixCopiado(false), 1800);
                } catch {
                  setPixCopiado(false);
                }
              }}
            >
              {pixCopiado ? 'Copiada' : 'Copiar chave'}
            </button>
          </div>
        </section>
      )}
    </div>
  );
}