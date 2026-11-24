import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';

import { listarMinhasDenuncias } from '../services/denuncias.js';
import { rotuloStatusDenuncia, rotuloMotivoDenuncia } from '../services/rotulosDenuncia.js';
import { ApiError } from '../services/api.js';
import './MinhasDenuncias.css';

// Tela "Denúncias feitas por mim": lista só-leitura das denúncias que o próprio
// usuário fez, para acompanhar o desfecho (status)
//
// LGPD: o backend devolve só o NOME do denunciado (nunca id) e o nome do produto
// quando a denúncia foi sobre um anúncio. Nada de editar/cancelar aqui.

function formatarData(iso) {
  return new Date(iso).toLocaleDateString('pt-BR', { dateStyle: 'short' });
}

// Classe de cor do selo por status (0..3), mesmo esquema do molde.
function classeStatus(status) {
  return `minhas-denuncias__status minhas-denuncias__status--${status}`;
}

export default function MinhasDenuncias() {
  const [denuncias, setDenuncias] = useState([]);
  const [carregando, setCarregando] = useState(true);
  const [erro, setErro] = useState('');

  useEffect(() => {
    let cancelado = false;

    listarMinhasDenuncias()
      .then((lista) => {
        if (cancelado) return;
        // A resposta é uma lista direta (não paginada).
        setDenuncias(Array.isArray(lista) ? lista : []);
        setErro('');
      })
      .catch((e) => {
        if (cancelado) return;
        setDenuncias([]);
        setErro(e instanceof ApiError ? e.message : 'Não foi possível carregar suas denúncias.');
      })
      .finally(() => {
        if (!cancelado) setCarregando(false);
      });

    return () => {
      cancelado = true;
    };
  }, []);

  return (
    <main className="minhas-denuncias">
      <header className="minhas-denuncias__cabecalho">
        <div>
          <h1 className="minhas-denuncias__titulo">Denúncias feitas por mim</h1>
          <p className="minhas-denuncias__subtitulo">
            Acompanhe o andamento das denúncias que você enviou.
          </p>
        </div>
        <Link className="minhas-denuncias__voltar" to="/perfil">
          ← Voltar ao perfil
        </Link>
      </header>

      {carregando && (
        <p className="minhas-denuncias__status-msg" role="status">Carregando…</p>
      )}

      {erro && !carregando && (
        <p className="minhas-denuncias__erro" role="alert">{erro}</p>
      )}

      {!carregando && !erro && denuncias.length === 0 && (
        <p className="minhas-denuncias__vazio">
          Você ainda não fez nenhuma denúncia.
        </p>
      )}

      {!carregando && !erro && denuncias.length > 0 && (
        <ul className="minhas-denuncias__lista">
          {denuncias.map((d) => (
            <li key={d.id} className="minhas-denuncias__item">
              <div className="minhas-denuncias__item-topo">
                <span className="minhas-denuncias__motivo">
                  {rotuloMotivoDenuncia(d.motivo)}
                </span>
                <span className={classeStatus(d.status)}>
                  {rotuloStatusDenuncia(d.status)}
                </span>
              </div>

              <dl className="minhas-denuncias__alvo">
                <div className="minhas-denuncias__alvo-linha">
                  <dt>Denunciado</dt>
                  <dd>{d.denunciadoNome}</dd>
                </div>
                {d.produtoNome && (
                  <div className="minhas-denuncias__alvo-linha">
                    <dt>Produto</dt>
                    <dd>{d.produtoNome}</dd>
                  </div>
                )}
              </dl>

              <p className="minhas-denuncias__descricao">{d.descricao}</p>

              <div className="minhas-denuncias__item-base">
                <span className="minhas-denuncias__data">{formatarData(d.criadaEm)}</span>
              </div>
            </li>
          ))}
        </ul>
      )}
    </main>
  );
}
