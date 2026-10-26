/// Tela provisória enquanto as interfaces dos módulos são integradas.

export default function EmBreve({ titulo }) {
  if (titulo === 'Alimentação') {
    return (
      <section
        aria-labelledby="titulo-alimentacao"
        style={{
          maxWidth: '900px',
          margin: '0 auto',
          textAlign: 'left',
          fontFamily: 'var(--fonte-corpo)',
        }}
      >
        <h1
          id="titulo-alimentacao"
          style={{
            fontFamily: 'var(--fonte-corpo)',
            fontSize: '2rem',
            letterSpacing: '-0.04em',
            marginBottom: '12px',
          }}
        >
          Alimentação
        </h1>

        <p style={{ color: '#596452', lineHeight: 1.6 }}>
          Um espaço para encontrar lanches, doces e bebidas
          oferecidos pela comunidade universitária.
        </p>

        <div
          style={{
            marginTop: '28px',
            padding: 'clamp(20px, 4vw, 36px)',
            background: 'var(--cor-superficie)',
            border: '1px solid #e2dfd2',
            borderRadius: '18px',
            boxShadow: '0 4px 16px rgba(35, 51, 30, 0.04)',
          }}
        >
          <h2
            style={{
              fontFamily: 'var(--fonte-corpo)',
              fontSize: '1.2rem',
              marginBottom: '12px',
            }}
          >
            Catálogo em preparação
          </h2>
          <p style={{ margin: 0, color: '#596452', lineHeight: 1.7 }}>
            A listagem de alimentos ainda está sendo integrada.
            Os produtos aparecerão aqui quando essa etapa estiver concluída.
          </p>
        </div>
      </section>
    );
  }

  return (
    <section style={{ textAlign: 'center', padding: '56px 16px' }}>
      <h1 style={{ fontSize: '2rem', marginBottom: '12px' }}>{titulo}</h1>
      <p style={{ color: 'var(--cor-tan)', fontSize: '1rem' }}>
        Esta área está em desenvolvimento e estará disponível em breve.
      </p>
    </section>
  );
}