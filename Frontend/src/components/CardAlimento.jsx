// Card de um alimento da aba Alimentação.
//
// Componente compartilhado entre a Alimentação (lista completa) e a Home
// (vitrine dos mais recentes), no mesmo espírito do CardAnuncio do Bazar.
// O card inteiro é clicável e leva ao detalhe do alimento (/alimentacao/:id).

import { Link } from 'react-router-dom';

import './CardAlimento.css';

function formatarPreco(preco) {
  return Number(preco).toLocaleString('pt-BR', {
    style: 'currency',
    currency: 'BRL',
  });
}

export default function CardAlimento({ alimento }) {
  const nomeVendedor = alimento.nomeVendedor?.trim();

  return (
    <li className="cartao-alimento">
      {alimento.imagemUrl && (
        <img
          className="cartao-alimento__imagem"
          src={alimento.imagemUrl}
          alt={alimento.nome}
          loading="lazy"
        />
      )}
      <div className="cartao-alimento__conteudo">
        <h2 className="cartao-alimento__nome">
          <Link
            className="cartao-alimento__link"
            to={`/alimentacao/${alimento.id}`}
          >
            {alimento.nome}
          </Link>
        </h2>

        <div className="cartao-alimento__tags">
          {alimento.categoria && (
            <span className="cartao-alimento__categoria">
              {alimento.categoria}
            </span>
          )}
        </div>

        {alimento.descricao && (
          <p className="cartao-alimento__descricao">{alimento.descricao}</p>
        )}

        {alimento.restricoes && alimento.restricoes !== 'null' && (
          <p className="cartao-alimento__restricoes">
            Restrições: {alimento.restricoes}
          </p>
        )}

       <div className="cartao-alimento__vendedor" aria-label={`Vendedor: ${nomeVendedor || 'Vendedor'}`}>
          {alimento.fotoVendedorUrl ? (
            <img
              className="cartao-alimento__foto-vendedor"
              src={alimento.fotoVendedorUrl}
              alt={`Foto de ${nomeVendedor || 'vendedor'}`}
              loading="lazy"
            />
          ) : nomeVendedor ? (
            <span className="cartao-alimento__avatar-vendedor" aria-hidden="true">
              {nomeVendedor.charAt(0).toUpperCase()}
            </span>
          ) : null}
          <span className="cartao-alimento__nome-vendedor">{nomeVendedor || 'Vendedor'}</span>
        </div>
      </div>


      <div className="cartao-alimento__rodape">
        <span className="cartao-alimento__preco">
          {formatarPreco(alimento.preco)}
        </span>

        <span className="cartao-alimento__quantidade">
          {alimento.quantidadeDisponivel}{' '}
          {alimento.quantidadeDisponivel === 1 ? 'unidade' : 'unidades'}
        </span>
      </div>
    </li>
  );
}