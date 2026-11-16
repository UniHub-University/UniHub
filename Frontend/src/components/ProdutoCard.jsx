import './ProdutoCard.css';
import { Link } from 'react-router-dom';

const formatadorPreco = new Intl.NumberFormat('pt-BR', {
  style: 'currency',
  currency: 'BRL',
});

// Cartão de um produto na listagem de busca. Recebe o ProdutoResumoDto já
// desserializado (camelCase do backend .NET).
export default function ProdutoCard({ produto }) {
  const {
    nome,
    descricao,
    preco,
    tipo,
    nomeVendedor,
    fotoVendedorUrl,
    vendedorId,
    campi = [],
    quantidadeDisponivel,
  } = produto;
  const nomeVendedorExibido = nomeVendedor?.trim();


  return (
    <article className="produto-card">
      <span className={`produto-card__tipo produto-card__tipo--${tipo}`}>
        {tipo === 'lanche' ? 'Lanche' : 'Bazar'}
      </span>

      <h3 className="produto-card__nome">{nome}</h3>

      {descricao && <p className="produto-card__descricao">{descricao}</p>}

      <p className="produto-card__preco">{formatadorPreco.format(preco)}</p>

       <dl className="produto-card__detalhes">
        <div className="produto-card__detalhe">
          <dt>Vendedor</dt>
          <dd>
            <span className="produto-card__vendedor-identidade">
              {fotoVendedorUrl ? (
                <img
                  className="produto-card__foto-vendedor"
                  src={fotoVendedorUrl}
                  alt={`Foto de ${nomeVendedorExibido || 'vendedor'}`}
                  loading="lazy"
                />
              ) : nomeVendedorExibido ? (
                <span className="produto-card__avatar-vendedor" aria-hidden="true">
                  {nomeVendedorExibido.charAt(0).toUpperCase()}
                </span>
              ) : null}
              {vendedorId ? (
                <Link className="produto-card__vendedor" to={`/vendedores/${encodeURIComponent(vendedorId)}`}>
                  {nomeVendedorExibido || 'Vendedor'}
                </Link>
              ) : (
                <span>{nomeVendedorExibido || 'Vendedor'}</span>
              )}
            </span>
          </dd>
        </div>
        {campi.length > 0 && (
          <div className="produto-card__detalhe">
            <dt>Campus</dt>
            <dd>{campi.join(', ')}</dd>
          </div>
        )}
        <div className="produto-card__detalhe">
          <dt>Disponível</dt>
          <dd>{quantidadeDisponivel}</dd>
        </div>
      </dl>
      {tipo === 'bazar' && <Link to={`/bazar/${encodeURIComponent(produto.id)}`} aria-label={`Ver anúncio: ${nome}`}>Ver anúncio →</Link>}
      {tipo === 'lanche' && <Link to={`/alimentacao/${encodeURIComponent(produto.id)}`} aria-label={`Ver anúncio: ${nome}`}>Ver anúncio →</Link>}
    </article>
  );
}