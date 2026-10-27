/* ---------------------------------------------------------------------------
   Tela de histórico de pedidos (BRV-89).
   Usa as variáveis de cor e fonte da identidade visual (definidas em index.css).
   --------------------------------------------------------------------------- */

.historico {
  max-width: 760px;
  margin: 0 auto;
  padding: 40px 20px 64px;
}

.historico__topo {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 28px;
}

.historico__voltar {
  font-size: 0.95rem;
  color: var(--cor-tan);
}

.historico__aviso {
  padding: 24px;
  text-align: center;
  background-color: var(--cor-superficie);
  border-radius: var(--raio);
  color: var(--cor-tan);
}

.historico__aviso--erro {
  color: var(--cor-erro);
}

.historico__lista {
  display: flex;
  flex-direction: column;
  gap: 16px;
  margin: 0;
  padding: 0;
  list-style: none;
}

/* ------------------------------ Cartão do pedido ------------------------------ */

.pedido {
  padding: 20px;
  background-color: var(--cor-superficie);
  border-radius: var(--raio);
  box-shadow: var(--sombra);
}

.pedido__cabecalho {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 14px;
}

.pedido__data {
  display: block;
  margin-top: 4px;
  font-size: 0.85rem;
  color: var(--cor-tan);
}

.pedido__status {
  padding: 4px 12px;
  border-radius: 999px;
  font-size: 0.8rem;
  font-weight: 600;
  background-color: var(--cor-creme);
  color: var(--cor-verde);
}

/* 0 Reservado, 1 Confirmado, 2 Concluído, 3 Cancelado */
.pedido__status--2 {
  background-color: var(--cor-verde);
  color: var(--cor-creme);
}

.pedido__status--3 {
  background-color: var(--cor-erro);
  color: var(--cor-creme);
}

.pedido__itens {
  margin: 0;
  padding: 12px 0;
  list-style: none;
  border-top: 1px solid rgba(177, 149, 99, 0.35);
  border-bottom: 1px solid rgba(177, 149, 99, 0.35);
}

.pedido__itens li {
  display: flex;
  justify-content: space-between;
  gap: 12px;
  padding: 4px 0;
  font-size: 0.95rem;
}

.pedido__itens li span:last-child {
  color: var(--cor-tan);
  white-space: nowrap;
}

.pedido__total {
  display: flex;
  justify-content: flex-end;
  gap: 8px;
  margin-top: 12px;
}

/* -------------------------------- Paginação -------------------------------- */

.historico__paginacao {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 16px;
  margin-top: 28px;
}

.historico__paginacao button {
  padding: 8px 18px;
  border: none;
  border-radius: var(--raio);
  background-color: var(--cor-verde);
  color: var(--cor-creme);
  font-weight: 600;
}

.historico__paginacao button:hover:not(:disabled) {
  background-color: var(--cor-verde-escuro);
}

.historico__paginacao button:disabled {
  opacity: 0.4;
  cursor: not-allowed;
}

@media (max-width: 520px) {
  .historico__topo {
    flex-direction: column;
    align-items: flex-start;
  }
}