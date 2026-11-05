// Placeholder temporário das telas do módulo de Ação Solidária ainda em
// desenvolvimento. Cada dupla substitui a rota pelo componente real no App.jsx.
// Serve só para a navegação não quebrar enquanto as telas não existem.

import { Link } from 'react-router-dom';

export default function EmConstrucao({ titulo = 'Em construção' }) {
  return (
    <main style={{ maxWidth: 820, margin: '0 auto', padding: '48px 20px' }}>
      <h1 style={{ fontFamily: 'var(--fonte-titulo)', color: 'var(--cor-verde)' }}>
        {titulo}
      </h1>
      <p style={{ color: 'var(--cor-tan)', lineHeight: 1.6 }}>
        Esta tela do módulo de Ação Solidária está em desenvolvimento.
      </p>
      <Link to="/solidariedade" style={{ color: 'var(--cor-verde)', fontWeight: 600 }}>
        ← Voltar à Ação Solidária
      </Link>
    </main>
  );
}