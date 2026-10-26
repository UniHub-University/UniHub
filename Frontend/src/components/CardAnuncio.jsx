// Card de um anúncio do Bazar (BRV-148).
//
// Componente compartilhado entre a Home (pré-visualização dos mais recentes) e
// a aba Bazar (feed completo), para o visual do card ficar num lugar só.
// O card inteiro é clicável e leva ao detalhe do anúncio (/bazar/:id).

import { Link } from 'react-router-dom';

import { rotuloStatus, rotuloCondicao } from '../services/rotulosProduto.js';
import './CardAnuncio.css';

const formatadorMoeda = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

function formatarPreco(valor) {
  if (typeof valor !== 'number' || Number.isNaN(valor)) return '—';
  return formatadorMoeda.format(valor);
}

export default function CardAnuncio({ item }) {
  const condicao = rotuloCondicao(item.condicao);

  return (
    <li className="cartao-anuncio">
      <div className="cartao-anuncio__topo">
        <h2 className="cartao-anuncio__nome">
          <Link className="cartao-anuncio__link" to={`/bazar/${item.id}`}>
            {item.nome}
          </Link>
        </h2>
        <div className="cartao-anuncio__tags">
          {item.categoria && (
            <span className="cartao-anuncio__categoria">{item.categoria}</span>
          )}
          {condicao && (
            <span className="cartao-anuncio__condicao">{condicao}</span>
          )}
        </div>
      </div>

      {item.descricao && (
        <p className="cartao-anuncio__descricao">{item.descricao}</p>
      )}

      <div className="cartao-anuncio__rodape">
        <span className="cartao-anuncio__preco">{formatarPreco(item.preco)}</span>
        <span className="cartao-anuncio__status">{rotuloStatus(item.status)}</span>
      </div>
    </li>
  );
}
