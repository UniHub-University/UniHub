import { useEffect, useState } from 'react';

import { criarDenuncia } from '../services/denuncias.js';
import { OPCOES_MOTIVO_DENUNCIA } from '../services/rotulosDenuncia.js';
import './ModalDenuncia.css';

// Modal reutilizável de denúncia (B1). Abre sobre a tela, com o fundo
// escurecido, e permite denunciar um vendedor escolhendo um motivo e escrevendo
// uma justificativa. É usado em dois lugares:
//  - detalhe do produto (Bazar/Alimentação): passa `vendedorId` + `produtoId`;
//  - perfil público do vendedor: passa só `vendedorId` (sem `produtoId`).
//
// O componente NÃO conhece o UsuarioId do dono (LGPD): envia o `vendedorId` e o
// backend traduz. O denunciante vem do token (não é enviado pela UI).
//
// Props:
//  - vendedorId (obrigatório): GUID do vendedor denunciado.
//  - produtoId  (opcional): GUID do produto, quando a denúncia parte do anúncio.
//  - onFechar   (obrigatório): callback para fechar o modal.

const DESCRICAO_MIN = 10;
const DESCRICAO_MAX = 500;

export default function ModalDenuncia({ vendedorId, produtoId, onFechar }) {
  const [motivo, setMotivo] = useState('');
  const [descricao, setDescricao] = useState('');
  const [enviando, setEnviando] = useState(false);
  const [erro, setErro] = useState('');
  const [sucesso, setSucesso] = useState(false);

  // Fecha o modal com a tecla ESC (padrão de acessibilidade para diálogos).
  useEffect(() => {
    function aoTeclar(evento) {
      if (evento.key === 'Escape') onFechar();
    }
    window.addEventListener('keydown', aoTeclar);
    return () => window.removeEventListener('keydown', aoTeclar);
  }, [onFechar]);

  const descricaoLimpa = descricao.trim();
  const descricaoValida =
    descricaoLimpa.length >= DESCRICAO_MIN && descricaoLimpa.length <= DESCRICAO_MAX;
  const podeEnviar = Boolean(motivo) && descricaoValida && !enviando;

  async function aoEnviar(evento) {
    evento.preventDefault();
    if (!podeEnviar) return;

    setEnviando(true);
    setErro('');

    try {
      await criarDenuncia({
        vendedorId,
        produtoId,
        motivo,
        descricao: descricaoLimpa,
      });
      setSucesso(true);
    } catch (e) {
      // ApiError traz a mensagem amigável do backend (auto-denúncia, motivo
      // inválido, vendedor não encontrado). O status 401 ganha um texto próprio.
      if (e?.status === 401) {
        setErro('Você precisa estar logado para denunciar.');
      } else {
        setErro(e?.message || 'Não foi possível enviar a denúncia. Tente novamente.');
      }
    } finally {
      setEnviando(false);
    }
  }

  // Clique no fundo escurecido fecha; clique dentro do balão não propaga.
  function aoClicarOverlay() {
    onFechar();
  }

  return (
    <div className="modal-denuncia__overlay" onClick={aoClicarOverlay}>
      <div
        className="modal-denuncia"
        role="dialog"
        aria-modal="true"
        aria-labelledby="modal-denuncia-titulo"
        onClick={(evento) => evento.stopPropagation()}
      >
        <button
          type="button"
          className="modal-denuncia__fechar"
          aria-label="Fechar"
          onClick={onFechar}
        >
          ×
        </button>

        {sucesso ? (
          <div className="modal-denuncia__sucesso">
            <h2 id="modal-denuncia-titulo">Denúncia enviada</h2>
            <p>
              Recebemos sua denúncia e ela será analisada pela administração.
              Obrigado por ajudar a manter a comunidade segura.
            </p>
            <button
              type="button"
              className="modal-denuncia__botao modal-denuncia__botao--primario"
              onClick={onFechar}
            >
              Fechar
            </button>
          </div>
        ) : (
          <form className="modal-denuncia__form" onSubmit={aoEnviar}>
            <h2 id="modal-denuncia-titulo">Denunciar</h2>
            <p className="modal-denuncia__ajuda">
              Conte o que aconteceu. Sua denúncia é confidencial e será analisada
              pela administração.
            </p>

            <label className="modal-denuncia__campo">
              <span className="modal-denuncia__rotulo">
                Motivo <span className="modal-denuncia__obrigatorio" aria-hidden="true">*</span>
              </span>
              <select
                className="modal-denuncia__select"
                value={motivo}
                onChange={(evento) => setMotivo(evento.target.value)}
                disabled={enviando}
                required
              >
                <option value="" disabled>
                  Selecione um motivo
                </option>
                {OPCOES_MOTIVO_DENUNCIA.map((opcao) => (
                  <option key={opcao.valor} value={opcao.valor}>
                    {opcao.rotulo}
                  </option>
                ))}
              </select>
            </label>

            <label className="modal-denuncia__campo">
              <span className="modal-denuncia__rotulo">
                Justificativa <span className="modal-denuncia__obrigatorio" aria-hidden="true">*</span>
              </span>
              <textarea
                className="modal-denuncia__textarea"
                value={descricao}
                onChange={(evento) => setDescricao(evento.target.value)}
                placeholder="Descreva sua ocorrência (mínimo de 10 caracteres)."
                maxLength={DESCRICAO_MAX}
                rows={5}
                disabled={enviando}
                required
              />
              <span className="modal-denuncia__contador">
                {descricaoLimpa.length}/{DESCRICAO_MAX}
              </span>
            </label>

            <p className="modal-denuncia__legenda-obrigatorio">
              <span className="modal-denuncia__obrigatorio" aria-hidden="true">*</span> campo obrigatório
            </p>

            {erro && (
              <p className="modal-denuncia__erro" role="alert">
                {erro}
              </p>
            )}

            <div className="modal-denuncia__acoes">
              <button
                type="button"
                className="modal-denuncia__botao modal-denuncia__botao--secundario"
                onClick={onFechar}
                disabled={enviando}
              >
                Cancelar
              </button>
              <button
                type="submit"
                className="modal-denuncia__botao modal-denuncia__botao--primario"
                disabled={!podeEnviar}
              >
                {enviando ? 'Enviando...' : 'Enviar denúncia'}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
